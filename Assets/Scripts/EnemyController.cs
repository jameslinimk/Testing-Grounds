using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour {
	public Transform player;
	private NavMeshAgent navMeshAgent;

	private float lastHit = -Mathf.Infinity;
	public float hitCooldown;

	private bool touchingPlayer = false;
	private Coroutine damageCoroutine;

	void Start() {
		navMeshAgent = GetComponent<NavMeshAgent>();
	}

	void Update() {
		navMeshAgent.SetDestination(player.position);
	}

	private IEnumerator DamageOverTime(PlayerHealthController player) {
		while (touchingPlayer) {
			if (Time.time - lastHit < hitCooldown)
				yield return new WaitForSeconds(hitCooldown - (Time.time - lastHit));

			lastHit = Time.time;
			player.TakeDamage(1, transform.position);
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
