using UnityEngine;

public class MeleeProximityController : MonoBehaviour {
	private MeleeEnemyController meleeEnemyController;

	void Start() {
		meleeEnemyController = transform.parent.GetComponent<MeleeEnemyController>();
	}

	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			meleeEnemyController.PlayerInProximity();
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.CompareTag("Player")) {
			meleeEnemyController.PlayerOutOfProximity();
		}
	}
}
