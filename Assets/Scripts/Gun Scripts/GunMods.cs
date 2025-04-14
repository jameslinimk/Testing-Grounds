using UnityEngine;

public interface IGunMod {
	public Rarity rarity { get; }
	public string name { get; }
	public bool unique { get; }
	string Description(float seed);

	void Apply(GunConfig config, float seed);
	void UnApply(GunConfig config, float seed);
	bool CanApply(GunConfig config);
}

public class FireRateMod : IGunMod {
	public Rarity rarity => Rarity.Common;
	public string name => "Fire Rate Mod";
	public bool unique => false;

	public string Description(float seed) {
		return $"Fire rate increased by {Mathf.Lerp(0.1f, 0.5f, seed)}";
	}

	public void Apply(GunConfig config, float seed) {
		config.fireCooldown -= Mathf.Lerp(0.1f, 0.5f, seed);
	}

	public void UnApply(GunConfig config, float seed) {
		config.fireCooldown += Mathf.Lerp(0.1f, 0.5f, seed);
	}

	public bool CanApply(GunConfig config) {
		return config.bullets < 100;
	}
}
