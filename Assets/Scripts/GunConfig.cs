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
	[Tooltip("Starting spread before bloom")] public float spread;
	public float maxBloom;
	[Tooltip("How much the spread increases per shot")] public float bloomRate;
	[Tooltip("How long the gun has to be idle before the bloom starts decreasing")] public float bloomCooldownDelay;
	[Tooltip("How fast bloom decreases after cooldown")] public float bloomCooldownRate;
}
