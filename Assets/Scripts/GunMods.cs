public interface IGunMod {
	public Rarity rarity { get; }
	public string name { get; }

	void Apply(GunConfig config);
	void UnApply(GunConfig config);
}

public class TestMod : IGunMod {
	public Rarity rarity => Rarity.Common;
	public string name => "Test Mod";

	public void Apply(GunConfig config) {
		config.bullets += 10;
	}

	public void UnApply(GunConfig config) {
		config.bullets -= 10;
	}
}

