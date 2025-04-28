using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.ComponentModel;

public class PlayerController : MonoBehaviour {
	private PlayerUIController uiController;

	private CapsuleCollider capsuleCollider;
	private Rigidbody rb;

	private Vector3 mv;
	private Vector3 lastMV;

	private InputAction sprintAction;
	private InputAction jumpAction;
	private InputAction crouchAction;

	public CameraController cameraController;

	public int score { get; private set; } = 0;
	private float CenterToEdgeDistance => capsuleCollider.height / 2 * transform.localScale.y - capsuleCollider.radius;

	[Header("Movement Settings")]
	[DefaultValue(50f)] public float speed;
	[DefaultValue(7f)] public float maxWalkSpeed;
	[DefaultValue(4f)] public float frictionSpeed;

	[DefaultValue(0.5f)] public float airControl;
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

	[Header("Jump Settings")]
	[DefaultValue(10.5f)] public float jumpForce;
	[DefaultValue(0.25f)] public float jumpStaminaCost;
	[DefaultValue(1.5f)] public float fallMultiplier;
	[DefaultValue(0.75f)] public float lowJumpMultiplier;
	private bool CanJump => isGrounded && !IsDashing && Stamina >= jumpStaminaCost;

	[Header("Crouch Settings")]
	[DefaultValue(2f)] public float crouchHeightMultiplier;
	[DefaultValue(4f)] public float maxCrouchSpeed;
	[DefaultValue(10f)] public float crouchJumpForce;
	private Vector3 originalScale;
	private float crouchHeightDifference;
	private bool isCrouching = false;

	[Header("Sprint Settings")]
	[DefaultValue(12f)] public float maxSprintSpeed;
	[DefaultValue(0.25f)] public float sprintCooldown;
	[DefaultValue(1f)] public float sprintStaminaCost;

	private bool sprinting = false;
	private float sprintEnd = -Mathf.Infinity;

	private bool CanSprint => Time.time - sprintEnd >= sprintCooldown && Stamina > 0 && !isCrouching && !IsDashing;

	[Header("Dash Settings")]
	[DefaultValue(1f)] public float dashCooldown;
	[DefaultValue(0.2f)] public float dashDuration;
	[DefaultValue(20f)] public float dashSpeed;
	[DefaultValue(0.5f)] public float dashStaminaCost;

	private Vector3 dashDirection;
	public float dashStart { get; private set; } = -Mathf.Infinity;

	private bool IsDashing => Time.time - dashStart < dashDuration;
	private bool CanDash => Time.time - (dashStart + dashDuration) >= dashCooldown && Stamina >= dashStaminaCost;

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
		if (GameManager.Instance.IsPaused || !CanDash) return;
		uiController.SetAlphaTarget(1f);

		Stamina -= dashStaminaCost;
		dashStart = Time.time;
		dashDirection = cameraController.TransformMovement(mv == Vector3.zero ? lastMV : mv);

		Vector3 targetLV = dashDirection * dashSpeed;
		rb.linearVelocity = new Vector3(targetLV.x, rb.linearVelocity.y, targetLV.z);
	}

	void OnJump() {
		if (GameManager.Instance.IsPaused || !CanJump) return;

		Stamina -= jumpStaminaCost;
		rb.AddForce(Vector3.up * (isCrouching ? crouchJumpForce : jumpForce), ForceMode.Impulse);
	}

	void OnCrouchPress(InputAction.CallbackContext context) {
		if (GameManager.Instance.IsPaused || isCrouching) return;
		transform.localScale = new Vector3(originalScale.x, originalScale.y / crouchHeightMultiplier, originalScale.z);
		transform.position -= new Vector3(0, crouchHeightDifference, 0);
		isCrouching = true;
	}

	void OnCrouchRelease(InputAction.CallbackContext context) {
		if (GameManager.Instance.IsPaused || !isCrouching) return;
		transform.localScale = originalScale;
		transform.position += new Vector3(0, crouchHeightDifference, 0);
		isCrouching = false;
	}

	/* -------------------------------------------------------------------------- */
	/*                                Unity events                                */
	/* -------------------------------------------------------------------------- */
	void Start() {
		uiController = GetComponent<PlayerUIController>();

		rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
		capsuleCollider = GetComponent<CapsuleCollider>();

		originalScale = transform.localScale;
		var h = 2 * originalScale.y;
		crouchHeightDifference = (h - (h / crouchHeightMultiplier)) / 2f;

		jumpAction = InputSystem.actions.FindAction("Jump");
		crouchAction = InputSystem.actions.FindAction("Crouch");
		sprintAction = InputSystem.actions.FindAction("Sprint");

		crouchAction.started += OnCrouchPress;
		crouchAction.canceled += OnCrouchRelease;
	}

	void Update() {
		UpdateGrounded();

		Vector3 test = cameraController.TransformMovement(Vector3.forward);
		Debug.DrawLine(transform.position, transform.position + test, Color.red);
	}

	private bool onLedge = false;

	private void CheckLedges(Vector3 tmv) {
		// if (rb.linearVelocity.y >= 0 || onLedge) return;
		if (Physics.SphereCast(transform.position + (Vector3.up * CenterToEdgeDistance), capsuleCollider.radius, tmv, out RaycastHit hit, capsuleCollider.radius * 2f)) {
			Debug.Log($"Hit ledge: {hit.collider.name}");
		}
	}

	void FixedUpdate() {
		ApplyGravity();

		/* --------------------------------- Falling -------------------------------- */
		if (!onSlope && rb.linearVelocity.y < 0) {
			rb.AddForce(fallMultiplier * Physics.gravity, ForceMode.Acceleration);
		}

		if (!onSlope && rb.linearVelocity.y > 0 && !jumpAction.IsPressed()) {
			rb.AddForce(lowJumpMultiplier * Physics.gravity, ForceMode.Acceleration);
		}

		/* --------------------------------- Dashing -------------------------------- */
		if (IsDashing) return;

		/* --------------------------- Sprinting + Stamina -------------------------- */
		float maxSpeed = isCrouching ? maxCrouchSpeed : maxWalkSpeed;
		if (sprintAction.IsPressed() && CanSprint && mv != Vector3.zero) {
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
		if (mv == Vector3.zero) return;

		Vector3 v1 = rb.linearVelocity;
		v1.y = 0;
		Vector3 v2 = cameraController.TransformMovement(mv * speed);

		CheckLedges(v2);

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
		// Debug.Log(v1.magnitude);
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Pickup")) {
			score += 1;
			uiController.RefreshScoreText();
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

		groundHitSpeedMultiplier = Mathf.MoveTowards(groundHitSpeedMultiplier, 1f, Time.deltaTime * 1.5f);
		if (!old && isGrounded) {
			// Hit the ground
			groundHitSpeedMultiplier = 1f - airTime;
			airTime = 0f;
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
