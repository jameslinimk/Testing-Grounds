using System.ComponentModel;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour {
	private PlayerController pc;
	private Rigidbody rb;
	private Animator animator;

	[Header("Health Settings")]
	[DefaultValue(100f)] public float maxHealth;
	[DefaultValue(100f)] public float health;
	public bool IsDead => health <= 0;

	public AnimationCurve hitKnockbackCurve;
	[DefaultValue(10f)] public float hitKnockbackCurveScale;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	void Start() {
		animator = transform.GetChild(0).GetComponent<Animator>();
		pc = GetComponent<PlayerController>();
		rb = GetComponent<Rigidbody>();
	}

	public void TakeDamage(float damage, Vector3 hitOrigin) {
		health -= damage;

		if (!pc.IsDashing) {
			Vector3 directionAwayFromHit = (transform.position - hitOrigin).normalized;

			float intensity = hitKnockbackCurve.Evaluate(damage / hitKnockbackCurveScale);
			rb.AddForce(directionAwayFromHit * intensity, ForceMode.Impulse);

			Vector3 localHitDirection = transform.InverseTransformDirection(directionAwayFromHit);

			animator.SetFloat("HitHorizontal", localHitDirection.x);
			animator.SetFloat("HitVertical", localHitDirection.z);
			animator.SetTrigger("Hit");
		}

		if (health <= 0) Die(hitOrigin);
	}

	private bool alreadyDied = false;

	public void Die(Vector3? hitOrigin = null) {
		if (alreadyDied) return;
		alreadyDied = true;

		Vector3 directionAwayFromHit = (hitOrigin == null) ? transform.forward : (transform.position - (Vector3)hitOrigin).normalized;

		Vector3 localDeathDirection = transform.InverseTransformDirection(directionAwayFromHit);
		Vector2 closestDir = Utils.GetClosestCardinalDirection(new Vector2(localDeathDirection.x, localDeathDirection.z));

		animator.SetFloat("DeathHorizontal", closestDir.x);
		animator.SetFloat("DeathVertical", closestDir.y);
		animator.SetTrigger("Die");

		rb.isKinematic = true;
		GameManager.Instance.EndGame();
	}
}
