using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.ComponentModel;
using System.Reflection;

public class PlayerController : MonoBehaviour {
	private PlayerHealthController healthController;
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

	[Header("Movement Settings")]
	[DefaultValue(50f)] public float speed;
	[DefaultValue(7f)] public float maxWalkSpeed;
	[DefaultValue(4f)] public float frictionSpeed;
	[DefaultValue(0.5f)] public float airControl;
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
	[DefaultValue(2.5f)] public float fallMultiplier;
	[DefaultValue(1.75f)] public float lowJumpMultiplier;
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

		var targetLV = dashDirection * dashSpeed;
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
		healthController = GetComponent<PlayerHealthController>();
		uiController = GetComponent<PlayerUIController>();

		rb = GetComponent<Rigidbody>();
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
	}

	void FixedUpdate() {
		ApplyGravity();

		/* --------------------------------- Falling -------------------------------- */
		if (!onSlope && rb.linearVelocity.y < 0) {
			rb.AddForce((fallMultiplier - 1) * Physics.gravity, ForceMode.Acceleration);
		}

		if (!onSlope && rb.linearVelocity.y > 0 && !jumpAction.IsPressed()) {
			rb.AddForce((lowJumpMultiplier - 1) * Physics.gravity, ForceMode.Acceleration);
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
		if (!isGrounded) maxSpeed *= airControl;
		if (mv == Vector3.zero) {
			ApplyFriction();
			return;
		}

		Vector3 v1 = rb.linearVelocity;
		v1.y = 0;
		Vector3 v2 = cameraController.TransformMovement(mv * speed);

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
		if (v1.magnitude != maxSpeed) ApplyFriction();

		// Debug.Log(v1.magnitude);

		/**
		 FIXME: when player collides with wall, player "jumps"
		 FIXME: speed slightly above maxSpeed. adding perp still increases mag b/c pythag. work on this l8r it seems to work
		*/
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Pickup")) {
			score += 1;
			uiController.UpdateScoreText();
			Destroy(other.gameObject);
		}
	}

	/* -------------------------------------------------------------------------- */
	/*                                    Other                                   */
	/* -------------------------------------------------------------------------- */
	void ApplyFriction() {
		if (!isGrounded) return;
		rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, frictionSpeed * Time.deltaTime);
	}

	void ApplyGravity() {
		Vector3 currentGravity = onSlope ? -slopeHit.normal * Physics.gravity.magnitude : Physics.gravity;
		rb.AddForce(currentGravity, ForceMode.Acceleration);
		Debug.DrawRay(transform.position, currentGravity, Color.yellow);
	}

	void UpdateGrounded() {
		float checkDistance = capsuleCollider.height / 2 * transform.localScale.y - capsuleCollider.radius + 0.1f;
		isGrounded = Physics.SphereCast(transform.position, capsuleCollider.radius, Vector3.down, out slopeHit, checkDistance);

		if (isGrounded) {
			float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
			onSlope = angle > 0 && angle < maxSlopeAngle;
		} else {
			onSlope = false;
		}
	}

	void AddForceSlope(Vector3 force, ForceMode forceMode = ForceMode.Force) {
		if (!onSlope) {
			rb.AddForce(force, forceMode);
			return;
		}

		Vector3 forceOnSlope = Vector3.ProjectOnPlane(force, slopeHit.normal);//.normalized * force.magnitude;
		rb.AddForce(forceOnSlope, forceMode);
	}
}
