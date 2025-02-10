using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

[System.Serializable]
public class Fader {
	[HideInInspector]
	public float target = 1;
	public float speed;

	public Fader(float target, float speed) {
		this.target = target;
		this.speed = speed;
	}

	/// <summary>
	/// Returns new value for given value
	/// </summary>
	public float Update(float value) {
		return Mathf.MoveTowards(value, target, speed * Time.deltaTime);
	}
}

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
	private Rigidbody rb;
	private Vector3 mv;
	private Vector3 lastMV;
	private SphereCollider sphereCollider;
	private InputAction sprintAction;
	private bool isGrounded;

	[Header("Health Settings")]
	public int maxHealth = 10;
	public int health = 10;
	public TextMeshProUGUI healthText;
	public TextMeshProUGUI endText;

	[Header("Movement Settings")]
	public float speed = 50f;
	public float walkSpeed = 7f;
	public float deceleration = 4f;
	public CameraController cameraController;

	[Header("Sprint Settings")]
	public float sprintSpeed = 12f;
	public float sprintCooldown = 1f;
	public float staminaRegenCooldown = 1f;
	public float maxStamina = 5f;
	public float staminaRegen = 1f;
	public float sprintStaminaCost = 1f;

	private bool sprinting = false;
	private float _stamina = 5f;
	private float Stamina {
		get => _stamina;
		set {
			if (value < _stamina) lastStaminaDrain = Time.time;
			_stamina = Math.Clamp(value, 0, maxStamina);
		}
	}
	private float sprintEnd = -Mathf.Infinity;
	private float lastStaminaDrain = -Mathf.Infinity;

	private bool CanRegen => Time.time - lastStaminaDrain >= staminaRegenCooldown && Stamina < maxStamina;
	private bool CanSprint => Time.time - sprintEnd >= sprintCooldown && Stamina > 0;

	[Header("Dash Settings")]
	public float dashCooldown = 1f;
	public float dashDuration = 0.2f;
	public float dashSpeed = 150f;

	private Vector3 dashDirection;
	private float dashStart = -Mathf.Infinity;

	private bool IsDashing => Time.time - dashStart < dashDuration;
	private bool CanDash => Time.time - (dashStart + dashDuration) >= dashCooldown;

	[Header("Score UI")]
	public TextMeshProUGUI scoreText;
	private int score = 0;

	[Header("Dash UI")]
	public TextMeshProUGUI dashCDText;
	public Image dashCDImage;
	public Fader dashFader = new(1, 3.5f);

	void Start() {
		rb = GetComponent<Rigidbody>();
		sphereCollider = GetComponent<SphereCollider>();
		health = maxHealth;

		UpdateScoreText();
		UpdateDashUI();
		UpdateHealthText();

		sprintAction = InputSystem.actions.FindAction("Sprint");
	}

	void OnMove(InputValue inputValue) {
		if (GameManager.Instance.IsPaused) return;
		Vector2 inputVector = inputValue.Get<Vector2>();
		mv = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

		if (mv != Vector3.zero) lastMV = mv;
	}

	void OnDash() {
		if (GameManager.Instance.IsPaused) return;
		if (!CanDash) return;
		dashFader.target = 1f;

		dashStart = Time.time;
		dashDirection = cameraController.TransformMovement(mv == Vector3.zero ? lastMV : mv);
	}

	void FixedUpdate() {
		if (IsDashing) {
			rb.AddForce(dashDirection * dashSpeed, ForceMode.Force);
			return;
		}

		float maxSpeed = walkSpeed;
		if (sprintAction.IsPressed() && CanSprint) {
			Debug.Log("sprinting");

			maxSpeed = sprintSpeed;
			Stamina -= Time.deltaTime * sprintStaminaCost;
			sprinting = true;
		} else if (sprinting) {
			Debug.Log("end_sprint");

			sprinting = false;
			sprintEnd = Time.time;
		} else if (CanRegen) {
			Debug.Log("regen");

			Stamina += Time.deltaTime * staminaRegen;
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

		// Debug.Log(rb.linearVelocity.magnitude);

		// FIXME: when player collides with wall, player "jumps"
		// FIXME: speed slightly above maxSpeed
	}

	private void ApplyFriction() {
		rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, deceleration * Time.deltaTime);
	}

	void Update() {
		UpdateDashUI();
		UpdateGrounded();
	}

	private void UpdateGrounded() {
		float sphereRadius = sphereCollider.radius * transform.localScale.y;
		isGrounded = Physics.Raycast(transform.position, Vector3.down, sphereRadius + 0.1f);
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Pickup")) {
			score += 1;
			UpdateScoreText();
			Destroy(other.gameObject);
		}
	}

	private void UpdateDashUI() {
		float ratio = Mathf.Clamp01(1 - (Time.time - dashStart - dashDuration) / dashCooldown);
		dashCDImage.fillAmount = ratio;

		float secondsLeft = Mathf.Clamp(dashCooldown - (Time.time - dashStart - dashDuration), 0f, dashCooldown);
		if (secondsLeft == 0f && dashCDText.alpha == 1f) {
			dashFader.target = 0f;
		} else {
			dashCDText.text = $"{secondsLeft:0.0}s";
		}

		dashCDText.alpha = dashFader.Update(dashCDText.alpha);
	}

	private void UpdateScoreText() {
		scoreText.text = $"Score: {score}";
	}

	private void UpdateHealthText() {
		healthText.text = $"Health: {health}";

	}

	public void TakeDamage(int damage, Vector3 hitOrigin) {
		health -= damage;
		rb.AddForce((transform.position - hitOrigin).normalized * 10, ForceMode.Impulse);
		UpdateHealthText();

		if (health <= 0) Die();
	}

	public void Die() {
		endText.text = $"You died with a score of {score}!";
		endText.gameObject.SetActive(true);
		GameManager.Instance.SetPause(true);
	}
}
