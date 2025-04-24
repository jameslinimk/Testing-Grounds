using System;
using UnityEngine;

public interface IGunMod {
	public Rarity rarity { get; }
	public string name { get; }
	public bool unique { get; }
	public string description { get; }

	IGunMod Clone();

	void UpdateSeed(float seed);
	void Apply(GunConfig config);
	void UnApply(GunConfig config);
	bool CanApply(GunConfig config) {
		return true;
	}
}

public class FireRateMod : IGunMod {
	public IGunMod Clone() {
		return (IGunMod)MemberwiseClone();
	}

	public Rarity rarity => Rarity.Common;
	public string name => "Fire Rate Mod";
	public bool unique => false;
	public string description => $"Increases fire rate by {Math.Round(fireRate, 2)} seconds.";

	private float fireRate;
	public void UpdateSeed(float seed) {
		fireRate = Mathf.Lerp(0.1f, 0.5f, seed);
	}

	public void Apply(GunConfig config) {
		config.fireCooldown -= fireRate;
	}

	public void UnApply(GunConfig config) {
		config.fireCooldown += fireRate;
	}
}

public class DoubleBulletMod : IGunMod {
	public IGunMod Clone() {
		return (IGunMod)MemberwiseClone();
	}

	public Rarity rarity => Rarity.Uncommon;
	public string name => "Double Bullet Mod";
	public bool unique => false;
	public string description => $"Doubles the amount of bullets, but reduces fire rate by {Math.Round(fireRate, 2)} seconds.";

	private float fireRate;
	public void UpdateSeed(float seed) {
		fireRate = Mathf.Lerp(0.1f, 0.5f, seed);
	}

	public void Apply(GunConfig config) {
		config.bullets *= 2;
		config.fireCooldown += fireRate;
	}

	public void UnApply(GunConfig config) {
		config.bullets /= 2;
		config.fireCooldown -= fireRate;
	}
}
