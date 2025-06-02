using UnityEngine;

public class MeleeProximityController : MonoBehaviour {
	private MeleeEnemyController meleeEnemyController;

	void Start() {
		meleeEnemyController = transform.parent.GetComponent<MeleeEnemyController>();
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Player")) {
			meleeEnemyController.PlayerInProximity();
		}
	}

	void OnCollisionExit(Collision collision) {
		if (collision.gameObject.CompareTag("Player")) {
			meleeEnemyController.PlayerOutOfProximity();
		}
	}
}
