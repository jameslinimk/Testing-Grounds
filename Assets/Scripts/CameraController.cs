using UnityEngine;

public class CameraController : MonoBehaviour {
	public Transform player;
	public float deceleration;
	private Vector3 offset;

	void Start() {
		offset = transform.position - player.position;
	}

	void FixedUpdate() {
		Vector3 moveTo = player.position + offset;
		transform.position = Vector3.Lerp(transform.position, moveTo, deceleration * Time.fixedDeltaTime);
	}
}
