using System;
using System.Collections.Generic;
using UnityEngine;

public enum FireType {
	Single,
	BurstSingle,
	BurstAuto,
	Auto
}

public enum Rarity {
	Common,
	Uncommon,
	Rare,
	Epic,
	Legendary
}

[Serializable]
public struct ProjectileInfo {
	public GameObject projectilePrefab;
	public float speed;
	public float lifetime;
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class GunConfig : ScriptableObject {
	[Header("Basic Info")]
	public string weaponName;
	public string description;
	public Rarity rarity;

	[Header("Visuals")]
	public Sprite icon;
	public GameObject weaponPrefab;

	[Header("Other Info")]
	public float drawSpeed;
	public bool isProjectile;
	public ProjectileInfo projectileInfo;

	[Header("Fire Behavior")]
	public FireType fireType;
	public float range;
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
	public float bloom;
	public float bloomCooldownDelay;
	public float bloomCooldownRate;

	public float dps => damage * bullets / fireCooldown;

	/* ---------------------------------- Mods ---------------------------------- */
	[HideInInspector] public readonly List<(IGunMod, float)> mods = new();
	private readonly HashSet<string> modSet = new();

	public void AddMod(IGunMod mod) {
		if (!mod.CanApply(this)) return;
		if (mod.unique) {
			if (modSet.Contains(mod.name)) return;
			modSet.Add(mod.name);
		}

		float seed = UnityEngine.Random.Range(0f, 1f);
		mods.Add((mod, seed));
		mod.Apply(this, seed);
	}

	public void AddMods(IEnumerable<IGunMod> mods) {
		foreach (var mod in mods) {
			AddMod(mod);
		}
	}

	public void RemoveMod(int index) {
		var (mod, seed) = mods[index];
		mod.UnApply(this, seed);
		if (mod.unique) modSet.Remove(mod.name);
		mods.RemoveAt(index);
	}

	public GunConfig Clone() {
		return Instantiate(this);
	}
}
