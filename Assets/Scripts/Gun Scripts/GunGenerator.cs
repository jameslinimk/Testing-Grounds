using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GunsManager : Singleton<GunsManager> {
	[SerializeField] private GunConfig[] gunConfigs;
	private readonly IGunMod[] gunMods = new IGunMod[] {
		new FireRateMod()
	};

	private Dictionary<Rarity, List<GunConfig>> gunConfigsByRarity;
	private Dictionary<Rarity, List<IGunMod>> modsByRarity;

	protected override void Awake() {
		base.Awake();
		gunConfigsByRarity = gunConfigs
			.GroupBy(g => g.rarity)
			.ToDictionary(g => g.Key, g => g.ToList());
		modsByRarity = gunMods
			.GroupBy(m => m.rarity)
			.ToDictionary(m => m.Key, m => m.ToList());
	}

	public static Dictionary<Rarity, float> ModRarityChances = new() {
		{ Rarity.Common, 0.5f },
		{ Rarity.Uncommon, 0.3f },
		{ Rarity.Rare, 0.15f },
		{ Rarity.Legendary, 0.05f }
	};

	[SerializeField] private GunConfig defaultGunConfig;
	public GunConfig DefaultGunConfig() {
		return defaultGunConfig.Clone();
	}

	[ContextMenu("Find All Guns")]
	void FindAllGuns() {
		string[] guids = AssetDatabase.FindAssets($"t:{typeof(GunConfig).Name}");
		gunConfigs = guids.Select(guid => {
			string path = AssetDatabase.GUIDToAssetPath(guid);
			return AssetDatabase.LoadAssetAtPath<GunConfig>(path);
		}).ToArray();
	}

	public GunConfig TestConfig() {
		GunConfig conf = DefaultGunConfig();
		conf.name += " (test)";
		conf.AddMods(gunMods);
		return conf;
	}

	public GunConfig RandomGunConfig(Rarity rarity) {
		if (!gunConfigsByRarity.TryGetValue(rarity, out var configs)) {
			Debug.LogWarning($"No gun configs found for rarity: {rarity}");
			return null;
		}
		GunConfig gun = configs.Rand().Clone();
		IGunMod mod = modsByRarity[ModRarityChances.Rand()].Rand();
		gun.AddMod(mod);
		return gun;
	}
}
