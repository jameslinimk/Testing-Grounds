using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour, IKnockbackable {
	public Func<Vector3> GetTarget;

	public AnimationCurve knockbackCurve;
	[DefaultValue(25f)] public float maxKnockbackDamage;

	private NavMeshAgent agent;
	private Rigidbody rb;
	public Animator Animator { get; private set; }

	private bool paused = true;

	private bool dead = false;
	private float knockbackStart = 0f;
	[DefaultValue(0.1f)] public float minimumKnockbackDuration;
	private bool GracePeriodActive => Time.time - knockbackStart < minimumKnockbackDuration;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	void Start() {
		agent = GetComponent<NavMeshAgent>();
		rb = GetComponent<Rigidbody>();

		GameObject model = transform.GetChild(1).gameObject;

		Animator = model.GetComponent<Animator>();
		Animator.SetFloat("RandomWalk", UnityEngine.Random.Range(1, 4));
		Animator.SetFloat("RandomAttack", UnityEngine.Random.Range(1, 6));
		Animator.SetFloat("RandomStand", UnityEngine.Random.Range(1, 4));

		// Randomly activate one of the child objects (zombie models)
		int randomIndex = UnityEngine.Random.Range(1, transform.childCount);
		for (int i = 1; i < transform.childCount; i++) {
			Transform child = transform.GetChild(i);
			child.gameObject.SetActive(i == randomIndex);
		}

		rb.isKinematic = true;
		agent.enabled = false;
		GameManager.OnPauseChange += OnPauseChange;
	}

	// Real start
	public void StandFinish() {
		Debug.Log("Stand finish called");
		ToggleAgent(true);
		paused = false;
	}

	private void ToggleAgent(bool on) {
		agent.enabled = on;
		rb.isKinematic = on;
	}

	void FixedUpdate() {
		if (GameManager.Instance.IsPaused || paused) return;

		if (agent.enabled) return;
		if (rb.linearVelocity.magnitude < 0.05f && !GracePeriodActive) {
			rb.linearVelocity = Vector3.zero;
			ToggleAgent(true);
			if (dead) Destroy(gameObject);
		}
	}

	void OnPauseChange() {
		agent.enabled = !GameManager.Instance.IsPaused;
		if (agent.enabled) {
			rb.isKinematic = false;
			knockbackStart = Time.time;
		} else {
			rb.linearVelocity = Vector3.zero;
			rb.isKinematic = true;
		}
	}

	void Update() {
		if (agent.enabled && GetTarget != null) agent.SetDestination(GetTarget());
	}

	public void OnKnockback(Vector3 hitOrigin, float damage) {
		ToggleAgent(false);

		knockbackStart = Time.time;
		Vector3 force = transform.position - hitOrigin;
		force.y = 0;
		force.Normalize();

		force *= knockbackCurve.Evaluate(damage / maxKnockbackDamage);
		rb.AddForce(force, ForceMode.Impulse);
	}

	public void OnDie(Vector3 hitOrigin, float damage) {
		dead = true;
		ToggleAgent(false);
		rb.isKinematic = true;
		paused = true;

		// Rotate away from hit origin
		Vector3 direction = hitOrigin - transform.position;
		direction.y = 0f;

		// Only rotate if direction has magnitude
		if (direction != Vector3.zero) {
			Quaternion lookRotation = Quaternion.LookRotation(direction);
			transform.rotation = lookRotation;
		}

		Animator.SetTrigger("Die");
	}

	public void ZombieDeath() {
		Destroy(gameObject);
	}
}
