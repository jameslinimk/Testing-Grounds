using UnityEngine;

public class AnimationEventHandler : MonoBehaviour {
	private SpellController spellController;

	void Start() {
		spellController = transform.parent.GetComponent<SpellController>();
	}

	public void FireProjectile() {
		spellController.FireProjectile();
	}
}
