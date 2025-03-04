using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour {
	public GunConfig defaultGunConfig;
	public LayerMask hitLayers;
	public Transform player;
	public CameraController cameraController;
	[DefaultValue(17f)] public float rotationSpeed;

	private Vector3 offset;
	private Quaternion rotationOffset;
	private float cameraToFirepointDistance;
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
		shootAction.performed += _ => OnShoot();

		offset = transform.position - player.position;
		rotationOffset = transform.rotation;
		SwitchGun(defaultGunConfig);
	}

	[ContextMenu("Add default gun")]
	void AddDefaultGun() {
		SwitchGun(defaultGunConfig);
	}

	public void SwitchGun(GunConfig newGun, int? setAmmo = null) {
		if (weaponInstance != null) Destroy(weaponInstance);

		config = newGun;
		ammo = setAmmo ?? config.maxAmmo;

		weaponInstance = Instantiate(newGun.weaponPrefab, transform.position, transform.rotation, transform);
		firePoint = weaponInstance.transform.Find("Firepoint");
		if (firePoint == null) Debug.LogError("FirePoint not found in weapon prefab.");
		cameraToFirepointDistance = Vector3.Distance(cameraController.transform.position, firePoint.position);
	}

	void LateUpdate() {
		Vector3 targetPosition = player.position + cameraController.RealRotation * offset;
		Quaternion targetRotation = cameraController.RealRotation * rotationOffset;

		transform.SetPositionAndRotation(
			targetPosition,
			Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed)
		);
	}

	void Update() {
		if (config.fireType != FireType.Single && shootAction.IsInProgress()) OnShoot();
	}

	private float lastShot = -Mathf.Infinity;
	private bool CanShoot => Time.time - lastShot >= config.fireCooldown && ammo > 0;
	private int ammo = 0;

	void OnShoot() {
		if (GameManager.Instance.IsPaused || !CanShoot) return;
		cameraController.OnShoot(); // Prevents freelook while shooting
		lastShot = Time.time;

		Debug.Log("shooting");

		// Getting direction gun is pointing
		float camRange = config.range + cameraToFirepointDistance + 2f; // Overshoot
		if (!Physics.Raycast(cameraController.transform.position, cameraController.transform.forward, out RaycastHit cameraHit, camRange, hitLayers)) return;
		Vector3 directionFromGun = (cameraHit.point - firePoint.position).normalized;

		// Real raycast
		Debug.DrawRay(firePoint.position, directionFromGun * config.range, Color.red, 0.5f);
		if (Physics.Raycast(firePoint.position, directionFromGun, out RaycastHit hit, config.range, hitLayers)) {
			Debug.Log("Applying damage to " + hit.collider.name);
		}
	}
}
