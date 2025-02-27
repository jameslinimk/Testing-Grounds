using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour {
	public GunConfig defaultGunConfig;
	public Transform player;
	public CameraController cameraController;
	[DefaultValue(8f)] public float rotationSpeed;
	[DefaultValue(10f)] public float positionSpeed;

	private Vector3 offset;
	private Quaternion rotationOffset;
	private GunConfig config;
	private Transform firePoint;
	private GameObject weaponInstance;

	private InputAction shootAction;

	void Start() {
		shootAction = InputSystem.actions.FindAction("Shoot");
		offset = transform.position - player.position;
		rotationOffset = Quaternion.Inverse(player.rotation) * transform.rotation;
		SwitchGun(defaultGunConfig);
	}

	[ContextMenu("Add default gun")]
	void AddDefaultGun() {
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

	void LateUpdate() {
		Vector3 targetPosition = player.position + cameraController.rotation * offset;
		Quaternion targetRotation = cameraController.rotation * rotationOffset;

		transform.SetPositionAndRotation(
			Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionSpeed),
			Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed)
		);

		// TODO gun lags behind player when moving
	}
}
