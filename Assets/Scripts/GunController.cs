using UnityEngine;

public class GunController : MonoBehaviour {
	public GunConfig defaultGunConfig;
	public Transform player;

	private Vector3 offset;
	private GunConfig config;
	private Transform firePoint;
	private GameObject weaponInstance;

	void Start() {
		offset = transform.position - player.position;
		SwitchGun(defaultGunConfig);
	}

	public void SwitchGun(GunConfig newGun) {
		if (weaponInstance != null) Destroy(weaponInstance);

		config = newGun;
		weaponInstance = Instantiate(newGun.weaponPrefab, transform.position, transform.rotation, transform);
		firePoint = weaponInstance.transform.Find("Firepoint");
		if (firePoint == null) {
			Debug.LogError("FirePoint not found in weapon prefab.");
		}
	}

	void Update() {
		transform.position = player.position + offset;
	}
}
