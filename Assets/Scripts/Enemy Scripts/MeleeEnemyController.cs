using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemyController : MonoBehaviour {
	private EnemyController enemyController;
	private EnemyHealthController health;
	public Transform player;

	[DefaultValue(10f)] public float damage;
	[DefaultValue(1f)] public float hitCooldown;
	private float lastHit = -Mathf.Infinity;

	private bool touchingPlayer = false;
	private Coroutine damageCoroutine;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		player = GameObject.FindGameObjectWithTag("Player").transform;
		Utils.SetDefaultValues(this);
	}

	void Start() {
		enemyController = GetComponent<EnemyController>();
		enemyController.GetTarget = () => player.position;
		health = GetComponent<EnemyHealthController>();
	}

	private IEnumerator DamageOverTime(PlayerHealthController player) {
		while (touchingPlayer) {
			if (Time.time - lastHit < hitCooldown)
				yield return new WaitForSeconds(hitCooldown - (Time.time - lastHit));

			lastHit = Time.time;
			if (health.health > 0) player.TakeDamage(damage, transform.position);
			yield return new WaitForSeconds(hitCooldown);
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Player")) {
			touchingPlayer = true;
			damageCoroutine = StartCoroutine(DamageOverTime(collision.gameObject.GetComponent<PlayerHealthController>()));
		}
	}

	void OnCollisionExit(Collision collision) {
		if (collision.gameObject.CompareTag("Player")) {
			touchingPlayer = false;
			StopCoroutine(damageCoroutine);
		}
	}
}
