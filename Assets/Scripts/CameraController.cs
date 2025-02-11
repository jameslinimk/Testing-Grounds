using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
	public Transform player;
	private Vector3 offset;

	public float sensitivity;

	private float yaw = 0f;
	private float pitch = 0f;
	private Vector2 lookInput;

	public float minPitch;
	public float maxPitch;

	private InputAction lookAction;
	private InputAction freelookAction;

	private float lastYaw = 0f;
	private float lastPitch = 0f;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		sensitivity = 0.1f;
		minPitch = -30f;
		maxPitch = 60f;
	}

	void Start() {
		offset = transform.position - player.position;

		yaw = transform.rotation.eulerAngles.y;
		pitch = transform.rotation.eulerAngles.x;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		lookAction = InputSystem.actions.FindAction("Look");
		freelookAction = InputSystem.actions.FindAction("Freelook");

		freelookAction.canceled += _ => {
			yaw = lastYaw;
			pitch = lastPitch;
		};
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
		Vector3 target = player.position + rotation * offset;

		transform.position = target;
		transform.LookAt(player.position);
	}

	public Vector3 TransformMovement(Vector3 mv) {
		float y = freelookAction.IsPressed() ? lastYaw : yaw;
		return Quaternion.Euler(0, y, 0) * mv;
	}
}
