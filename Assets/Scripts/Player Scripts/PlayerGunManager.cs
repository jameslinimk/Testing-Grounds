using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunManager : MonoBehaviour {
	public GunController gunController;
	private GunConfig[] guns = new GunConfig[3];
	private int currentGunIndex = 0;

	void Start() {
		InputSystem.actions.FindAction("Gun1").performed += _ => SwitchGun(0);
		InputSystem.actions.FindAction("Gun2").performed += _ => SwitchGun(1);
		InputSystem.actions.FindAction("Gun3").performed += _ => SwitchGun(2);
		InputSystem.actions.FindAction("NextGun").performed += _ => SwitchGun((currentGunIndex + 1) % guns.Length);
		InputSystem.actions.FindAction("PreviousGun").performed += _ => SwitchGun((currentGunIndex - 1 + guns.Length) % guns.Length);
	}

	private void SwitchGun(int gunIndex) {
		GunConfig gun = guns[gunIndex];
		if (gun == null) return;
		gunController.SwitchGun(gun);
		currentGunIndex = gunIndex;
	}
}
