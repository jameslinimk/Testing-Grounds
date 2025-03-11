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
	[DefaultValue(true)] public bool autoReload;

	private Rigidbody playerRb;

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

		playerRb = player.GetComponent<Rigidbody>();
	}

	[ContextMenu("Add default gun")]
	void AddDefaultGun() {
		SwitchGun(defaultGunConfig);
	}

	private bool switchingGuns;
	private Coroutine switchGunCoroutine;
	private void StopSwitchGunCoroutine() {
		if (switchGunCoroutine == null) return;
		StopCoroutine(switchGunCoroutine);
		switchingGuns = false;
	}

	public void SwitchGun(GunSlot gunSlot) {
		if (gunSlot.CanPocketReload) gunSlot.Reload();
		SwitchGun(gunSlot.config, gunSlot.currentAmmo);
	}

	// setAmmo == null also switches instant
	public void SwitchGun(GunConfig newGun, int? setAmmo = null) {
		if (weaponInstance != null) Destroy(weaponInstance);

		if (bursting) StopBurstCoroutine();
		if (reloading) StopReloadCoroutine();
		if (switchingGuns) StopSwitchGunCoroutine();

		// Switch before for UI
		config = newGun;
		ammo = setAmmo ?? config.maxAmmo;

		switchGunCoroutine = StartCoroutine(SwitchGunCoroutine(config, setAmmo == null));
	}

	IEnumerator SwitchGunCoroutine(GunConfig config, bool instant = false) {
		switchingGuns = true;
		if (!instant) yield return new WaitForSeconds(config.drawSpeed);
		switchingGuns = false;

		currentSpread = config.spread;

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
		if (config.fireType.Is(FireType.BurstAuto, FireType.Auto) && shootAction.IsInProgress()) OnShoot();

		// Bloom reset
		if (Time.time - lastShot >= config.bloomCooldownDelay) {
			currentSpread = Mathf.MoveTowards(currentSpread, config.spread, config.bloomCooldownRate * Time.deltaTime);
		}

		CrosshairController.Instance.UpdateOuterCircleSize(currentSpread);
	}

	/* -------------------------------- Shooting -------------------------------- */

	private bool bursting = false;
	private Coroutine burstCoroutine;
	private void StopBurstCoroutine() {
		if (burstCoroutine == null) return;
		StopCoroutine(burstCoroutine);
		bursting = false;
	}

	private float lastShot = -Mathf.Infinity;
	private bool CanShoot => ammo > 0 && Time.time - lastShot >= config.fireCooldown && !switchingGuns && !bursting && !reloading;

	void OnShoot() {
		if (ammo == 0 && Time.time - lastShot >= config.fireCooldown) OnReload();
		if (GameManager.Instance.IsPaused || !CanShoot) return;

		lastShot = Time.time;
		ammo--;

		var (position, lastLook) = cameraController.IsFreelooking ? cameraController.ShootReset() : (cameraController.transform.position, cameraController.transform.forward);
		Vector3 targetPoint;
		if (Physics.Raycast(position, lastLook, out RaycastHit cameraHit, camRange, hitLayers)) {
			targetPoint = cameraHit.point;
		} else {
			targetPoint = position + lastLook * fallbackDistance;
		}

		// Real raycast from gun
		Vector3 directionFromGun = (targetPoint - firePoint.position).normalized;
		if (config.burstCount == 0) {
			ShootBullets(directionFromGun);
		} else {
			burstCoroutine = StartCoroutine(BurstCoroutine(directionFromGun));
		}
	}

	IEnumerator BurstCoroutine(Vector3 directionFromGun) {
		bursting = true;
		for (int i = 0; i < config.burstCount; i++) {
			ShootBullets(directionFromGun);
			yield return new WaitForSeconds(config.burstDelay);
		}
		lastShot = Time.time;
		bursting = false;
	}

	/* ------------------------ Bloom/recoil/kickback/etc ----------------------- */
	private float currentSpread = 0f;

	private Vector3 ApplySpread(Vector3 direction, float spread) {
		return Quaternion.Euler(
			Random.Range(-spread, spread),
			Random.Range(-spread, spread),
			0f
		) * direction;
	}

	private void ShootBullets(Vector3 directionFromGun) {
		currentSpread += config.bloom;
		currentSpread = Mathf.Clamp(currentSpread, 0f, config.maxBloom);

		for (int i = 0; i < config.bullets; i++) {
			CrosshairController.Instance.PulseCrosshair(config.burstCount);
			Vector3 newDir = ApplySpread(directionFromGun, currentSpread);

			if (!config.isProjectile) {
				Debug.DrawRay(firePoint.position, newDir * config.range, Color.red, 0.5f);
				if (!Physics.Raycast(firePoint.position, newDir, out RaycastHit hit, config.range, hitLayers)) continue;
				if (!hit.collider.TryGetComponent<EnemyHealthController>(out var enemy)) continue;

				ApplyDamage(enemy, hit.point);
			} else {
				ProjectileInfo info = config.projectileInfo;
				GameObject projectile = Instantiate(info.projectilePrefab, firePoint.position, Quaternion.LookRotation(newDir));
				projectile.GetComponent<ProjectileController>().Initialize(newDir, info.speed, info.lifetime, hitLayers, ApplyDamage);
			}
		}

		Vector3 kickback = -config.kickback * directionFromGun;
		playerRb.AddForce(kickback, ForceMode.Impulse);
	}

	private void ApplyDamage(EnemyHealthController enemy, Vector3 hitPoint) {
		enemy.TakeDamage(config.damage, hitPoint);
	}

	/* ----------------------------- Ammo/reloading ----------------------------- */

	public int ammo { get; private set; } = 0;
	public float reloadStart { get; private set; } = -Mathf.Infinity;

	private bool reloading = false;
	private Coroutine reloadCoroutine;
	private void StopReloadCoroutine() {
		if (reloadCoroutine == null) return;
		StopCoroutine(reloadCoroutine);
		reloading = false;
		reloadStart = -Mathf.Infinity;
	}

	void OnReload() {
		if (bursting) StopBurstCoroutine();
		if (reloading || switchingGuns || ammo == config.maxAmmo) return;
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
