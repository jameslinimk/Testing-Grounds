using System.Collections;
using UnityEngine;

public class GunCollectableController : MonoBehaviour {
	public GunSlot gunSlot;
	private float startTime = 0f;
	private Vector3 rotation;
	private Vector3 dropDirection;
	private Collider playerCollider;
	private new BoxCollider collider;
	private Rigidbody rb;

	private bool isGrounded = false;

	private readonly float pickupDelay = 1.5f;
	private readonly float floatHeight = 0.5f;
	private readonly float floatFrequency = 1f;
	private readonly float dropSpeed = 10f;
	private readonly float upwardComponent = 4f;

	void Start() {
		collider = GetComponent<BoxCollider>();
		collider.enabled = true;
		Physics.IgnoreCollision(collider, playerCollider, true);
		StartCoroutine(CollisionCoroutine());

		rb = gameObject.AddComponent<Rigidbody>();
		rb.AddForce((dropDirection.normalized * dropSpeed) + (Vector3.up * upwardComponent), ForceMode.Impulse);

		rotation = new Vector3(
			Random.Range(5, 10),
			Random.Range(15, 45),
			Random.Range(5, 10)
		);
	}

	IEnumerator CollisionCoroutine() {
		yield return new WaitForSeconds(pickupDelay);
		var collider = GetComponent<BoxCollider>();
		Physics.IgnoreCollision(collider, playerCollider, false);
	}

	void Update() {
		UpdateGrounded();
		transform.Rotate(rotation * Time.deltaTime);

		if (isGrounded && rb.linearVelocity.magnitude < 0.1f) {
			if (startTime == 0f) {
				Debug.Log("GunCollectableController: Rigidbody is not moving, starting floating behavior.");

				rb.isKinematic = true;
				collider.isTrigger = true;
				startTime = Time.time;
			}

			float floatingY = floatHeight * (Mathf.Sin(((Time.time - startTime) * floatFrequency) + Mathf.PI) + 1);
			transform.position = new Vector3(transform.position.x, floatingY, transform.position.z);
		}
	}

	private void UpdateGrounded() {
		Vector3 boxCenter = transform.TransformPoint(collider.center);
		Vector3 halfExtents = Vector3.Scale(collider.size, transform.lossyScale) * 0.5f;
		Vector3 direction = Vector3.down;

		isGrounded = Physics.BoxCast(boxCenter, halfExtents, direction, out _, transform.rotation, 0.1f);
	}

	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			Debug.Log($"GunCollectableController: OnTriggerEnter with {other.name}");

			var playerGunManager = other.GetComponent<PlayerGunManager>();
			if (playerGunManager.AddGun(gunSlot)) {
				Destroy(gameObject);
			}
		}
	}

	public void Initialize(GunSlot gunSlot, Vector3 dropDirection, Collider playerCollider) {
		this.gunSlot = gunSlot.Clone();
		this.dropDirection = dropDirection;
		this.playerCollider = playerCollider;
	}
}
