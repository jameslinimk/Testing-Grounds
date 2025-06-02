using System.ComponentModel;
using UnityEngine;

public interface IKnockbackable {
	void OnKnockback(Vector3 hitOrigin, float damage);
	void OnDie(Vector3 hitOrigin);
}

public class EnemyHealthController : MonoBehaviour {
	public bool spawning = true;
	private IKnockbackable knockbackable;
	[DefaultValue(100f)] public float maxHealth;
	[DefaultValue(100f)] public float health;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	void Start() {
		knockbackable = GetComponent<IKnockbackable>();
	}

	public void TakeDamage(float damage, Vector3 hitOrigin) {
		if (spawning) return;
		health -= damage;

		if (health <= 0) knockbackable.OnDie(hitOrigin);
		else knockbackable.OnKnockback(hitOrigin, damage);
	}
}
