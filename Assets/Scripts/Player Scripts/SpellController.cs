using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellController : MonoBehaviour {
	public LayerMask hitLayers;
	private PlayerController pc;
	private new Collider collider;
	public CameraController cameraController;
	public PlayerUIController playerUIController;
	[DefaultValue(17f)] public float rotationSpeed;
	[DefaultValue(300f)] public float fallbackDistance;
	[DefaultValue(true)] public bool autoReload;

	public Transform rightHand;
	public Transform leftHand;

	public Vector3 CalculateFirePoint() {
		if (!Config.IsTwoHanded) return rightHand.position;
		return (rightHand.position + leftHand.position) / 2f;
	}

	private Rigidbody rb;

	private float camRange = 0f;
	public GunConfig Config { get; private set; }

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

		rb = GetComponent<Rigidbody>();
		pc = GetComponent<PlayerController>();
		collider = GetComponent<Collider>();
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

	/// <summary>
	/// If setAmmo is null, it will switch guns instantly
	/// </summary>
	public void SwitchGun(GunConfig newGun, int? setAmmo = null) {
		if (bursting) StopBurstCoroutine();
		if (reloading) StopReloadCoroutine();
		if (switchingGuns) StopSwitchGunCoroutine();

		// Switch before for UI
		Config = newGun;
		ammo = setAmmo ?? Config.maxAmmo;

		switchGunCoroutine = StartCoroutine(SwitchGunCoroutine(Config, setAmmo == null));
	}

	IEnumerator SwitchGunCoroutine(GunConfig config, bool instant = false) {
		switchingGuns = true;
		if (!instant) yield return new WaitForSeconds(config.drawSpeed);
		switchingGuns = false;

		currentSpread = config.spread;

		if (camRange == 0f) camRange = (config.speed * config.lifetime) + Vector3.Distance(cameraController.transform.position, CalculateFirePoint()) + 2f;
	}

	void Update() {
		if (Config.fireType.Is(FireType.BurstAuto, FireType.Auto) && shootAction.IsInProgress()) OnShoot();

		// Bloom reset
		if (Time.time - lastShot >= Config.bloomCooldownDelay) {
			currentSpread = Mathf.MoveTowards(currentSpread, Config.spread, Config.bloomCooldownRate * Time.deltaTime);
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
	private bool waitingForShot = false;
	public bool IsCasting => waitingForShot;
	private bool CanShoot => ammo > 0 && Time.time - lastShot >= Config.fireCooldown && !switchingGuns && !bursting && !reloading && !pc.IsDashing && !pc.Sprinting && !waitingForShot;

	public void FireProjectile() {
		if (!waitingForShot) return;

		Debug.Log("Firing projectile");

		waitingForShot = false;

		lastShot = Time.time;
		ammo--;

		Vector3 directionFromGun = CalculateLookDirection();
		if (Config.burstCount == 0) {
			ShootBullets(directionFromGun);
		} else {
			burstCoroutine = StartCoroutine(BurstCoroutine(directionFromGun));
		}
	}

	void OnShoot() {
		if (ammo == 0 && Time.time - lastShot >= Config.fireCooldown) OnReload();
		if (GameManager.Instance.IsPaused || !CanShoot) return;

		pc.Animator.SetTrigger("Attack");
		waitingForShot = true;
	}

	public Vector3 CalculateLookDirection(bool resetFreelook = true) {
		var (position, lastLook) = (resetFreelook && cameraController.IsFreelooking) ? cameraController.ShootReset() : (cameraController.transform.position, cameraController.transform.forward);
		Vector3 targetPoint;
		if (Physics.Raycast(position, lastLook, out RaycastHit cameraHit, camRange, hitLayers)) {
			targetPoint = cameraHit.point;
		} else {
			targetPoint = position + lastLook * fallbackDistance;
		}

		// Real raycast from gun
		return (targetPoint - CalculateFirePoint()).normalized;
	}

	IEnumerator BurstCoroutine(Vector3 directionFromGun) {
		bursting = true;
		for (int i = 0; i < Config.burstCount; i++) {
			ShootBullets(directionFromGun);
			yield return new WaitForSeconds(Config.burstDelay);
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
		currentSpread += Config.bloom;
		currentSpread = Mathf.Clamp(currentSpread, 0f, Config.maxBloom);

		for (int i = 0; i < Config.bullets; i++) {
			CrosshairController.Instance.PulseCrosshair(Config.bullets, Config.burstCount);
			Vector3 newDir = ApplySpread(directionFromGun, currentSpread);

			GameObject projectile = Instantiate(Config.projectilePrefab, CalculateFirePoint(), Quaternion.LookRotation(newDir));
			projectile.transform.localScale = Vector3.one * Config.projectilePrefabScale;

			// Ignore collision to player
			Physics.IgnoreCollision(projectile.GetComponent<Collider>(), collider);

			var mover = projectile.GetComponent<ProjectileMover>();
			mover.lifetime = Config.lifetime;
			mover.speed = Config.speed;
			mover.onEnemyHit = ApplyDamage;
			mover.hitLayers = hitLayers;
		}

		Vector3 kickback = -Config.kickback * directionFromGun;
		rb.AddForce(kickback, ForceMode.Impulse);
	}

	private void ApplyDamage(EnemyHealthController enemy, Vector3 hitPoint) {
		enemy.TakeDamage(Config.damage, hitPoint);
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
		if (reloading || switchingGuns || ammo == Config.maxAmmo) return;
		reloadCoroutine = StartCoroutine(ReloadCoroutine());
	}

	IEnumerator ReloadCoroutine() {
		reloadStart = Time.time;

		reloading = true;
		yield return new WaitForSeconds(Config.reloadTime);
		reloading = false;

		ammo = Config.maxAmmo;
	}
}
