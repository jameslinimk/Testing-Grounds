using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
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
	public float speed;
	public float maxWalkSpeed;
	public float deceleration;
	private bool isGrounded = true;

	[Header("Stamina Settings")]
	public float staminaRegenCooldown;
	public float maxStamina;
	public float staminaRegen;
	public float standingStaminaRegen;

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
	public float jumpForce;
	public float jumpStaminaCost;
	public float fallMultiplier;
	public float lowJumpMultiplier;
	private bool CanJump => isGrounded && !IsDashing && Stamina >= jumpStaminaCost;

	[Header("Crouch Settings")]
	public float crouchHeightMultiplier;
	public float maxCrouchSpeed;
	public float crouchJumpForce;
	private Vector3 originalScale;
	private float crouchHeightDifference;
	private bool isCrouching = false;

	[Header("Sprint Settings")]
	public float maxSprintSpeed;
	public float sprintCooldown;
	public float sprintStaminaCost;

	private bool sprinting = false;
	private float sprintEnd = -Mathf.Infinity;

	private bool CanSprint => Time.time - sprintEnd >= sprintCooldown && Stamina > 0 && !isCrouching && !IsDashing;

	[Header("Dash Settings")]
	public float dashCooldown;
	public float dashDuration;
	public float dashSpeed;
	public float dashStaminaCost;

	private Vector3 dashDirection;
	public float dashStart { get; private set; } = -Mathf.Infinity;

	private bool IsDashing => Time.time - dashStart < dashDuration;
	private bool CanDash => Time.time - (dashStart + dashDuration) >= dashCooldown && Stamina >= dashStaminaCost;

	[Header("Slope Settings")]
	public float maxSlopeAngle;
	private RaycastHit slopeHit;
	private bool onSlope;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		speed = 50f;
		maxWalkSpeed = 7f;
		deceleration = 4f;

		staminaRegenCooldown = 1f;
		maxStamina = 5f;
		staminaRegen = 1f;
		standingStaminaRegen = 2f;

		jumpForce = 20f;
		jumpStaminaCost = 0.25f;
		fallMultiplier = 2.5f;
		lowJumpMultiplier = 2f;

		crouchHeightMultiplier = 2f;
		maxCrouchSpeed = 4f;
		crouchJumpForce = 10f;

		maxSprintSpeed = 12f;
		sprintCooldown = 0.25f;
		sprintStaminaCost = 1f;

		dashCooldown = 1f;
		dashDuration = 0.2f;
		dashSpeed = 150f;
		dashStaminaCost = 0.5f;

		maxSlopeAngle = 45f;
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
		if (IsDashing) {
			AddForceSlope(dashDirection * dashSpeed, ForceMode.Force);
			return;
		}

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
		if (mv == Vector3.zero) {
			ApplyFriction();
			return;
		}

		if (Mathf.Abs(rb.linearVelocity.magnitude - maxSpeed) < 0.1f) {
			rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
		}

		Vector3 v1 = rb.linearVelocity;
		v1.y = 0;
		Vector3 v2 = cameraController.TransformMovement(mv * speed);

		// Normal movement
		if (v1.magnitude < maxSpeed) {
			AddForceSlope(v2, ForceMode.Force);
			rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);
			return;
		}

		// Speed control
		Vector3 vParallel = Vector3.Project(v2, v1);
		Vector3 vPerp = v2 - vParallel;

		if (Vector3.Dot(vParallel, v1) < 0) {
			AddForceSlope(vParallel, ForceMode.Force);
		}

		AddForceSlope(vPerp, ForceMode.Force);
		if (rb.linearVelocity.magnitude != maxSpeed) ApplyFriction();

		Debug.Log(rb.linearVelocity.magnitude);

		/**
		 FIXME: when player collides with wall, player "jumps"
		 FIXME: speed slightly above maxSpeed. adding perp still increases mag b/c pythag. work on this l8r it seems to work
		*/
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Pickup")) {
			Debug.Log("Picked up");
			score += 1;
			uiController.UpdateScoreText();
			Destroy(other.gameObject);
		}
	}

	/* -------------------------------------------------------------------------- */
	/*                                    Other                                   */
	/* -------------------------------------------------------------------------- */
	void ApplyFriction() {
		rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, deceleration * Time.deltaTime);
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

		// rb.useGravity = !onSlope;
	}

	void AddForceSlope(Vector3 force, ForceMode forceMode = ForceMode.Force) {
		if (!onSlope) {
			rb.AddForce(force, forceMode);
			return;
		}

		Vector3 forceOnSlope = Vector3.ProjectOnPlane(force, slopeHit.normal);//.normalized * force.magnitude;
		rb.AddForce(forceOnSlope, forceMode);

		// Debug.DrawRay(transform.position, forceOnSlope, Color.red, 0.5f);
	}
}
