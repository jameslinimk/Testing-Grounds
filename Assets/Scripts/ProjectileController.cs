using System;
using UnityEngine;

public class ProjectileController : MonoBehaviour {
	private float lifetime;
	private LayerMask hitLayers;
	private Action<EnemyHealthController, Vector3> onHitEnemy;

	private float shotTime;
	private bool LifeOver => Time.time - shotTime > lifetime;

	public void Initialize(Vector3 direction, float speed, float lifetime, LayerMask hitLayers, Action<EnemyHealthController, Vector3> onHitEnemy) {
		this.lifetime = lifetime;
		this.hitLayers = hitLayers;
		this.onHitEnemy = onHitEnemy;

		shotTime = Time.time;
		Rigidbody rb = GetComponent<Rigidbody>();

		rb.linearVelocity = direction * speed;
	}

	void Update() {
		if (LifeOver) Destroy(gameObject);
	}

	void OnCollisionEnter(Collision collision) {
		if (!hitLayers.ContainsLayer(collision.gameObject.layer)) return;

		if (collision.collider.TryGetComponent<EnemyHealthController>(out var enemy)) {
			Vector3 hitPoint = collision.contacts[0].point;
			onHitEnemy.Invoke(enemy, hitPoint);
		}

		Destroy(gameObject);
	}
}
