using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class GunSlot {
	public GunSlot Clone() {
		return (GunSlot)MemberwiseClone();
	}

	public GunConfig config;
	public int currentAmmo;

	public bool Empty => config == null;
	public float putAwayTime = Mathf.Infinity;
	public bool CanPocketReload => Time.time - putAwayTime >= config.reloadTime;

	public GunSlot() { }

	public GunSlot(GunConfig conf) {
		config = conf;
		currentAmmo = conf.maxAmmo;
	}

	public void ReConfig(GunConfig conf) {
		config = conf;
		currentAmmo = conf.maxAmmo;
		putAwayTime = Mathf.Infinity;
	}

	public void ReSlot(GunSlot slot) {
		config = slot.config;
		currentAmmo = slot.currentAmmo;
		putAwayTime = slot.putAwayTime;
	}

	public void Reload() {
		currentAmmo = config.maxAmmo;
		putAwayTime = Mathf.Infinity;
	}

	public void Clear() {
		config = null;
		currentAmmo = 0;
		putAwayTime = Mathf.Infinity;
	}
}

public class PlayerGunManager : MonoBehaviour {
	public GunController gunController;
	public GunSlot[] gunSlots = new GunSlot[3];
	private int currentGunIndex = 0;

	public GunSlot CurrentGun => gunSlots[currentGunIndex];

	void Start() {
		for (int i = 0; i < gunSlots.Length; i++) gunSlots[i] = new GunSlot();
		gunSlots[0].ReConfig(GunsManager.Instance.DefaultGunConfig());
		gunController.SwitchGun(gunSlots[0].config);

		AddGun(GunsManager.Instance.TestConfig());
		AddGun(GunsManager.Instance.TestConfig());

		// Input actions
		for (int i = 0; i < gunSlots.Length; i++) {
			int iCopy = i;
			InputSystem.actions.FindAction($"Gun{i + 1}").performed += _ => SwitchGun(iCopy);
		}
		InputSystem.actions.FindAction("NextGun").performed += _ => SwitchGun(Utils.WrapAround(currentGunIndex + 1, gunSlots.Length));
		InputSystem.actions.FindAction("PreviousGun").performed += _ => SwitchGun(Utils.WrapAround(currentGunIndex - 1, gunSlots.Length));
		InputSystem.actions.FindAction("DropCurrentGun").performed += _ => DropGun(currentGunIndex);
	}

	public bool AddGun(GunConfig config) {
		foreach (var gunSlot in gunSlots) {
			if (gunSlot.Empty) {
				gunSlot.ReConfig(config);
				return true;
			}
		}
		return false;
	}

	public bool AddGun(GunSlot slot) {
		foreach (var gunSlot in gunSlots) {
			if (gunSlot.Empty) {
				gunSlot.ReSlot(slot);
				return true;
			}
		}
		ErrorTextController.Instance.SetText("No empty gun slot available");
		return false;
	}

	public void DropGun(int gunIndex) {
		GunSlot gunSlot = gunSlots[gunIndex];
		if (gunSlot.Empty) return;

		for (int i = Utils.WrapAround(gunIndex - 1, gunSlots.Length); i != gunIndex; i = Utils.WrapAround(i - 1, gunSlots.Length)) {
			if (i == gunIndex) break;
			if (!gunSlots[i].Empty) {
				GameObject gun = Instantiate(gunSlot.config.weaponPrefab, gunController.transform.position, gunController.transform.rotation);
				gun.AddComponent<GunCollectableController>().Initialize(gunSlot, gunController.CalculateLookDirection(), GetComponent<Collider>());

				SwitchGun(i);
				gunSlot.Clear();
				return;
			}
		}

		ErrorTextController.Instance.SetText("No other gun to switch to");
	}

	private void SwitchGun(int gunIndex) {
		GunSlot newGunSlot = gunSlots[gunIndex];
		if (newGunSlot.Empty || gunIndex == currentGunIndex) return;

		gunSlots[currentGunIndex].currentAmmo = gunController.ammo;
		gunSlots[currentGunIndex].putAwayTime = Time.time;

		gunController.SwitchGun(newGunSlot);

		currentGunIndex = gunIndex;
	}

	void OnTriggerEnter(Collider other) {
		if (other.TryGetComponent<GunCollectableController>(out var gunCollectable)) {
			if (AddGun(gunCollectable.gunSlot)) {
				Destroy(other.gameObject);
			}
		}
	}
}
