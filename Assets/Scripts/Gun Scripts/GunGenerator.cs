using System.Linq;
using UnityEditor;
using UnityEngine;

public class GunsManager : Singleton<GunsManager> {
	[SerializeField] private GunConfig[] gunConfigs;
	private readonly IGunMod[] gunMods = new IGunMod[] {
		new TestMod()
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
}
