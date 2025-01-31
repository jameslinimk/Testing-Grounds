using UnityEngine;

public class KillArea : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		Debug.Log("Player entered kill area: " + other.name);
		if (other.CompareTag("Player")) {
			other.GetComponent<PlayerController>().Die();
		}
	}
}
