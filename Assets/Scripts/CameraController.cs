using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
	private Camera cam;
	private Vector3 offset;

	public Transform player;
	private Rigidbody playerRb;
	private PlayerController playerController;

	[Header("Camera Settings")]
	public float lookAtHeight;
	public float sensitivity;

	private float yaw = 0f;
	private float pitch = 0f;
	private Vector2 lookInput;

	public float minPitch;
	public float maxPitch;

	[Header("FOV Settings")]
	public float maxFOV;
	public float minFOV;
	public float fovDeceleration;

	private InputAction lookAction;
	private InputAction freelookAction;

	private float lastYaw = 0f;
	private float lastPitch = 0f;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		lookAtHeight = 1.5f;
		sensitivity = 0.15f;

		maxFOV = 75f;
		minFOV = 60f;
		fovDeceleration = 5f;

		minPitch = -30f;
		maxPitch = 60f;
	}

	void Start() {
		offset = transform.position - player.position;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		lookAction = InputSystem.actions.FindAction("Look");
		freelookAction = InputSystem.actions.FindAction("Freelook");

		freelookAction.canceled += _ => {
			yaw = lastYaw;
			pitch = lastPitch;
		};

		cam = GetComponent<Camera>();
		playerRb = player.GetComponent<Rigidbody>();
		playerController = player.GetComponent<PlayerController>();
	}

	void LateUpdate() {
		lookInput = !GameManager.Instance.IsPaused ? lookAction.ReadValue<Vector2>() : Vector2.zero;

		if (!freelookAction.IsPressed()) {
			lastYaw = yaw;
			lastPitch = pitch;
		}

		yaw += lookInput.x * sensitivity;
		pitch -= lookInput.y * sensitivity;
		pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

		Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
		Vector3 targetPosition = player.position + rotation * offset;

		transform.position = targetPosition;
		transform.LookAt(player.position + Vector3.up * lookAtHeight);

		var r = playerRb.linearVelocity;
		float playerSpeed = Mathf.Sqrt(r.x * r.x + r.z * r.z);
		float targetFOV = Mathf.Lerp(minFOV, maxFOV, playerSpeed / playerController.maxSprintSpeed);
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovDeceleration * Time.deltaTime);
	}

	public Vector3 TransformMovement(Vector3 mv) {
		float y = freelookAction.IsPressed() ? lastYaw : yaw;
		return Quaternion.Euler(0, y, 0) * mv;
	}
}
