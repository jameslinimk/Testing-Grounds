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

public enum ArmorSetModels {
	None,
	StarterClothes,
	PlateSet1,
}

public class ArmorSetSlotInfo {
	public List<string> modelNames;
	public bool enableUnderwear = false;
}

public class ArmorSet {
	public ArmorSetModels name;
	public Dictionary<ArmorSlot, ArmorSetSlotInfo> slotInfo = new();

	public bool Equals(ArmorSet other) {
		if (other == null) return false;
		return name == other.name;
	}

	public static Dictionary<ArmorSetModels, ArmorSet> ArmorSets = new() {
		{ ArmorSetModels.None, new ArmorSet {
			name = ArmorSetModels.None,
			slotInfo = new() {
				{ ArmorSlot.Boots, new ArmorSetSlotInfo { modelNames = new() { "Mesh/Body/Feet" } } },
				{ ArmorSlot.Chest, new ArmorSetSlotInfo { modelNames = new() { "Mesh/Body/Chest" } } },
				{ ArmorSlot.Gloves, new ArmorSetSlotInfo { modelNames = new() { "Mesh/Body/Hands" } } },
				{ ArmorSlot.Pants, new ArmorSetSlotInfo { modelNames = new() { "Mesh/Body/Legs" }, enableUnderwear = true } },
				{ ArmorSlot.Helmet, new ArmorSetSlotInfo { modelNames = new() {} } },
			}
		} },
		{ ArmorSetModels.StarterClothes, new ArmorSet {
			name = ArmorSetModels.StarterClothes,
			slotInfo = new() {
				{ ArmorSlot.Boots, new ArmorSetSlotInfo { modelNames = new() { "Armors/StarterClothes/Starter_Boots" } } },
				{ ArmorSlot.Chest, new ArmorSetSlotInfo { modelNames = new() { "Armors/StarterClothes/Starter_Chest" } } },
				{ ArmorSlot.Pants, new ArmorSetSlotInfo { modelNames = new() { "Armors/StarterClothes/Starter_Pants" } } },
			}
		} },
		{ArmorSetModels.PlateSet1, new ArmorSet {
			name = ArmorSetModels.PlateSet1,
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
	public ArmorSetModels set;
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
			RawEquipSlot(ArmorSlot.Chest, ArmorSet.ArmorSets[ArmorSetModels.PlateSet1]);
		}

		if (Keyboard.current.yKey.wasReleasedThisFrame) {
			RawEquipSlot(ArmorSlot.Chest, ArmorSet.ArmorSets[ArmorSetModels.None]);
		}
	}

	/// <summary>
	/// Doesn't change `this.slots`
	/// </summary>
	public void RawEquipSlot(ArmorSlot slot, ArmorSet targetSet) {
		var currentSet = ArmorSet.ArmorSets[slots.ContainsKey(slot) ? slots[slot].set : ArmorSetModels.None];
		if (currentSet.Equals(targetSet)) {
			Debug.LogWarning($"Armor slot {slot} already has armor set {targetSet.name} equipped!");
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
