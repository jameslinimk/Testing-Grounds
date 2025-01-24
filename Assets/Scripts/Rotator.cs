using UnityEngine;

public class Rotator : MonoBehaviour {
	void Start() {
		transform.rotation = Quaternion.Euler(
			Random.Range(0, 360),
			Random.Range(0, 360),
			Random.Range(0, 360)
		);
	}

	void Update() {
		transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
	}
}
