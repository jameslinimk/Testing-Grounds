using UnityEngine;

public class CameraController : MonoBehaviour {
	public GameObject player;
	public float deceleration;
	private Vector3 offset;

	void Start() {
		offset = transform.position - player.transform.position;
	}

	void FixedUpdate() {
		Vector3 moveTo = player.transform.position + offset;
		transform.position = Vector3.Lerp(transform.position, moveTo, deceleration * Time.fixedDeltaTime);
	}
}
