using System;
using UnityEngine;

public class ProjectileController : MonoBehaviour {
	private LayerMask hitLayers;
	private Action<EnemyHealthController, Vector3> onEnemyHit;
	private Action<PlayerHealthController, Vector3> onPlayerHit;
	private bool enemy;

	public void Initialize(Vector3 direction, float speed, float lifetime, LayerMask hitLayers, Action<EnemyHealthController, Vector3> onEnemyHit) {
		this.hitLayers = hitLayers;
		this.onEnemyHit = onEnemyHit;
		enemy = false;

		Rigidbody rb = GetComponent<Rigidbody>();

		rb.linearVelocity = direction * speed;
		Destroy(gameObject, lifetime);
	}

	public void Initialize(Vector3 direction, float speed, float lifetime, LayerMask hitLayers, Action<PlayerHealthController, Vector3> onPlayerHit) {
		this.hitLayers = hitLayers;
		this.onPlayerHit = onPlayerHit;
		enemy = true;

		Rigidbody rb = GetComponent<Rigidbody>();

		rb.linearVelocity = direction * speed;
		Destroy(gameObject, lifetime);
	}

	void OnCollisionEnter(Collision collision) {
		if (!hitLayers.ContainsLayer(collision.gameObject.layer)) return;

		if (!enemy) {
			if (collision.collider.TryGetComponent<EnemyHealthController>(out var enemy)) {
				Vector3 hitPoint = collision.contacts[0].point;
				onEnemyHit.Invoke(enemy, hitPoint);
			}
		} else {
			if (collision.collider.TryGetComponent<PlayerHealthController>(out var player)) {
				Vector3 hitPoint = collision.contacts[0].point;
				onPlayerHit.Invoke(player, hitPoint);
			}
		}

		Destroy(gameObject);
	}
}
