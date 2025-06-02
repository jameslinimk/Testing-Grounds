using System.ComponentModel;
using UnityEngine;

public class MeleeEnemyController : MonoBehaviour {
	private EnemyController ec;
	private EnemyHealthController health;
	private Transform player;
	private PlayerHealthController playerHealth;

	[DefaultValue(10f)] public float damage;

	private bool touchingPlayer = false;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		player = GameObject.FindGameObjectWithTag("Player").transform;
		Utils.SetDefaultValues(this);
	}

	void Start() {
		player = GameObject.FindGameObjectWithTag("Player").transform;

		ec = transform.parent.GetComponent<EnemyController>();
		health = transform.parent.GetComponent<EnemyHealthController>();
		playerHealth = player.GetComponent<PlayerHealthController>();

		ec.GetTarget = () => player.position;
	}

	public void PlayerInProximity() {
		ec.Animator.SetBool("Attacking", true);
	}

	public void PlayerOutOfProximity() {
		ec.Animator.SetBool("Attacking", false);
	}

	public void ZombieAttack() {
		if (!touchingPlayer || health.health <= 0) return;
		Debug.Log("Zombie Attack - touching player");
		playerHealth.TakeDamage(damage, transform.position);
	}

	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			touchingPlayer = true;
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.CompareTag("Player")) {
			touchingPlayer = false;
		}
	}
}
