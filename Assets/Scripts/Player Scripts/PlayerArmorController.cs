using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ArmorSlot {
	Boots,
	Chest,
	Gloves,
	Pants,
	Helmet,
}

struct ArmorModels {
	public const string Starter = "StarterClothes";
	public const string Plate = "PlateSet1";
	public const string None = "None";

	/// <summary>
	/// What slots do each armor set have
	/// </summary>
	public static Dictionary<string, ArmorSlot[]> ArmorModelSlots = new() {
		{ Starter, new[] { ArmorSlot.Boots, ArmorSlot.Chest, ArmorSlot.Pants }},
		{ Plate, new[] { ArmorSlot.Boots, ArmorSlot.Chest, ArmorSlot.Gloves, ArmorSlot.Pants, ArmorSlot.Helmet }},
		{ None, new[] { ArmorSlot.Boots, ArmorSlot.Chest, ArmorSlot.Gloves, ArmorSlot.Pants, ArmorSlot.Helmet }},
	};

	/// <summary>
	/// Which meshes for each armor set correspond to which armor slot
	/// </summary>
	public static Dictionary<ArmorSlot, string[]> SlotToggles = new() {
		{ ArmorSlot.Boots, new[] { "Boots" } },
		{ ArmorSlot.Chest, new[] { "Chest", "Shoulders" } },
		{ ArmorSlot.Gloves, new[] { "Gloves" } },
		{ ArmorSlot.Pants, new[] { "Pants" } },
		{ ArmorSlot.Helmet, new[] { "Helmet" } },
	};

	/// <summary>
	/// Which naked body parts need to be toggled off when wearing corresponding armor piece
	/// </summary>
	public static Dictionary<ArmorSlot, string[]> SlotNakedToggles = new() {
		{ ArmorSlot.Boots, new[] { "Feet" } },
		{ ArmorSlot.Chest, new[] { "Chest" } },
		{ ArmorSlot.Gloves, new[] { "Hands" } },
		{ ArmorSlot.Pants, new[] { "Legs" } },
		{ ArmorSlot.Helmet, new string[0] },
	};
}

[CreateAssetMenu(fileName = "NewArmor", menuName = "Inventory/Armor")]
public class ArmorConfig : ScriptableObject {
	public Rarity rarity;
	public string set;
	public ArmorSlot[] slotsOfSet;
}

public class PlayerArmorController : MonoBehaviour {
	public Dictionary<ArmorSlot, ArmorConfig> slots = new();

	private Transform playerModel;
	private Transform armorsParent;
	private Transform nakedParent;
	private GameObject underwear;

	void Start() {
		playerModel = transform.GetChild(0);
		armorsParent = playerModel.Find("Armors");
		nakedParent = playerModel.Find("Mesh").Find("Body");
		underwear = playerModel.Find("Mesh").Find("Accessories").Find("Underwear").gameObject;
	}

	public void EquipSlot(ArmorSlot slot, string armorSet) {
		var current = slots[slot];
		if (current.set == armorSet) {
			Debug.LogWarning($"Armor slot {slot} already has armor set {armorSet}");
			return;
		}

		var x = ArmorModels.ArmorModelSlots[armorSet];
		if (x == null) {
			Debug.LogWarning($"Armor set {armorSet} not found");
			return;
		}
		if (!x.Contains(slot)) {
			Debug.LogWarning($"Armor set {armorSet} does not contain slot {slot}");
			return;
		}

		var slotToggles = ArmorModels.SlotToggles[slot];
		var nakedToggles = ArmorModels.SlotNakedToggles[slot];

		var currentNaked = current.set == ArmorModels.None;
		var switchingToNaked = armorSet == ArmorModels.None;

		/**

		Naked -> armor:
		- Disable naked body parts
		- Skip second block
		- Enable armor piece

		Armor -> naked:
		- Enable naked body parts
		- Disable current armor piece
		- Skip third block

		Armor1 -> armor2:
		- Re-disable naked body parts
		- Disable current armor piece
		- Enable armor piece

		TODO: NEED TO TEST!

		*/

		// Toggle naked
		underwear.SetActive(switchingToNaked);
		foreach (var nakedToggle in nakedToggles) {
			var naked = nakedParent.Find(nakedToggle);
			if (naked == null) {
				Debug.LogWarning($"Naked body part {nakedToggle} not found");
			} else {
				naked.gameObject.SetActive(switchingToNaked);
			}
		}

		// If we are not currently naked, IE: wearing armor, toggle armor off
		if (!currentNaked) {
			var currentSetParent = armorsParent.Find(current.set);
			foreach (var toggle in slotToggles) {
				var piece = currentSetParent.Find(toggle);
				if (piece == null) {
					Debug.LogWarning($"1 Armor piece {toggle} not found");
				} else {
					piece.gameObject.SetActive(false);
				}
			}
		}

		if (!switchingToNaked) {
			var setParent = armorsParent.Find(armorSet);
			foreach (var toggle in slotToggles) {
				var piece = setParent.Find(toggle);
				if (piece == null) {
					Debug.LogWarning($"2 Armor piece {toggle} not found");
				} else {
					piece.gameObject.SetActive(true);
				}
			}
		}
	}
}
