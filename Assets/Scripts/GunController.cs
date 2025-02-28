using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour {
	public GunConfig defaultGunConfig;
	public Transform player;
	public CameraController cameraController;
	[DefaultValue(17f)] public float rotationSpeed;

	private Vector3 offset;
	private Quaternion rotationOffset;
	private GunConfig config;
	private Transform firePoint;
	private GameObject weaponInstance;

	private InputAction shootAction;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

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
			targetPosition,
			Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed)
		);
	}
}
