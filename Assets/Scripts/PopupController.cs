using TMPro;
using UnityEngine;

public class PopupController : MonoBehaviour {
	private readonly float displayDistance = 7f;
	private readonly float fadeDuration = 0.5f;

	public bool canShow = false;

	public Transform collectableTransform;
	public PlayerGunManager player;
	public new Transform camera;
	public GunSlot gunSlot;

	private GameObject canvas;

	void Start() {
		canvas = transform.GetChild(0).gameObject;
		canvas.SetActive(false);
		canvas.GetComponent<Canvas>().worldCamera = camera.GetComponent<Camera>();

		/* ------------------------------ Setting text ------------------------------ */
		TextMeshProUGUI nameText = canvas.transform.GetChild(0).Find("NameText").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI modsText = canvas.transform.GetChild(0).Find("ModsText").GetComponent<TextMeshProUGUI>();

		nameText.text = "";
		modsText.text = "";

		nameText.text = gunSlot.config.name;
		foreach (IGunMod mod in gunSlot.config.mods) {
			modsText.text += $"{mod.name}\n";
			modsText.text += $"<size=0.15>- {mod.description}\n\n</size>";
		}

		// FIXME: when player first looks at it, the styling is not there
	}

	public void Initialize(GunSlot gunSlot, Transform collectableTransform, PlayerGunManager player, Transform camera) {
		this.gunSlot = gunSlot;
		this.collectableTransform = collectableTransform;
		this.player = player;
		this.camera = camera;
	}

	void Update() {
		transform.position = collectableTransform.position;

		if (!canShow) {
			canvas.SetActive(false);
			return;
		}

		bool inDistance = Vector3.Distance(player.transform.position, transform.position) <= displayDistance;
		if (!inDistance) {
			canvas.SetActive(false);
			return;
		}

		Vector3 cameraLook = player.gunController.CalculateLookDirection(false);
		if (!Physics.Raycast(player.gunController.FirePointWithFallback, cameraLook, out RaycastHit hit, displayDistance) || hit.collider.gameObject != gameObject) {
			canvas.SetActive(false);
			return;
		}

		canvas.SetActive(inDistance);
		if (inDistance) {
			canvas.transform.LookAt(camera);
			canvas.transform.Rotate(0f, 180f, 0f);
		}
	}
}
