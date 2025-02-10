using UnityEngine;

public class Rotator : MonoBehaviour {
	private Vector3 rotation;

	void Start() {
		transform.rotation = Quaternion.Euler(
			Random.Range(0, 360),
			Random.Range(0, 360),
			Random.Range(0, 360)
		);
		rotation = new Vector3(
			Random.Range(15, 45),
			Random.Range(15, 45),
			Random.Range(15, 45)
		);
	}

	void Update() {
		transform.Rotate(rotation * Time.deltaTime);
	}
}
