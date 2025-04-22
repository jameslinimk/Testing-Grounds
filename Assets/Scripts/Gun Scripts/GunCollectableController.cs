using System.Collections;
using UnityEngine;

public class GunCollectableController : MonoBehaviour {
	public GunSlot gunSlot;
	private Vector3 rotation;
	private Vector3 dropDirection;
	private Collider playerCollider;
	private new BoxCollider collider;
	private Rigidbody rb;

	private float startTime = 0f;
	private bool isGrounded = false;
	private float baseY = 0f;

	// Config
	private readonly float pickupDelay = 1.5f;
	private readonly float floatHeight = 0.5f;
	private readonly float floatFrequency = 1f;
	private readonly float dropSpeed = 20f;
	private readonly float upwardComponent = 8f;
	private readonly float linearDamping = 4f;

	public void Initialize(GunSlot gunSlot, Vector3 dropDirection, Collider playerCollider) {
		this.gunSlot = gunSlot.Clone();
		this.dropDirection = dropDirection;
		this.playerCollider = playerCollider;
	}

	public void Initialize(GunSlot gunSlot) {
		this.gunSlot = gunSlot.Clone();
	}

	void Start() {
		collider = GetComponent<BoxCollider>();
		collider.enabled = true;
		if (playerCollider != null) {
			Physics.IgnoreCollision(collider, playerCollider, true);
			StartCoroutine(CollisionCoroutine());
		}

		rb = gameObject.AddComponent<Rigidbody>();
		rb.linearDamping = linearDamping;
		if (dropDirection != null) rb.AddForce((dropDirection.normalized * dropSpeed) + (Vector3.up * upwardComponent), ForceMode.Impulse);

		rotation = new Vector3(Random.Range(-5, 5), Random.Range(15, 45), Random.Range(-5, 5));
	}

	IEnumerator CollisionCoroutine() {
		yield return new WaitForSeconds(pickupDelay);
		Physics.IgnoreCollision(collider, playerCollider, false);
	}

	void Update() {
		if (startTime == 0f) UpdateGrounded();
		transform.Rotate(rotation * Time.deltaTime);

		if (isGrounded && startTime == 0f) {
			rb.isKinematic = true;
			collider.isTrigger = true;
			startTime = Time.time;
			baseY = transform.position.y;
		}

		if (startTime != 0f) {
			float floatingY = floatHeight * 0.5f * (Mathf.Cos(((Time.time - startTime) * floatFrequency) + Mathf.PI) + 1);
			transform.position = new Vector3(transform.position.x, baseY + floatingY, transform.position.z);
		}
	}

	private void UpdateGrounded() {
		Vector3 boxCenter = transform.TransformPoint(collider.center);
		Vector3 halfExtents = Vector3.Scale(collider.size, transform.lossyScale) * 0.5f;

		isGrounded = Physics.Raycast(boxCenter, Vector3.down, out RaycastHit hit, halfExtents.y + 0.1f) && hit.collider != null;
	}

	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			var playerGunManager = other.GetComponent<PlayerGunManager>();
			if (playerGunManager.AddGun(gunSlot)) {
				Destroy(gameObject);
			}
		}
	}
}
