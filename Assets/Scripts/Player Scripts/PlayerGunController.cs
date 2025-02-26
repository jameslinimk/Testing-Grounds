using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunController : MonoBehaviour {
	private GunController[] guns = new GunController[3];
	private int currentGunIndex = 0;

	void Start() {
		InputSystem.actions.FindAction("Gun1").performed += _ => SwitchGun(0);
		InputSystem.actions.FindAction("Gun2").performed += _ => SwitchGun(1);
		InputSystem.actions.FindAction("Gun3").performed += _ => SwitchGun(2);
		InputSystem.actions.FindAction("NextGun").performed += _ => {
			currentGunIndex = (currentGunIndex + 1) % guns.Length;
			SwitchGun(currentGunIndex);
		};
		InputSystem.actions.FindAction("PreviousGun").performed += _ => {
			currentGunIndex = (currentGunIndex - 1 + guns.Length) % guns.Length;
			SwitchGun(currentGunIndex);
		};
	}

	private void SwitchGun(int gunIndex) {
		Debug.Log($"Switching to gun {gunIndex}");
	}
}
