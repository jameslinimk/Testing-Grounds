using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

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
	private bool isGrounded;

	[Header("Health Settings")]
	public int maxHealth;
	public int health;
	public TextMeshProUGUI healthText;
	public TextMeshProUGUI endText;

	[Header("Movement Settings")]
	public float speed;
	public float maxSpeed;
	public float deceleration;

	[Header("Dash Settings")]
	public float dashCooldown;
	public float dashDuration;
	public float dashSpeed;

	private Vector3 dashDirection;
	private float dashStart = -Mathf.Infinity;
	private bool IsDashing => Time.time - dashStart < dashDuration;

	[Header("Score UI")]
	public TextMeshProUGUI scoreText;
	private int score = 0;

	[Header("Dash UI")]
	public TextMeshProUGUI dashCDText;
	public Image dashCDImage;
	public Fader dashCDfader = new(1, 3.5f);

	void Start() {
		rb = GetComponent<Rigidbody>();
		sphereCollider = GetComponent<SphereCollider>();
		health = maxHealth;

		UpdateScoreText();
		UpdateDashUI();
		UpdateHealthText();
	}

	void OnMove(InputValue inputValue) {
		Vector2 inputVector = inputValue.Get<Vector2>();
		mv = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

		if (mv != Vector3.zero) lastMV = mv;
	}

	void OnJump() {
		if (PauseManager.Instance.IsPaused) return;
		if (Time.time - (dashStart + dashDuration) < dashCooldown) return;
		dashCDfader.target = 1f;

		dashStart = Time.time;
		dashDirection = mv == Vector3.zero ? lastMV : mv;
	}

	void FixedUpdate() {
		if (IsDashing) {
			rb.AddForce(dashDirection * dashSpeed, ForceMode.Force);
			return;
		}

		if (mv == Vector3.zero) {
			ApplyFriction();
			return;
		}

		if (Mathf.Abs(rb.linearVelocity.magnitude - maxSpeed) < 0.1f) {
			rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
		}

		if (rb.linearVelocity.magnitude < maxSpeed) {
			rb.AddForce(mv * speed, ForceMode.Force);
			return;
		}

		Vector3 v1 = rb.linearVelocity;
		Vector3 v2 = mv * speed;

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
			dashCDfader.target = 0f;
		} else {
			dashCDText.text = $"{secondsLeft:0.0}s";
		}

		dashCDText.alpha = dashCDfader.Update(dashCDText.alpha);
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

	private void Die() {
		endText.text = $"You died with a score of {score}!";
		endText.gameObject.SetActive(true);
		PauseManager.Instance.SetPause(true);
	}
}
