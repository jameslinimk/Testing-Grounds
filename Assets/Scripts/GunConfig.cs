using UnityEngine;

public enum FireType {
	Single,
	Burst,
	Auto
}

public enum Rarity {
	Common,
	Uncommon,
	Rare,
	Epic,
	Legendary
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class GunConfig : ScriptableObject {
	[Header("Basic Info")]
	public string weaponName;
	public string description;
	public Rarity rarity;
	public Sprite icon;
	// Firepoint is a child of the weapon prefab
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
	public float headshotMultiplier = 2f;
	public int bullets = 1;
	public int maxAmmo;
	public float reloadTime;

	[Header("Accuracy")]
	public float spread;
	public float maxBloom;
	public float bloomRate;
	public float bloomCooldownDelay;
	public float bloomCooldownRate;

	public float dps => damage * bullets / fireCooldown;
}
