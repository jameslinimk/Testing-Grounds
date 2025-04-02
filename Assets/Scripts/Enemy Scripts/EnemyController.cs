using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour, IKnockbackable {
	public Func<Vector3> GetTarget;

	[DefaultValue(1.2f)] public float knockbackForceMultiplier;
	[DefaultValue(2.6f)] public float deathKnockbackForceMultiplier;
	[DefaultValue(5f)] public float deathKnockbackDelay;

	private NavMeshAgent agent;
	private Rigidbody rb;

	private bool dead = false;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	void Start() {
		agent = GetComponent<NavMeshAgent>();
		rb = GetComponent<Rigidbody>();

		ToggleAgent(true);
	}

	private void ToggleAgent(bool on) {
		agent.enabled = on;
		rb.isKinematic = !on;
		if (on) {
			agent.nextPosition = rb.position;
		} else {
			rb.position = agent.nextPosition;
		}
	}

	void FixedUpdate() {
		if (agent.enabled) return;
		if (rb.linearVelocity.magnitude < 0.1f) {
			ToggleAgent(true);
			rb.linearVelocity = Vector3.zero;
			if (dead) Destroy(gameObject);
		}
	}

	void Update() {
		if (agent.enabled && GetTarget != null) agent.SetDestination(GetTarget());
	}

	public void OnKnockback(Vector3 hitOrigin, float damage) {
		Debug.Log($"Enemy {gameObject.name} knocked back from {hitOrigin} with damage {damage}.");
		ToggleAgent(false);

		Vector3 force = (transform.position - hitOrigin).normalized;
		force.y = 0;
		rb.AddForce(damage * knockbackForceMultiplier * force, ForceMode.Impulse);
	}

	public void OnDie(Vector3 hitOrigin, float damage) {
		rb.constraints = RigidbodyConstraints.None;
		dead = true;
		ToggleAgent(false);

		Vector3 force = (transform.position - hitOrigin).normalized;
		rb.AddForce(damage * deathKnockbackForceMultiplier * force, ForceMode.Impulse);

		Destroy(gameObject, deathKnockbackDelay);
	}
}
