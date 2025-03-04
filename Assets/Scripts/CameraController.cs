using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
	private Camera cam;
	private Vector3 offset;

	public Transform player;
	private Rigidbody playerRb;
	private PlayerController playerController;

	[Header("Camera Settings")]
	public Vector3 shoulderOffset;
	[DefaultValue(0f)] public float cameraTilt;
	[DefaultValue(0.15f)] public float sensitivity;

	private float yaw = 0f;
	private float pitch = 0f;
	public Quaternion RealRotation { get; private set; }
	private Vector2 lookInput;

	[DefaultValue(-100f)] public float minPitch;
	[DefaultValue(60f)] public float maxPitch;

	[Header("FOV Settings")]
	[DefaultValue(75f)] public float maxFOV;
	[DefaultValue(60f)] public float minFOV;
	[DefaultValue(5f)] public float fovSpeed;

	private InputAction lookAction;
	private InputAction freelookAction;
	private bool canFreelook = true;
	public bool Freelooking => freelookAction.IsInProgress() && canFreelook;

	private float lastYaw = 0f;
	private float lastPitch = 0f;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		shoulderOffset = new Vector3(1.6f, 1f, 0f);
		Utils.SetDefaultValues(this);
	}

	void Start() {
		offset = transform.position - player.position;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		lookAction = InputSystem.actions.FindAction("Look");
		freelookAction = InputSystem.actions.FindAction("Freelook");

		freelookAction.canceled += _ => {
			canFreelook = true;
			yaw = lastYaw;
			pitch = lastPitch;
		};

		cam = GetComponent<Camera>();
		playerRb = player.GetComponent<Rigidbody>();
		playerController = player.GetComponent<PlayerController>();
	}

	// TODO fix logic of canceling freelook

	public void OnShoot() {
		// On shoot, aim in the last aim direction and prevent freelooking until key is re-pressed
		if (!Freelooking) return;
		canFreelook = false;
		yaw = lastYaw;
		pitch = lastPitch;
		transform.rotation = Quaternion.Euler(pitch, yaw, 0);
	}

	void LateUpdate() {
		lookInput = !GameManager.Instance.IsPaused ? lookAction.ReadValue<Vector2>() : Vector2.zero;

		if (canFreelook && freelookAction.IsPressed()) {
			lastYaw = yaw;
			lastPitch = pitch;
			RealRotation = Quaternion.Euler(pitch, yaw, 0);
		}

		yaw += lookInput.x * sensitivity;
		pitch -= lookInput.y * sensitivity;
		pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

		// Shoulder
		Vector3 rotatedShoulder = Quaternion.Euler(0, yaw, 0) * shoulderOffset + player.position;

		Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
		Vector3 targetPosition = rotatedShoulder + rotation * offset;
		if (!Freelooking) RealRotation = rotation;

		transform.position = targetPosition;
		transform.LookAt(rotatedShoulder + Vector3.up * cameraTilt);

		/* -------------------------------- FOV stuff ------------------------------- */
		var r = playerRb.linearVelocity;
		float playerSpeed = Mathf.Sqrt(r.x * r.x + r.z * r.z);
		float targetFOV = Mathf.LerpUnclamped(minFOV, maxFOV, playerSpeed / playerController.maxSprintSpeed);
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovSpeed * Time.deltaTime);
	}

	public Vector3 TransformMovement(Vector3 mv) {
		float y = Freelooking ? lastYaw : yaw;
		return Quaternion.Euler(0, y, 0) * mv;
	}
}
