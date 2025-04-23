using System.ComponentModel;
using UnityEngine;

public class PopupController : MonoBehaviour {
	private GameObject child;

	public PlayerGunManager player;
	public new Transform camera;
	[DefaultValue(7f)] public float displayDistance;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	void Start() {
		child = transform.GetChild(0).gameObject;
		child.SetActive(false);
	}

	void Update() {
		bool inDistance = Vector3.Distance(player.transform.position, transform.position) <= displayDistance;
		if (!inDistance) {
			child.SetActive(false);
			return;
		}

		Vector3 cameraLook = player.gunController.CalculateLookDirection(false);
		if (!Physics.Raycast(player.gunController.FirePointWithFallback, cameraLook, out RaycastHit hit, displayDistance) || hit.collider.gameObject != gameObject) {
			child.SetActive(false);
			return;
		}

		child.SetActive(inDistance);
		if (inDistance) {
			child.transform.LookAt(camera);
			child.transform.Rotate(0f, 180f, 0f);
		}
	}
}
