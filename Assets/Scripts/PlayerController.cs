using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections;

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

	[Header("Score UI")]
	public TextMeshProUGUI scoreText;
	private int score = 0;

	[Header("Dash UI")]
	public TextMeshProUGUI dashCDText;
	public Image dashCDImage;

	void Start() {
		rb = GetComponent<Rigidbody>();
		SetScoreText();
	}

	void OnMove(InputValue inputValue) {
		Vector2 inputVector = inputValue.Get<Vector2>();
		mv = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

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

			// No component parallel to current velocity; can't go faster (ProjectOnPlane is equal to subtracting projection)
			Vector3 vPerp = Vector3.ProjectOnPlane(v2, v1);

			if (rb.linearVelocity.magnitude == maxSpeed) {
				rb.AddForce(vPerp, ForceMode.Force);
			} else {
				rb.AddForce(vPerp, ForceMode.Force);
				rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
			}
		} else {
			rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
		}

		// if (rb.linearVelocity.magnitude > maxSpeed) Debug.Log(rb.linearVelocity.magnitude - maxSpeed);
		// TODO: Fix player going faster than maxSpeed (by like 0.1)
	}

	void Update() {
		SetDashUI();
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Pickup")) {
			score += 1;
			SetScoreText();
			Destroy(other.gameObject);
		}
	}

	private IEnumerator FadeCDTextToZero() {
		Color ogColor = dashCDText.color;
		float duration = 0.5f;
		float elapsedTime = 0f;

		while (elapsedTime < duration) {
			elapsedTime += Time.deltaTime;
			float alpha = Mathf.Lerp(ogColor.a, 0f, elapsedTime / duration);
			dashCDText.color = new Color(ogColor.r, ogColor.g, ogColor.b, alpha);
			yield return null;
		}

		dashCDText.color = new Color(ogColor.r, ogColor.g, ogColor.b, 0f);
	}

	private void SetDashUI() {
		float ratio = Math.Clamp(1 - (Time.time - dashStart) / dashCooldown, 0f, 1f);
		float secondsLeft = Math.Clamp(dashCooldown - (Time.time - dashStart), 0f, dashCooldown);
		dashCDImage.fillAmount = ratio;
		if (secondsLeft == 0f) {
			dashCDText.text = "";
			StartCoroutine(FadeCDTextToZero());
		} else {
			dashCDText.text = $"{secondsLeft:0.#}s";
		}
	}

	private void SetScoreText() {
		scoreText.text = $"Score: {score}";
	}
}
