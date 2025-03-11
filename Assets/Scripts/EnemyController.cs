using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour, IKnockbackable {
	public Transform player;
	private NavMeshAgent agent;
	private Rigidbody rb;

	[DefaultValue(10f)] public float damage;
	[DefaultValue(1f)] public float hitCooldown;
	private float lastHit = -Mathf.Infinity;

	private bool touchingPlayer = false;
	private Coroutine damageCoroutine;

	[DefaultValue(0.8f)] public float knockbackForceMultiplier;
	[DefaultValue(0.5f)] public float knockbackDuration;

	[DefaultValue(2.3f)] public float deathKnockbackForceMultiplier;
	[DefaultValue(3f)] public float deathKnockbackDuration;

	private bool knockedback = false;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		player = GameObject.FindWithTag("Player").transform;
		Utils.SetDefaultValues(this);
	}

	void Start() {
		agent = GetComponent<NavMeshAgent>();
		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;
	}

	void Update() {
		if (!knockedback) agent.SetDestination(player.position);
	}

	private Coroutine knockbackCoroutine;

	public void OnKnockback(Vector3 hitOrigin, float damage) {
		if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);
		knockbackCoroutine = StartCoroutine(KnockbackCoroutine(hitOrigin, damage));
	}

	IEnumerator KnockbackCoroutine(Vector3 hitOrigin, float damage, bool die = false) {
		knockedback = true;
		agent.enabled = false;
		rb.isKinematic = false;

		Vector3 force = (transform.position - hitOrigin).normalized;
		if (!die) force.y = 0;
		rb.AddForce(damage * knockbackForceMultiplier * (die ? deathKnockbackForceMultiplier : 1f) * force, ForceMode.Impulse);

		yield return new WaitForSeconds(die ? deathKnockbackDuration : knockbackDuration);
		if (die) Destroy(gameObject);

		rb.linearVelocity = Vector3.zero;

		rb.isKinematic = true;
		agent.enabled = true;

		knockedback = false;
	}

	public void OnDie(Vector3 hitOrigin, float damage) {
		if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);

		rb.constraints = RigidbodyConstraints.None;
		StartCoroutine(KnockbackCoroutine(hitOrigin, damage, true));
	}

	private IEnumerator DamageOverTime(PlayerHealthController player) {
		while (touchingPlayer) {
			if (Time.time - lastHit < hitCooldown)
				yield return new WaitForSeconds(hitCooldown - (Time.time - lastHit));

			lastHit = Time.time;
			player.TakeDamage(damage, transform.position);
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
