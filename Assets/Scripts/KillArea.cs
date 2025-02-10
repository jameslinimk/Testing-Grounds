using UnityEngine;

public class KillArea : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			other.GetComponent<PlayerController>().Die();
		}
	}
}
