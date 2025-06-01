using System;
using UnityEngine;

public class ProjectileController : MonoBehaviour {
	private LayerMask hitLayers;
	private Action<EnemyHealthController, Vector3> onEnemyHit;
	private Action<PlayerHealthController, Vector3> onPlayerHit;
	private bool enemy;

	private Vector3 direction;
	private float speed;

	public void Initialize(Vector3 direction, float speed, float lifetime, LayerMask hitLayers, Action<EnemyHealthController, Vector3> onEnemyHit) {
		this.hitLayers = hitLayers;
		this.onEnemyHit = onEnemyHit;
		enemy = false;

		this.direction = direction;
		this.speed = speed;

		Destroy(gameObject, lifetime);
	}

	public void Initialize(Vector3 direction, float speed, float lifetime, LayerMask hitLayers, Action<PlayerHealthController, Vector3> onPlayerHit) {
		this.hitLayers = hitLayers;
		this.onPlayerHit = onPlayerHit;
		enemy = true;

		this.direction = direction;
		this.speed = speed;

		Destroy(gameObject, lifetime);
	}

	void Update() {
		transform.position += speed * Time.deltaTime * direction;
	}

	void OnTriggerEnter(Collider other) {
		if (!enemy && other.gameObject.CompareTag("Player")) return;
		if (!hitLayers.ContainsLayer(other.gameObject.layer)) return;

		if (!enemy) {
			if (other.TryGetComponent<EnemyHealthController>(out var enemy)) {
				Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
				onEnemyHit.Invoke(enemy, hitPoint);
			}
		} else {
			if (other.TryGetComponent<PlayerHealthController>(out var player)) {
				Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
				onPlayerHit.Invoke(player, hitPoint);
			}
		}

		Destroy(gameObject);
	}
}
