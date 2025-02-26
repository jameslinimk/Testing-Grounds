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
	[DefaultValue(1.5f)] public float lookAtHeight;
	[DefaultValue(0.15f)] public float sensitivity;

	private float yaw = 0f;
	private float pitch = 0f;
	private Vector2 lookInput;

	[DefaultValue(-30f)] public float minPitch;
	[DefaultValue(60f)] public float maxPitch;

	[Header("FOV Settings")]
	[DefaultValue(75f)] public float maxFOV;
	[DefaultValue(60f)] public float minFOV;
	[DefaultValue(5f)] public float fovDampingRate;

	private InputAction lookAction;
	private InputAction freelookAction;

	private float lastYaw = 0f;
	private float lastPitch = 0f;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
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
		float targetFOV = Mathf.LerpUnclamped(minFOV, maxFOV, playerSpeed / playerController.maxSprintSpeed);
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovDampingRate * Time.deltaTime);
	}

	public Vector3 TransformMovement(Vector3 mv) {
		float y = freelookAction.IsPressed() ? lastYaw : yaw;
		return Quaternion.Euler(0, y, 0) * mv;
	}
}
