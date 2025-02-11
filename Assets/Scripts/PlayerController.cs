using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour {
	private Rigidbody rb;
	private Vector3 mv;
	private Vector3 lastMV;
	private CapsuleCollider capsuleCollider;
	private InputAction sprintAction;
	private bool isGrounded;

	public CameraController cameraController;

	[Header("Health Settings")]
	public float maxHealth;
	public float health;
	public TextMeshProUGUI endText;

	[Header("Movement Settings")]
	public float speed;
	public float maxWalkSpeed;
	public float deceleration;

	[Header("Stamina Settings")]
	public float staminaRegenCooldown;
	public float maxStamina;
	public float staminaRegen;
	public float standingStaminaRegen;

	private float lastStaminaDrain = -Mathf.Infinity;

	private float _stamina = 5f;
	private float Stamina {
		get => _stamina;
		set {
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
	private InputAction jumpAction;

	private bool CanJump => isGrounded && !IsDashing && Stamina >= jumpStaminaCost;

	[Header("Sprint Settings")]
	public float maxSprintSpeed;
	public float sprintCooldown;
	public float sprintStaminaCost;

	private bool sprinting = false;
	private float sprintEnd = -Mathf.Infinity;

	private bool CanSprint => Time.time - sprintEnd >= sprintCooldown && Stamina > 0;

	[Header("Dash Settings")]
	public float dashCooldown;
	public float dashDuration;
	public float dashSpeed;
	public float dashStaminaCost;

	private Vector3 dashDirection;
	private float dashStart = -Mathf.Infinity;

	private bool IsDashing => Time.time - dashStart < dashDuration;
	private bool CanDash => Time.time - (dashStart + dashDuration) >= dashCooldown && Stamina >= dashStaminaCost;

	[Header("Score UI")]
	public TextMeshProUGUI scoreText;
	private int score = 0;

	[Header("Dash UI")]
	public TextMeshProUGUI dashCDText;
	public Image dashCDImage;
	private float dashCDImageAlphaTarget = 1f;

	[Header("Stamina UI")]
	private float staminaBarWidth = 0f;
	public Image staminaBarOverlay;

	[Header("Health UI")]
	private float healthBarWidth = 0f;
	private Color? barHealthyColor = null;
	public Image healthBarOverlay;
	public Color barDamagedColor;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		maxHealth = 10;
		health = 10;

		speed = 50f;
		maxWalkSpeed = 7f;
		deceleration = 4f;

		staminaRegenCooldown = 1f;
		maxStamina = 5f;
		staminaRegen = 1f;
		standingStaminaRegen = 2f;

		jumpForce = 15f;
		jumpStaminaCost = 0.25f;
		fallMultiplier = 2.5f;
		lowJumpMultiplier = 2f;

		maxSprintSpeed = 12f;
		sprintCooldown = 0.25f;
		sprintStaminaCost = 1f;

		dashCooldown = 1f;
		dashDuration = 0.2f;
		dashSpeed = 150f;
		dashStaminaCost = 0.5f;

		barDamagedColor = Color.red;
	}

	void Start() {
		rb = GetComponent<Rigidbody>();
		capsuleCollider = GetComponent<CapsuleCollider>();
		health = maxHealth;

		UpdateScoreText();
		UpdateDashUI();

		jumpAction = InputSystem.actions.FindAction("Jump");
		sprintAction = InputSystem.actions.FindAction("Sprint");
	}

	void OnMove(InputValue inputValue) {
		if (GameManager.Instance.IsPaused) return;
		Vector2 inputVector = inputValue.Get<Vector2>();
		mv = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

		if (mv != Vector3.zero) lastMV = mv;
	}

	void OnDash() {
		if (GameManager.Instance.IsPaused || !CanDash) return;
		dashCDImageAlphaTarget = 1f;

		Stamina -= dashStaminaCost;
		dashStart = Time.time;
		dashDirection = cameraController.TransformMovement(rb.linearVelocity == Vector3.zero ? lastMV : mv);
	}

	void OnJump() {
		if (GameManager.Instance.IsPaused || !CanJump) return;

		Stamina -= jumpStaminaCost;
		rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
	}

	void FixedUpdate() {
		if (rb.linearVelocity.y < 0) {
			rb.AddForce((fallMultiplier - 1) * Physics.gravity, ForceMode.Acceleration);
		}

		if (rb.linearVelocity.y > 0 && !jumpAction.IsPressed()) {
			rb.AddForce((lowJumpMultiplier - 1) * Physics.gravity, ForceMode.Acceleration);
		}

		if (IsDashing) {
			rb.AddForce(dashDirection * dashSpeed, ForceMode.Force);
			return;
		}

		// Sprinting and Stamina
		float maxSpeed = maxWalkSpeed;
		if (sprintAction.IsPressed() && CanSprint && mv != Vector3.zero) {
			maxSpeed = maxSprintSpeed;
			Stamina -= sprintStaminaCost * Time.deltaTime;
			sprinting = true;
		} else if (sprinting) {
			sprinting = false;
			sprintEnd = Time.time;
		} else if (CanRegen) {
			Stamina += (mv == Vector3.zero ? standingStaminaRegen : staminaRegen) * Time.deltaTime;
		}

		if (mv == Vector3.zero) {
			ApplyFriction();
			return;
		}

		if (Mathf.Abs(rb.linearVelocity.magnitude - maxSpeed) < 0.1f) {
			rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
		}

		Vector3 v1 = rb.linearVelocity;
		Vector3 v2 = cameraController.TransformMovement(mv * speed);

		if (rb.linearVelocity.magnitude < maxSpeed) {
			rb.AddForce(v2, ForceMode.Force);
			return;
		}

		Vector3 vParallel = Vector3.Project(v2, v1);
		Vector3 vPerp = v2 - vParallel;

		// If parallel component is in opposite direction, add it
		if (Vector3.Dot(vParallel, v1) < 0) {
			rb.AddForce(vParallel, ForceMode.Force);
		}

		rb.AddForce(vPerp, ForceMode.Force);
		if (rb.linearVelocity.magnitude != maxSpeed) ApplyFriction();

		// FIXME: when player collides with wall, player "jumps"
		// FIXME: speed slightly above maxSpeed
	}

	void ApplyFriction() {
		rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, deceleration * Time.deltaTime);
	}

	void Update() {
		UpdateDashUI();
		UpdateGrounded();
		UpdateStaminaUI();
		UpdateHealthUI();
	}

	void UpdateGrounded() {
		isGrounded = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.height / 2 + 0.1f);
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Pickup")) {
			score += 1;
			UpdateScoreText();
			Destroy(other.gameObject);
		}
	}

	void UpdateDashUI() {
		float ratio = Mathf.Clamp01(1 - (Time.time - dashStart - dashDuration) / dashCooldown);
		dashCDImage.fillAmount = ratio;

		float secondsLeft = Mathf.Clamp(dashCooldown - (Time.time - dashStart - dashDuration), 0f, dashCooldown);
		if (secondsLeft == 0f) {
			dashCDImageAlphaTarget = 0f;
		} else {
			dashCDText.text = $"{secondsLeft:0.0}s";
		}

		dashCDText.alpha = Utils.EaseTowards(dashCDText.alpha, dashCDImageAlphaTarget, 5f, 2f);
	}

	void UpdateStaminaUI() {
		if (staminaBarWidth == 0f) staminaBarWidth = staminaBarOverlay.rectTransform.rect.width;

		float current = staminaBarOverlay.rectTransform.rect.width / staminaBarWidth;
		float target = Stamina / maxStamina;

		float ratio = Utils.EaseTowards(current, target, 5f, 2f);
		staminaBarOverlay.rectTransform.sizeDelta = new Vector2(staminaBarWidth * ratio, staminaBarOverlay.rectTransform.rect.height);
	}

	void UpdateHealthUI() {
		if (healthBarWidth == 0f) healthBarWidth = healthBarOverlay.rectTransform.rect.width;

		float current = healthBarOverlay.rectTransform.rect.width / healthBarWidth;
		float target = health / maxHealth;

		float ratio = Utils.EaseTowards(current, target, 5f, 2f);
		healthBarOverlay.rectTransform.sizeDelta = new Vector2(healthBarWidth * ratio, healthBarOverlay.rectTransform.rect.height);

		// Bar color
		if (barHealthyColor == null) barHealthyColor = healthBarOverlay.color;
		healthBarOverlay.color = Color.Lerp(barDamagedColor, (Color)barHealthyColor, ratio); ;
	}

	void UpdateScoreText() {
		scoreText.text = $"Score: {score}";
	}

	public void TakeDamage(int damage, Vector3 hitOrigin) {
		health -= damage;
		rb.AddForce((transform.position - hitOrigin).normalized * 10, ForceMode.Impulse);

		if (health <= 0) Die();
	}

	public void Die() {
		endText.text = $"You died with a score of {score}!";
		endText.gameObject.SetActive(true);
		GameManager.Instance.SetPause(true, false);
	}
}
