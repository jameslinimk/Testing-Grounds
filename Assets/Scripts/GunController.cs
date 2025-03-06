using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour {
	public GunConfig defaultGunConfig;
	public LayerMask hitLayers;
	public Transform player;
	public CameraController cameraController;
	public PlayerUIController playerUIController;
	[DefaultValue(17f)] public float rotationSpeed;
	[DefaultValue(300f)] public float fallbackDistance;

	private Vector3 offset;
	private Quaternion rotationOffset;
	private float camRange = 0f;
	public GunConfig config { get; private set; }
	private Transform firePoint;
	private GameObject weaponInstance;

	private InputAction shootAction;
	private InputAction reloadAction;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	void Start() {
		shootAction = InputSystem.actions.FindAction("Shoot");
		reloadAction = InputSystem.actions.FindAction("Reload");

		shootAction.performed += _ => OnShoot();
		reloadAction.performed += _ => OnReload();

		offset = transform.position - player.position;
		rotationOffset = transform.rotation;
		SwitchGun(defaultGunConfig);
	}

	[ContextMenu("Add default gun")]
	void AddDefaultGun() {
		SwitchGun(defaultGunConfig);
	}

	private bool switchingGuns;
	private Coroutine switchGunCoroutine;

	public void SwitchGun(GunSlot gunSlot) {
		if (gunSlot.CanPocketReload) gunSlot.Reload();
		SwitchGun(gunSlot.config, gunSlot.currentAmmo);
	}

	// setAmmo == null also switches instant
	public void SwitchGun(GunConfig newGun, int? setAmmo = null) {
		if (weaponInstance != null) Destroy(weaponInstance);
		if (reloading) {
			StopCoroutine(reloadCoroutine);
			reloading = false;
			reloadStart = -Mathf.Infinity;
		}
		if (switchingGuns) {
			StopCoroutine(switchGunCoroutine);
			switchingGuns = false;
		}

		config = newGun;
		ammo = setAmmo ?? config.maxAmmo;
		playerUIController.RefreshAmmoText();

		switchGunCoroutine = StartCoroutine(SwitchGunCoroutine(config, setAmmo == null));
	}

	IEnumerator SwitchGunCoroutine(GunConfig config, bool instant = false) {
		switchingGuns = true;
		if (!instant) yield return new WaitForSeconds(config.drawSpeed);
		switchingGuns = false;

		weaponInstance = Instantiate(config.weaponPrefab, transform.position, transform.rotation, transform);
		firePoint = weaponInstance.transform.Find("Firepoint");
		if (firePoint == null) Debug.LogError("FirePoint not found in weapon prefab.");

		if (camRange == 0f) camRange = config.range + Vector3.Distance(cameraController.transform.position, firePoint.position) + 2f;
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

	void OnShoot() {
		if (GameManager.Instance.IsPaused || !CanShoot) return;
		lastShot = Time.time;
		ammo--;
		playerUIController.RefreshAmmoText();

		var (position, lastLook) = cameraController.IsFreelooking ? cameraController.ShootReset() : (cameraController.transform.position, cameraController.transform.forward);
		Vector3 targetPoint;
		if (Physics.Raycast(position, lastLook, out RaycastHit cameraHit, camRange, hitLayers)) {
			targetPoint = cameraHit.point;
		} else {
			targetPoint = position + lastLook * fallbackDistance;
		}

		// Real raycast from gun
		Vector3 directionFromGun = (targetPoint - firePoint.position).normalized;
		Debug.DrawRay(firePoint.position, directionFromGun * config.range, Color.red, 0.5f);
		if (Physics.Raycast(firePoint.position, directionFromGun, out RaycastHit hit, config.range, hitLayers)) {
			if (hit.collider.TryGetComponent<EnemyHealthController>(out var enemy)) {
				enemy.TakeDamage(config.damage, hit.point);
				Debug.Log($"Hit {hit.collider.name} for {config.damage} damage.");
			}
		}
	}

	public int ammo { get; private set; } = 0;
	private bool reloading = false;
	public float reloadStart { get; private set; } = -Mathf.Infinity;
	private Coroutine reloadCoroutine;

	void OnReload() {
		if (reloading || ammo == config.maxAmmo) return;
		reloadCoroutine = StartCoroutine(ReloadCoroutine());
	}

	IEnumerator ReloadCoroutine() {
		reloadStart = Time.time;

		reloading = true;
		yield return new WaitForSeconds(config.reloadTime);
		reloading = false;

		ammo = config.maxAmmo;
	}
}
