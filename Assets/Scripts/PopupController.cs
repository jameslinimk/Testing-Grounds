using UnityEngine;

[CreateAssetMenu(menuName = "Settings/GunPopupSettings")]
public class GunPopupSettings : ScriptableObject {
	public GameObject popupPrefab;
	public float displayDistance = 7f;
	public float fadeDuration = 0.5f;
}

public class PopupController : MonoBehaviour {
	public GunPopupSettings settings;
	public PlayerGunManager player;
	public new Transform camera;
	public GunSlot gunSlot;

	private GameObject canvas;

	void Start() {
		if (settings == null) settings = Resources.Load<GunPopupSettings>("Settings/GunPopupSettings");
		canvas = Instantiate(settings.popupPrefab, transform.position, Quaternion.identity, transform);
		canvas.SetActive(false);
	}

	void Initialize(GunSlot gunSlot, PlayerGunManager player, Transform camera) {
		this.gunSlot = gunSlot;
		this.player = player;
		this.camera = camera;
	}

	void Update() {
		bool inDistance = Vector3.Distance(player.transform.position, transform.position) <= settings.displayDistance;
		if (!inDistance) {
			canvas.SetActive(false);
			return;
		}

		Vector3 cameraLook = player.gunController.CalculateLookDirection(false);
		if (!Physics.Raycast(player.gunController.FirePointWithFallback, cameraLook, out RaycastHit hit, settings.displayDistance) || hit.collider.gameObject != gameObject) {
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
