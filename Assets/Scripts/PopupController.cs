using TMPro;
using UnityEngine;

public class PopupController : MonoBehaviour {
	private readonly float displayDistance = 7f;
	private readonly float fadeSpeed = 4f;

	public bool canShow = false;
	private bool resetAlpha = false;
	private float targetAlpha = 0f;

	public Transform collectableTransform;
	public PlayerGunManager gunManager;
	private SpellController spellController;
	public new Transform camera;
	public GunSlot gunSlot;

	private GameObject panel;
	private CanvasGroup panelGroup;
	private TextMeshProUGUI nameText;
	private TextMeshProUGUI modsText;

	void Start() {
		GameObject canvas = transform.GetChild(0).gameObject;
		canvas.GetComponent<Canvas>().worldCamera = camera.GetComponent<Camera>();

		panel = canvas.transform.GetChild(0).gameObject;
		panel.SetActive(true);

		panelGroup = panel.GetComponent<CanvasGroup>();

		/* ------------------------------ Setting text ------------------------------ */
		nameText = panel.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
		modsText = panel.transform.Find("ModsText").GetComponent<TextMeshProUGUI>();

		panelGroup.alpha = 0f;

		nameText.text = gunSlot.config.spellName;
		modsText.text = "";
		foreach (IGunMod mod in gunSlot.config.mods) {
			modsText.text += $"{mod.name}\n";
			modsText.text += $"<size=0.15>- {mod.description}\n\n</size>";
		}
	}

	public void Initialize(GunSlot gunSlot, Transform collectableTransform, PlayerGunManager player, Transform camera) {
		this.gunSlot = gunSlot;
		this.collectableTransform = collectableTransform;
		gunManager = player;
		spellController = player.GetComponent<SpellController>();
		this.camera = camera;
	}

	void Update() {
		if (!resetAlpha) {
			panelGroup.alpha = 1f;
			resetAlpha = true;
		}
		transform.position = collectableTransform.position;
		panelGroup.alpha = Utils.EaseTowards(panelGroup.alpha, targetAlpha, fadeSpeed, 1f);
		if (panelGroup.alpha == 0f) panel.SetActive(false);

		if (!canShow) {
			panel.SetActive(false);
			return;
		}

		bool inDistance = Vector3.Distance(gunManager.transform.position, transform.position) <= displayDistance;
		if (!inDistance) {
			targetAlpha = 0f;
			return;
		}

		Vector3 cameraLook = spellController.CalculateTargetPoint(false);
		if (!Physics.Raycast(spellController.CalculateFirePoint(), cameraLook, out RaycastHit hit, displayDistance) || hit.collider.gameObject != gameObject) {
			targetAlpha = 0f;
			return;
		}

		targetAlpha = 1f;
		panel.SetActive(true);
		panel.transform.LookAt(camera);
		panel.transform.Rotate(0f, 180f, 0f);
	}
}
