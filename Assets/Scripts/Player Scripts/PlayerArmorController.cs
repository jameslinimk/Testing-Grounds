using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ArmorSlot {
	Boots,
	Chest,
	Gloves,
	Pants,
	Helmet,
}

public class ArmorSetSlotInfo {
	public List<string> modelNames;
	public bool enableUnderwear = false;
}

public class ArmorSet {
	public string name;
	public Dictionary<ArmorSlot, ArmorSetSlotInfo> slotInfo = new();

	public bool Equals(ArmorSet other) {
		if (other == null) return false;
		return name == other.name;
	}

	public static Dictionary<string, ArmorSet> ArmorSets = new() {
		{ "None", new ArmorSet {
			name = "None",
			slotInfo = new() {
				{ ArmorSlot.Boots, new ArmorSetSlotInfo { modelNames = new() { "Mesh/Body/Feet" } } },
				{ ArmorSlot.Chest, new ArmorSetSlotInfo { modelNames = new() { "Mesh/Body/Chest" } } },
				{ ArmorSlot.Gloves, new ArmorSetSlotInfo { modelNames = new() { "Mesh/Body/Hands" } } },
				{ ArmorSlot.Pants, new ArmorSetSlotInfo { modelNames = new() { "Mesh/Body/Legs" }, enableUnderwear = true } },
				{ ArmorSlot.Helmet, new ArmorSetSlotInfo { modelNames = new() {} } },
			}
		} },
		{ "StarterClothes", new ArmorSet {
			name = "StarterClothes",
			slotInfo = new() {
				{ ArmorSlot.Boots, new ArmorSetSlotInfo { modelNames = new() { "Armors/StarterClothes/Starter_Boots" } } },
				{ ArmorSlot.Chest, new ArmorSetSlotInfo { modelNames = new() { "Armors/StarterClothes/Starter_Chest" } } },
				{ ArmorSlot.Pants, new ArmorSetSlotInfo { modelNames = new() { "Armors/StarterClothes/Starter_Pants" } } },
			}
		} },
		{"PlateSet1", new ArmorSet {
			name = "PlateSet1",
			slotInfo = new() {
				{ ArmorSlot.Boots, new ArmorSetSlotInfo { modelNames = new() { "Armors/PlateSet1/PlateSet1_Boots" } } },
				{ ArmorSlot.Chest, new ArmorSetSlotInfo { modelNames = new() { "Armors/PlateSet1/PlateSet1_Chest", "Armors/PlateSet1/PlateSet1_Shoulders" } } },
				{ ArmorSlot.Gloves, new ArmorSetSlotInfo { modelNames = new() { "Armors/PlateSet1/PlateSet1_Gloves" } } },
				{ ArmorSlot.Helmet, new ArmorSetSlotInfo { modelNames = new() { "Armors/PlateSet1/PlateSet1_Helmet" } } },
				{ ArmorSlot.Pants, new ArmorSetSlotInfo { modelNames = new() { "Armors/PlateSet1/PlateSet1_Pants" } } },
			}
		} }
	};
}

[CreateAssetMenu(fileName = "NewArmor", menuName = "Inventory/Armor")]
public class ArmorConfig : ScriptableObject {
	public Rarity rarity;
	public ArmorSet set;
	public ArmorSlot[] slotsOfSet;
}

public class PlayerArmorController : MonoBehaviour {
	public Dictionary<ArmorSlot, ArmorConfig> slots = new();

	private Transform playerModel;
	private GameObject underwear;

	void Start() {
		playerModel = transform.GetChild(0);
		underwear = playerModel.Find("Mesh").Find("Accessories").Find("Underwear").gameObject;
	}

	void Update() {
		if (Keyboard.current.tKey.wasReleasedThisFrame) {
			RawEquipSlot(ArmorSlot.Chest, ArmorSet.ArmorSets["PlateSet1"]);
		}
	}

	/// <summary>
	/// Doesn't change `this.slots`
	/// </summary>
	public void RawEquipSlot(ArmorSlot slot, ArmorSet targetSet) {
		var currentSet = slots.ContainsKey(slot) ? slots[slot].set : ArmorSet.ArmorSets["None"];
		if (currentSet.Equals(targetSet)) {
			Debug.LogWarning($"Armor slot {slot} already has armor set {targetSet} equipped!");
			return;
		}

		var currentSlotInfo = currentSet.slotInfo[slot];
		var targetSlotInfo = targetSet.slotInfo[slot];

		underwear.SetActive(targetSlotInfo.enableUnderwear);

		foreach (var modelName in currentSlotInfo.modelNames) {
			var model = playerModel.Find(modelName);
			if (model == null) {
				Debug.LogWarning($"Model {modelName} not found in current set {currentSet.name}");
			} else {
				model.gameObject.SetActive(false);
			}
		}

		foreach (var modelName in targetSlotInfo.modelNames) {
			var model = playerModel.Find(modelName);
			if (model == null) {
				Debug.LogWarning($"Model {modelName} not found in target set {targetSet.name}");
			} else {
				model.gameObject.SetActive(true);
			}
		}
	}
}
