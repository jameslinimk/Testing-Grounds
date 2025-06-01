using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.ComponentModel;

public class PlayerController : MonoBehaviour {
	private PlayerUIController uiController;
	private PlayerHealthController healthController;

	private CapsuleCollider capsuleCollider;
	private Rigidbody rb;
	private Animator animator;

	private Vector3 mv;
	private Vector3 lastMV = Vector3.forward;

	private InputAction sprintAction;
	private InputAction crouchAction;

	public CameraController cameraController;

	private float CenterToEdgeDistance => capsuleCollider.height / 2 * transform.localScale.y - capsuleCollider.radius;

	[Header("Movement Settings")]
	[DefaultValue(55f)] public float walkAccel;
	[DefaultValue(5.5f)] public float maxWalkSpeed;

	[DefaultValue(0.7f)] public float airControl;
	[DefaultValue(0.2f)] public float groundHitDashDelay;
	private float landedTime = -Mathf.Infinity;
	private bool IsLanding => Time.time - landedTime < groundHitDashDelay;

	[DefaultValue(0.75f)] public float groundHitRecoverSpeed;
	[Tooltip("Changes max seconds in air scale")][DefaultValue(2f)] public float groundHitCurveMaxTime;
	[Tooltip("F(seconds in air)=% of movement lost")] public AnimationCurve groundHitCurve;
	private float groundHitSpeedMultiplier = 1f;
	private float airTime = 0f;
	private bool isGrounded = true;

	[Header("Stamina Settings")]
	[DefaultValue(1f)] public float staminaRegenCooldown;
	[DefaultValue(5f)] public float maxStamina;
	[DefaultValue(1f)] public float staminaRegen;
	[DefaultValue(2f)] public float standingStaminaRegen;

	private float lastStaminaDrain = -Mathf.Infinity;
	private float _stamina = 5f;
	public float Stamina {
		get => _stamina;
		private set {
			if (value < _stamina) lastStaminaDrain = Time.time;
			_stamina = Math.Clamp(value, 0f, maxStamina);
		}
	}

	private bool CanRegen => Time.time - lastStaminaDrain >= staminaRegenCooldown && Stamina < maxStamina;

	[Header("Crouch Settings")]
	[DefaultValue(2f)] public float crouchHeightMultiplier;
	[DefaultValue(45f)] public float crouchAccel;
	[DefaultValue(4f)] public float maxCrouchSpeed;
	private Vector3 originalScale;
	private float crouchHeightDifference;
	private bool isCrouching = false;

	[Header("Sprint Settings")]
	[DefaultValue(77f)] public float sprintAccel;
	[DefaultValue(7f)] public float maxSprintSpeed;
	[DefaultValue(0.25f)] public float sprintCooldown;
	[DefaultValue(1f)] public float sprintStaminaCost;

	private bool sprinting = false;
	private float sprintEnd = -Mathf.Infinity;

	private bool CanSprint => Time.time - sprintEnd >= sprintCooldown && Stamina > 0 && !isCrouching && !IsDashing;

	[Header("Dash Settings")]
	[DefaultValue(1f)] public float dashCooldown;
	[DefaultValue(1.1f)] public float dashDuration;
	public AnimationCurve dashCurve;
	[DefaultValue(10f)] public float dashSpeed;
	[DefaultValue(0.5f)] public float dashStaminaCost;

	private Vector3 dashDirection;
	public float DashStart { get; private set; } = -Mathf.Infinity;

	[HideInInspector] public bool IsDashing => Time.time - DashStart < dashDuration;
	private bool CanDash => Time.time - (DashStart + dashDuration) >= dashCooldown && Stamina >= dashStaminaCost && isGrounded && !IsLanding;

	[Header("Slope Settings")]
	[DefaultValue(45f)] public float maxSlopeAngle;
	private RaycastHit slopeHit;
	private bool onSlope;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	/* -------------------------------------------------------------------------- */
	/*                                Input events                                */
	/* -------------------------------------------------------------------------- */
	void OnMove(InputValue inputValue) {
		if (GameManager.Instance.IsPaused) return;
		Vector2 inputVector = inputValue.Get<Vector2>();
		mv = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

		if (mv != Vector3.zero) lastMV = mv;
	}

	void OnDash() {
		if (GameManager.Instance.IsPaused || healthController.isDead || !CanDash) return;
		uiController.SetAlphaTarget(1f);

		Stamina -= dashStaminaCost;
		DashStart = Time.time;
		Vector3 rawDir = mv == Vector3.zero ? lastMV : mv;
		dashDirection = cameraController.TransformMovement(rawDir);

		transform.forward = dashDirection;
		animator.SetTrigger("Dash");
	}

	void OnCrouchPress(InputAction.CallbackContext context) {
		if (GameManager.Instance.IsPaused || healthController.isDead || isCrouching) return;
		transform.localScale = new Vector3(originalScale.x, originalScale.y / crouchHeightMultiplier, originalScale.z);
		transform.position -= new Vector3(0, crouchHeightDifference, 0);
		isCrouching = true;
	}

	void OnCrouchRelease(InputAction.CallbackContext context) {
		if (GameManager.Instance.IsPaused || healthController.isDead || !isCrouching) return;
		transform.localScale = originalScale;
		transform.position += new Vector3(0, crouchHeightDifference, 0);
		isCrouching = false;
	}

	/* -------------------------------------------------------------------------- */
	/*                                Unity events                                */
	/* -------------------------------------------------------------------------- */
	void Start() {
		uiController = GetComponent<PlayerUIController>();
		healthController = GetComponent<PlayerHealthController>();

		rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
		capsuleCollider = GetComponent<CapsuleCollider>();

		animator = transform.GetChild(0).GetComponent<Animator>();

		originalScale = transform.localScale;
		var h = 2 * originalScale.y;
		crouchHeightDifference = (h - (h / crouchHeightMultiplier)) / 2f;

		crouchAction = InputSystem.actions.FindAction("Crouch");
		sprintAction = InputSystem.actions.FindAction("Sprint");

		crouchAction.started += OnCrouchPress;
		crouchAction.canceled += OnCrouchRelease;
	}

	void Update() {
		if (healthController.isDead) return;

		UpdateGrounded();

		if (!IsDashing) {
			Vector3 forward = cameraController.TransformMovement(Vector3.forward);
			transform.forward = Vector3.Slerp(transform.forward, forward, Time.deltaTime * 10f);
		}

		animator.SetBool("IsCrouching", isCrouching);
	}

	void FixedUpdate() {
		if (healthController.isDead) return;

		ApplyGravity();

		/* --------------------------------- Dashing -------------------------------- */
		if (IsDashing) {
			float t = (Time.time - DashStart) / dashDuration;
			Vector3 targetLV = dashCurve.Evaluate(t) * dashSpeed * dashDirection;
			rb.linearVelocity = new Vector3(targetLV.x, rb.linearVelocity.y, targetLV.z);

			return;
		}

		/* --------------------------- Sprinting + Stamina -------------------------- */
		float accel = isCrouching ? crouchAccel : walkAccel;
		float maxSpeed = isCrouching ? maxCrouchSpeed : maxWalkSpeed;
		if (sprintAction.IsPressed() && CanSprint && mv != Vector3.zero) {
			accel = sprintAccel;
			maxSpeed = maxSprintSpeed;
			Stamina -= sprintStaminaCost * Time.deltaTime;
			sprinting = true;
		} else if (sprinting) {
			sprinting = false;
			sprintEnd = Time.time;
		} else if (CanRegen) {
			Stamina += (rb.linearVelocity == Vector3.zero ? standingStaminaRegen : staminaRegen) * Time.deltaTime;
		}

		/* ---------------------------- General movement ---------------------------- */
		maxSpeed *= groundHitSpeedMultiplier;
		if (!isGrounded) maxSpeed *= airControl;

		animator.SetFloat("Horizontal", mv.normalized.x, 0.1f, Time.deltaTime);
		animator.SetFloat("Vertical", mv.normalized.z, 0.1f, Time.deltaTime);

		// Animator speed: 0 = idle, 1 = walking, 2 = sprinting
		animator.SetInteger("MovementState", sprinting ? 2 : (mv.magnitude > 0.2f ? 1 : 0));

		if (mv == Vector3.zero) return;

		Vector3 v1 = rb.linearVelocity;
		v1.y = 0;
		Vector3 v2 = cameraController.TransformMovement(mv * accel);

		if (Mathf.Abs(v1.magnitude - maxSpeed) < 0.1f) {
			Vector3 maxV1 = v1.normalized * maxSpeed;
			rb.linearVelocity = new Vector3(maxV1.x, rb.linearVelocity.y, maxV1.z);
		}

		// Normal movement
		if (v1.magnitude < maxSpeed) {
			AddForceSlope(v2, ForceMode.Force);
			Vector3 clampedV1 = Vector3.ClampMagnitude(v1, maxSpeed);
			rb.linearVelocity = new Vector3(clampedV1.x, rb.linearVelocity.y, clampedV1.z);
			return;
		}

		// Speed control
		Vector3 vParallel = Vector3.Project(v2, v1);
		Vector3 vPerp = v2 - vParallel;

		if (Vector3.Dot(vParallel, v1) < 0) {
			AddForceSlope(vParallel, ForceMode.Force);
		}

		AddForceSlope(vPerp, ForceMode.Force);
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Pickup")) {
			Destroy(other.gameObject);
		}
	}

	/* -------------------------------------------------------------------------- */
	/*                                    Other                                   */
	/* -------------------------------------------------------------------------- */
	private void ApplyGravity() {
		Vector3 currentGravity = onSlope ? -slopeHit.normal * Physics.gravity.magnitude : Physics.gravity;
		rb.AddForce(currentGravity, ForceMode.Acceleration);
	}

	private void UpdateGrounded() {
		bool old = isGrounded;
		isGrounded = Physics.SphereCast(transform.position, capsuleCollider.radius, Vector3.down, out slopeHit, CenterToEdgeDistance + 0.1f);

		groundHitSpeedMultiplier = Mathf.MoveTowards(groundHitSpeedMultiplier, 1f, Time.deltaTime * groundHitRecoverSpeed);
		if (!old && isGrounded) {
			// Hit the ground
			groundHitSpeedMultiplier = Mathf.Clamp01(1f - groundHitCurve.Evaluate(airTime / groundHitCurveMaxTime));
			airTime = 0f;
			landedTime = Time.time;
		}
		if (!isGrounded) airTime += Time.deltaTime;

		if (isGrounded) {
			float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
			onSlope = angle > 0 && angle < maxSlopeAngle;
		} else {
			onSlope = false;
		}
	}

	private void AddForceSlope(Vector3 force, ForceMode forceMode = ForceMode.Force) {
		if (!onSlope) {
			rb.AddForce(force, forceMode);
			return;
		}

		Vector3 forceOnSlope = Vector3.ProjectOnPlane(force, slopeHit.normal);//.normalized * force.magnitude;
		rb.AddForce(forceOnSlope, forceMode);
	}
}
