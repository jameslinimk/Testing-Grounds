public interface IGunMod {
	public Rarity rarity { get; }
	public string name { get; }
	public bool unique { get; }

	void Apply(GunConfig config);
	void UnApply(GunConfig config);
	bool CanApply(GunConfig config);
}

public class TestMod : IGunMod {
	public Rarity rarity => Rarity.Common;
	public string name => "Test Mod";
	public bool unique => false;

	public void Apply(GunConfig config) {
		config.bullets += 10;
	}

	public void UnApply(GunConfig config) {
		config.bullets -= 10;
	}

	public bool CanApply(GunConfig config) {
		return config.bullets < 100;
	}
}
