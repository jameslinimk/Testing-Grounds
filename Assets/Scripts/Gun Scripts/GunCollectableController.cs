using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Settings/GunCollectableSettings")]
public class GunCollectableSettings : ScriptableObject {
	public float pickupDelay = 1.5f;
	public float floatHeight = 0.5f;
	public float floatFrequency = 1f;
	public float dropSpeed = 20f;
	public float upwardComponent = 8f;
	public float linearDamping = 4f;
}

public class GunCollectableController : MonoBehaviour {
	public GunCollectableSettings settings;
	public GunSlot gunSlot;
	public Collider playerCollider;

	private Vector3 rotation;
	private Vector3 dropDirection;
	private new BoxCollider collider;
	private Rigidbody rb;

	private float startTime = 0f;
	private bool isGrounded = false;
	private float baseY = 0f;

	public void Initialize(GunSlot gunSlot, Vector3 dropDirection, Collider playerCollider, GunCollectableSettings settings = null) {
		this.gunSlot = gunSlot.Clone();
		this.dropDirection = dropDirection;
		this.playerCollider = playerCollider;
		this.settings = settings;
	}

	public void Initialize(GunSlot gunSlot, GunCollectableSettings settings = null) {
		this.gunSlot = gunSlot.Clone();
		this.settings = settings;
	}

	void Start() {
		if (settings == null) settings = Resources.Load<GunCollectableSettings>("Settings/GunCollectableSettings");
		transform.localScale = Vector3.one * GunController.GunPrefabScale;

		collider = GetComponent<BoxCollider>();
		collider.enabled = true;
		if (playerCollider != null) {
			Physics.IgnoreCollision(collider, playerCollider, true);
			StartCoroutine(CollisionCoroutine());
		}

		rb = gameObject.AddComponent<Rigidbody>();
		rb.linearDamping = settings.linearDamping;
		if (dropDirection != null) rb.AddForce((dropDirection.normalized * settings.dropSpeed) + (Vector3.up * settings.upwardComponent), ForceMode.Impulse);

		rotation = new Vector3(Random.Range(-5, 5), Random.Range(15, 45), Random.Range(-5, 5));
	}

	IEnumerator CollisionCoroutine() {
		yield return new WaitForSeconds(settings.pickupDelay);
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
			float floatingY = settings.floatHeight * 0.5f * (Mathf.Cos(((Time.time - startTime) * settings.floatFrequency) + Mathf.PI) + 1);
			transform.position = new Vector3(transform.position.x, baseY + floatingY, transform.position.z);
		}
	}

	private void UpdateGrounded() {
		Vector3 boxCenter = transform.TransformPoint(collider.center);
		Vector3 halfExtents = Vector3.Scale(collider.size, transform.lossyScale) * 0.5f;

		isGrounded = Physics.Raycast(boxCenter, Vector3.down, out RaycastHit hit, halfExtents.y + 0.1f) && hit.collider != null;
	}
}
