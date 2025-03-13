using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour, IKnockbackable {
	public Transform player;

	[DefaultValue(0.8f)] public float knockbackForceMultiplier;
	[DefaultValue(2.3f)] public float deathKnockbackForceMultiplier;
	[DefaultValue(3f)] public float deathKnockbackDelay;

	private NavMeshAgent agent;
	private Rigidbody rb;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		player = GameObject.FindWithTag("Player").transform;
		Utils.SetDefaultValues(this);
	}

	void Start() {
		agent = GetComponent<NavMeshAgent>();
		rb = GetComponent<Rigidbody>();

		agent.updatePosition = false;
		agent.updateRotation = false;
	}

	void FixedUpdate() {
		agent.SetDestination(player.position);
		Vector3 desiredVelocity = agent.desiredVelocity;

		if (rb.linearVelocity.magnitude < agent.speed) {
			rb.AddForce(desiredVelocity - rb.linearVelocity, ForceMode.VelocityChange);
		}

		if (desiredVelocity.magnitude > 0.1f) {
			Quaternion targetRotation = Quaternion.LookRotation(desiredVelocity);
			rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, agent.angularSpeed * Time.fixedDeltaTime));
		}

		agent.nextPosition = rb.position;
	}

	void LateUpdate() {
		agent.nextPosition = rb.position;
	}

	public void OnKnockback(Vector3 hitOrigin, float damage) {
		Vector3 force = (transform.position - hitOrigin).normalized;
		force.y = 0;
		rb.AddForce(damage * knockbackForceMultiplier * force, ForceMode.Impulse);
	}

	public void OnDie(Vector3 hitOrigin, float damage) {
		Vector3 force = (transform.position - hitOrigin).normalized;

		rb.constraints = RigidbodyConstraints.None;
		rb.AddForce(damage * knockbackForceMultiplier * deathKnockbackForceMultiplier * force, ForceMode.Impulse);

		Destroy(gameObject, deathKnockbackDelay);
	}
}
