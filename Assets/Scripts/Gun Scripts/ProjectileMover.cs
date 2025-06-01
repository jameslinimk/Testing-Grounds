using System;
using UnityEngine;

public class ProjectileMover : MonoBehaviour {
	public LayerMask hitLayers = 0;
	public float speed = 15f;
	public float lifetime = 5f;
	public float hitOffset = 0f;
	public Action<EnemyHealthController, Vector3> onEnemyHit;
	public bool UseFirePointRotation;
	public Vector3 rotationOffset = new(0, 0, 0);
	public GameObject hit;
	public GameObject flash;
	private Rigidbody rb;
	public GameObject[] Detached;

	void Start() {
		rb = GetComponent<Rigidbody>();
		if (flash != null) {
			var flashInstance = Instantiate(flash, transform.position, Quaternion.identity);
			flashInstance.transform.forward = gameObject.transform.forward;
			var flashPs = flashInstance.GetComponent<ParticleSystem>();
			if (flashPs != null) {
				Destroy(flashInstance, flashPs.main.duration);
			} else {
				var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
				Destroy(flashInstance, flashPsParts.main.duration);
			}
		}
		Destroy(gameObject, lifetime);
	}

	void FixedUpdate() {
		if (speed != 0) {
			rb.linearVelocity = transform.forward * speed;
			//transform.position += transform.forward * (speed * Time.deltaTime);
		}
	}

	//https ://docs.unity3d.com/ScriptReference/Rigidbody.OnCollisionEnter.html
	void OnCollisionEnter(Collision collision) {
		if (!hitLayers.ContainsLayer(collision.gameObject.layer)) return;

		if (collision.gameObject.TryGetComponent<EnemyHealthController>(out var enemy)) {
			onEnemyHit.Invoke(enemy, collision.contacts[0].point);
		}

		//Lock all axes movement and rotation
		rb.constraints = RigidbodyConstraints.FreezeAll;
		speed = 0;

		ContactPoint contact = collision.contacts[0];
		Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
		Vector3 pos = contact.point + contact.normal * hitOffset;

		if (hit != null) {
			var hitInstance = Instantiate(hit, pos, rot);
			if (UseFirePointRotation) { hitInstance.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0); } else if (rotationOffset != Vector3.zero) { hitInstance.transform.rotation = Quaternion.Euler(rotationOffset); } else { hitInstance.transform.LookAt(contact.point + contact.normal); }

			var hitPs = hitInstance.GetComponent<ParticleSystem>();
			if (hitPs != null) {
				Destroy(hitInstance, hitPs.main.duration);
			} else {
				var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
				Destroy(hitInstance, hitPsParts.main.duration);
			}
		}
		foreach (var detachedPrefab in Detached) {
			if (detachedPrefab != null) {
				detachedPrefab.transform.parent = null;
			}
		}
		Destroy(gameObject);
	}
}
