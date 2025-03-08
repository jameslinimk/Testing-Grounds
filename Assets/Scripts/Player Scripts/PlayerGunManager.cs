using UnityEngine;
using UnityEngine.InputSystem;

public class GunSlot {
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
	private readonly GunSlot[] gunSlots = new GunSlot[3];
	private int currentGunIndex = 0;

	public GunSlot CurrentGun => gunSlots[currentGunIndex];

	void Start() {
		for (int i = 0; i < gunSlots.Length; i++) gunSlots[i] = new GunSlot();
		gunSlots[0].ReConfig(gunController.defaultGunConfig.Clone());

		// For testing add other guns with diff names (rename default gun config)
		gunSlots[1].ReConfig(gunController.defaultGunConfig.Clone());
		gunSlots[2].ReConfig(gunController.defaultGunConfig.Clone());
		gunSlots[1].config.weaponName = "Gun2";
		gunSlots[2].config.weaponName = "Gun3";

		InputSystem.actions.FindAction("Gun1").performed += _ => SwitchGun(0);
		InputSystem.actions.FindAction("Gun2").performed += _ => SwitchGun(1);
		InputSystem.actions.FindAction("Gun3").performed += _ => SwitchGun(2);
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

	public void DropGun(int gunIndex) {
		GunSlot gunSlot = gunSlots[gunIndex];
		if (gunSlot.Empty) return;

		for (int i = Utils.WrapAround(gunIndex - 1, gunSlots.Length); i != gunIndex; i = Utils.WrapAround(i - 1, gunSlots.Length)) {
			if (i == gunIndex) break;
			if (!gunSlots[i].Empty) {
				SwitchGun(i);
				gunSlot.Clear();
				return;
			}
		}

		ErrorTextController.Instance.SetText("No other gun to switch to");
	}

	private void SwitchGun(int gunIndex) {
		GunSlot gunSlot = gunSlots[gunIndex];
		if (gunSlot.Empty || gunIndex == currentGunIndex) return;

		gunController.SwitchGun(gunSlot);

		gunSlots[currentGunIndex].putAwayTime = Time.time;
		currentGunIndex = gunIndex;
	}
}
