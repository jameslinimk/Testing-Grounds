using System.ComponentModel;
using UnityEngine;

public interface IKnockbackable {
	void OnKnockback(Vector3 hitOrigin, float damage);
	void OnDie(Vector3 hitOrigin, float damage);
}

public class EnemyHealthController : MonoBehaviour {
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
		health -= damage;

		if (health <= 0) knockbackable.OnDie(hitOrigin, damage);
		else knockbackable.OnKnockback(hitOrigin, damage);
	}
}
