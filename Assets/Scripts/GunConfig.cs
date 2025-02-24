using UnityEngine;

public enum FireType {
	Single,
	Burst,
	Auto
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class GunConfig : ScriptableObject {
	[Header("Basic Info")]
	public string weaponName;
	public string description;
	public Sprite icon;
	public GameObject weaponPrefab;

	[Header("Other Info")]
	public float drawSpeed;

	[Header("Fire Behavior")]
	public FireType fireType;
	public int burstCount = 0;
	public float burstDelay = 0f;
	public float fireCooldown;
	public float kickback;

	[Header("Damage & Ammo")]
	public float damage;
	public float headShotMultiplier = 2f;
	public int bullets = 1;
	public int maxAmmo;
	public float reloadTime;

	[Header("Accuracy")]
	public float spread;
	public float maxBloom;
	public float bloomRate;
	public float bloomCooldownDelay;
	public float bloomCooldownRate;
}
