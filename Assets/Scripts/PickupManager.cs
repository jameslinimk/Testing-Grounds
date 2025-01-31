using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PickupManager : MonoBehaviour {
	public GameObject pickupPrefab;
	private BoxCollider spawnArea;

	private float lastSpawn = -Mathf.Infinity;
	public float spawnCooldown;

	void Start() {
		spawnArea = GetComponent<BoxCollider>();
	}

	void Update() {
		if (Time.time - lastSpawn < spawnCooldown) return;
		lastSpawn = Time.time;

		Instantiate(pickupPrefab, GetRandomPosition(), Quaternion.identity, transform);
	}

	private Vector3 GetRandomPosition() {
		Vector3 size = spawnArea.size;
		Vector3 center = spawnArea.center;

		return new Vector3(
			Random.Range(center.x - size.x / 2, center.x + size.x / 2),
			Random.Range(center.y - size.y / 2, center.y + size.y / 2),
			Random.Range(center.z - size.z / 2, center.z + size.z / 2)
		);
	}
}
