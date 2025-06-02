using UnityEngine;

public class EnemyAnimationEvenHandler : MonoBehaviour {
	private EnemyController enemyController;
	private MeleeEnemyController meleeEnemyController;

	void Start() {
		enemyController = transform.parent.GetComponent<EnemyController>();
		meleeEnemyController = transform.parent.GetChild(0).GetComponent<MeleeEnemyController>();
	}

	public void StandFinish() {
		enemyController.StandFinish();
	}

	public void ZombieDeath() {
		enemyController.ZombieDeath();
	}

	public void ZombieAttack() {
		meleeEnemyController.ZombieAttack();
	}
}
