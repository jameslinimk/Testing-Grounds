using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
	public Transform player;
	private Vector3 offset;

	public float sensitivity = 0.1f;
	public float deceleration = 6f;

	[HideInInspector]
	public float yaw = 0f;
	[HideInInspector]
	public float pitch = 0f;
	private Vector2 lookInput;

	public float minPitch = -30f;
	public float maxPitch = 60f;

	private InputAction lookAction;

	void Start() {
		offset = transform.position - player.position;

		// yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
		// pitch = Mathf.Asin(offset.y / offset.magnitude) * Mathf.Rad2Deg;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		lookAction = InputSystem.actions.FindAction("Look");
	}

	void LateUpdate() {
		lookInput = !GameManager.Instance.IsPaused ? lookAction.ReadValue<Vector2>() : Vector2.zero;

		yaw += lookInput.x * sensitivity;
		pitch -= lookInput.y * sensitivity;
		pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

		Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
		Vector3 target = player.position + rotation * offset;

		transform.position = target;
		transform.LookAt(player.position);
	}

	public Vector3 TransformMovement(Vector3 mv) {
		return Quaternion.Euler(0, yaw, 0) * mv;
	}
}
