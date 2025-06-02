using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour, IKnockbackable {
	public Func<Vector3> GetTarget;

	public AnimationCurve knockbackCurve;
	[DefaultValue(25f)] public float maxKnockbackDamage;
	[DefaultValue(5f)] public float maxKnockbackForce;

	private float speed = 0f;
	[DefaultValue(0.65f)] public float attackingSpeedModifier;

	private NavMeshAgent agent;
	private Rigidbody rb;
	public Animator Animator { get; private set; }
	private EnemyHealthController health;
	private new Collider collider;

	private bool paused = true;

	private bool dead = false;
	private float knockbackStart = 0f;
	[DefaultValue(0.1f)] public float minimumKnockbackDuration;
	private bool GracePeriodActive => Time.time - knockbackStart < minimumKnockbackDuration;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	public void Initialize(float speed, float damage) {
		if (TryGetComponent<MeleeEnemyController>(out var meleeController)) {
			meleeController.damage = damage;
		}

		this.speed = speed;
	}

	void Start() {
		agent = GetComponent<NavMeshAgent>();
		rb = GetComponent<Rigidbody>();
		health = GetComponent<EnemyHealthController>();
		collider = GetComponent<Collider>();

		Transform model = transform.GetChild(1);

		Animator = model.GetComponent<Animator>();
		Animator.SetFloat("RandomWalk", UnityEngine.Random.Range(1, 4));
		Animator.SetFloat("RandomAttack", UnityEngine.Random.Range(1, 6));
		Animator.SetFloat("RandomStand", UnityEngine.Random.Range(1, 4));

		// Randomly activate one of the child objects (zombie models)
		int randomIndex = UnityEngine.Random.Range(1, model.childCount);
		for (int i = 1; i < model.childCount; i++) {
			model.GetChild(i).gameObject.SetActive(i == randomIndex);
		}

		rb.isKinematic = true;
		agent.enabled = false;
		GameManager.OnPauseChange += OnPauseChange;
		GameManager.EnemiesDie += EnemiesDie;

		if (speed == 0f) speed = agent.speed;
	}

	// Real start
	public void StandFinish() {
		ToggleAgent(true);
		paused = false;
		health.spawning = false;
	}

	private void ToggleAgent(bool on) {
		agent.enabled = on;
		rb.isKinematic = on;
	}

	void FixedUpdate() {
		if (GameManager.Instance.IsPaused || paused) return;

		if (agent.enabled) {
			bool attacking = Animator.GetBool("Attacking");
			agent.speed = attacking ? speed * attackingSpeedModifier : speed;
			return;
		}
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

		force *= knockbackCurve.Evaluate(damage / maxKnockbackDamage) * maxKnockbackForce;
		Debug.Log($"{damage} -> {force.magnitude}");
		rb.AddForce(force, ForceMode.Impulse);
	}

	public void OnDie(Vector3 hitOrigin) {
		if (dead) return;
		collider.isTrigger = true;

		dead = true;
		ToggleAgent(false);
		rb.isKinematic = true;
		paused = true;

		// Face hit origin
		Vector3 direction = hitOrigin - transform.position;
		direction.y = 0; // Keep rotation only around Y axis
		if (direction != Vector3.zero) {
			transform.rotation = Quaternion.LookRotation(direction);
		}

		Animator.SetTrigger("Die");
	}

	public void EnemiesDie() {
		OnDie(Vector3.zero);
	}

	public void ZombieDeath() {
		GameManager.Instance.OnEnemyDead();
		Destroy(gameObject);
	}
}
