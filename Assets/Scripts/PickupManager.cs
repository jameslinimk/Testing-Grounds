using UnityEngine;

public class PickupManager : MonoBehaviour {
	public GameObject pickupPrefab;
	public GameObject spawnArea;

	private float lastSpawn = -Mathf.Infinity;
	public float spawnCooldown;

	void Update() {
		if (Time.time - lastSpawn < spawnCooldown) return;
		lastSpawn = Time.time;

		Instantiate(pickupPrefab, new Vector3(Random.Range(-4, 4), 0.85f, Random.Range(-4, 4)), Quaternion.identity);
	}
}
