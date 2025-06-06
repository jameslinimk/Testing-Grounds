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
	private Quaternion rotation = Quaternion.identity;
	private Vector2 lookInput;

	[DefaultValue(-100f)] public float minPitch;
	[DefaultValue(60f)] public float maxPitch;

	[Header("FOV Settings")]
	[DefaultValue(66f)] public float maxFOV;
	[DefaultValue(60f)] public float minFOV;
	[DefaultValue(5f)] public float fovSpeed;

	[Header("Collision Settings")]
	[DefaultValue(0.3f)] public float collisionRadius = 0.3f;         // Radius of the sphere‑cast used to test collisions
	[DefaultValue(0.1f)] public float collisionBuffer = 0.1f;         // Extra distance kept from surfaces
	public LayerMask collisionMask = ~0;                              // Layers considered solid for the camera

	private InputAction lookAction;
	private InputAction freelookAction;
	private bool canFreelook = true;
	public bool IsFreelooking => freelookAction.IsInProgress() && canFreelook;

	private float lastYaw = 0f;
	private float lastPitch = 0f;

	private Vector3 lastPosition = Vector3.zero;
	private Quaternion lastViewRotation = Quaternion.identity;
	private Quaternion lastRotation = Quaternion.identity;

	public float RealYaw => IsFreelooking ? lastYaw : yaw;
	public float RealPitch => IsFreelooking ? lastPitch : pitch;
	public Quaternion RealRotation => IsFreelooking ? lastViewRotation : rotation;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		shoulderOffset = new Vector3(1.6f, 1f, 0f);
		Utils.SetDefaultValues(this);
	}

	void Start() {
		offset = transform.localPosition; // keep local offset to allow easy scaling by collisions

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		lookAction = InputSystem.actions.FindAction("Look");
		freelookAction = InputSystem.actions.FindAction("Freelook");

		freelookAction.started += _ => {
			if (!canFreelook) return;
			lastYaw = yaw;
			lastPitch = pitch;
			lastPosition = transform.position;
			lastRotation = transform.rotation;
			lastViewRotation = Quaternion.Euler(pitch, yaw, 0);
		};
		freelookAction.canceled += _ => {
			if (canFreelook) {
				yaw = lastYaw;
				pitch = lastPitch;
			}
			canFreelook = true;
		};

		cam = GetComponent<Camera>();
		playerRb = player.GetComponent<Rigidbody>();
		playerController = player.GetComponent<PlayerController>();
	}

	/// <summary>
	/// Called by the shooter when the player fires while freelooking to snap back to the last aimed direction.
	/// Returns the last camera position and forward direction before the snap.
	/// </summary>
	public (Vector3, Vector3) ShootReset() {
		if (!IsFreelooking) {
			Debug.LogWarning("Trying to reset shoot while not freelooking");
			return (Vector3.zero, Vector3.zero);
		}

		// Aim in the last aim direction and prevent freelooking until key is re‑pressed and return last position and rotation
		canFreelook = false;
		yaw = lastYaw;
		pitch = lastPitch;
		return (lastPosition, lastRotation * Vector3.forward);
	}

	void LateUpdate() {
		/* ---------------------------- Read look input --------------------------- */
		lookInput = !GameManager.Instance.IsPaused ? lookAction.ReadValue<Vector2>() : Vector2.zero;

		yaw += lookInput.x * sensitivity;
		pitch -= lookInput.y * sensitivity;
		pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

		/* ------------------------------ Pivot point --------------------------- */
		Vector3 pivot = Quaternion.Euler(0, yaw, 0) * shoulderOffset + player.position;
		rotation = Quaternion.Euler(pitch, yaw, 0);

		/* ---------------------------- Desired camera --------------------------- */
		Vector3 desiredLocalOffset = rotation * offset; // local space offset rotated to world
		Vector3 desiredPosition = pivot + desiredLocalOffset;

		/* --------------------------- Collision check --------------------------- */
		Vector3 dirToDesired = desiredPosition - pivot;
		float desiredDistance = dirToDesired.magnitude;

		if (desiredDistance > 0.001f) {
			if (Physics.SphereCast(pivot, collisionRadius, dirToDesired.normalized, out RaycastHit hit, desiredDistance + collisionBuffer, collisionMask, QueryTriggerInteraction.Ignore)) {
				// Move the camera to just before the obstruction, respecting buffer
				desiredPosition = pivot + dirToDesired.normalized * Mathf.Max(hit.distance - collisionBuffer, 0.05f);
			}
		}

		transform.position = desiredPosition;
		transform.LookAt(pivot + Vector3.up * cameraTilt);

		/* ----------------------------- FOV adjust ------------------------------ */
		var vel = playerRb.linearVelocity;
		float playerSpeed = Mathf.Sqrt(vel.x * vel.x + vel.z * vel.z);
		float targetFOV = Mathf.LerpUnclamped(minFOV, maxFOV, playerSpeed / playerController.maxSprintSpeed);
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovSpeed * Time.deltaTime);
	}

	/// <summary>
	/// Transforms a movement vector from local (input) space to world space relative to the camera's yaw.
	/// </summary>
	public Vector3 TransformMovement(Vector3 mv) {
		return Quaternion.Euler(0, RealYaw, 0) * mv;
	}
}
