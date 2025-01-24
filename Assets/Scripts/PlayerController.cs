using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
	private Rigidbody rb;
	private Vector3 mv;
	private Vector3 lastMV;

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

	[Header("Other")]
	public TextMeshProUGUI scoreText;
	private int score = 0;

	void Start() {
		rb = GetComponent<Rigidbody>();
		SetScoreText();
	}

	void OnMove(InputValue inputValue) {
		Vector2 inputVector = inputValue.Get<Vector2>();
		mv = new Vector3(inputVector.x, 0.0f, inputVector.y);

		if (mv != Vector3.zero) lastMV = mv;
	}

	void OnJump() {
		if (Time.time - dashStart < dashCooldown) return;

		dashStart = Time.time;
		dashDirection = mv == Vector3.zero ? lastMV : mv;
	}

	void FixedUpdate() {
		if (IsDashing) {
			rb.AddForce(dashDirection * dashSpeed, ForceMode.Force);
			// rb.AddForce(mv * speed / 3, ForceMode.Force);
			return;
		}

		if (mv != Vector3.zero) {
			rb.AddForce(mv * speed, ForceMode.Force);
		} else {
			rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
		}
		rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Pickup")) {
			score += 1;
			SetScoreText();
			Destroy(other.gameObject);
		}
	}

	void SetScoreText() {
		scoreText.text = $"Score: {score}";
	}
}
