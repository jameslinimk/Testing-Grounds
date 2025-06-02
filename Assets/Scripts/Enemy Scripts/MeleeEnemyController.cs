using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class MeleeEnemyController : MonoBehaviour {
	private EnemyController ec;
	private EnemyHealthController health;
	public Transform player;
	private PlayerHealthController playerHealth;

	[DefaultValue(10f)] public float damage;
	private float lastHit = -Mathf.Infinity;

	private bool touchingPlayer = false;
	private Coroutine damageCoroutine;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		player = GameObject.FindGameObjectWithTag("Player").transform;
		Utils.SetDefaultValues(this);
	}

	void Start() {
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
		Debug.Log("Zombie Attack");

		if (!touchingPlayer || health.health <= 0) return;
		Debug.Log("Zombie Attack - touching player");
		playerHealth.TakeDamage(damage, transform.position);
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Player")) {
			touchingPlayer = true;
		}
	}

	void OnCollisionExit(Collision collision) {
		if (collision.gameObject.CompareTag("Player")) {
			touchingPlayer = false;
		}
	}
}
