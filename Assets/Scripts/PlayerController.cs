using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;

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
		mv = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;

		if (mv != Vector3.zero) lastMV = mv;
	}

	void OnJump() {
		if (Time.time - (dashStart + dashDuration) < dashCooldown) return;

		dashStart = Time.time;
		dashDirection = mv == Vector3.zero ? lastMV : mv;
	}

	void FixedUpdate() {
		if (IsDashing) {
			rb.AddForce(dashDirection * dashSpeed, ForceMode.Force);
			return;
		}

		if (mv != Vector3.zero) {
			if (Math.Abs(rb.linearVelocity.magnitude - maxSpeed) < 0.1f) {
				rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
			}

			if (rb.linearVelocity.magnitude < maxSpeed) {
				rb.AddForce(mv * speed, ForceMode.Force);
				return;
			}

			Vector3 v1 = rb.linearVelocity;
			Vector3 v2 = mv * speed;

			// No component parallel to current velocity; can't go faster
			Vector3 vPerp = v2 - (Vector3.Dot(v2, v1) / v1.sqrMagnitude * v1);

			if (rb.linearVelocity.magnitude == maxSpeed) {
				rb.AddForce(vPerp, ForceMode.Force);
				Debug.Log("Exact");
			} else {
				rb.AddForce(vPerp, ForceMode.Force);
				rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
			}
		} else {
			rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
		}

		if (rb.linearVelocity.magnitude > maxSpeed) Debug.Log(rb.linearVelocity.magnitude);
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Pickup")) {
			score += 1;
			SetScoreText();
			Destroy(other.gameObject);
		}
	}

	private void SetScoreText() {
		scoreText.text = $"Score: {score}";
	}
}
