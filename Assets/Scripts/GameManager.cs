using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour {
	public bool IsPaused { get; private set; } = false;
	[HideInInspector] public bool CanUnpause = true;
	private InputAction pauseAction;

	private static GameManager _instance;
	public static GameManager Instance {
		get {
			if (_instance == null) {
				_instance = FindFirstObjectByType<GameManager>();
				if (_instance == null) {
					GameObject singletonObject = new GameObject("GameManager");
					_instance = singletonObject.AddComponent<GameManager>();
				}
			}
			return _instance;
		}
	}

	void Awake() {
		if (_instance != null && _instance != this) {
			Destroy(gameObject);
			return;
		}

		_instance = this;
		DontDestroyOnLoad(gameObject);
	}

	void Start() {
		pauseAction = InputSystem.actions.FindAction("Pause");
		pauseAction.performed += _ => { if (CanUnpause) SetPause(!IsPaused); };
	}

	public void SetPause(bool pause, bool canUnpause = true) {
		CanUnpause = canUnpause;
		IsPaused = pause;
		Time.timeScale = pause ? 0 : 1;

		if (pause) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		} else {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	[ContextMenu("Check All Script's Default Values")]
	void CheckScriptDefaultValues() {
		Utils.CheckScriptDefaultValues(FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None));
		Debug.Log("All scripts validated");
	}

	[ContextMenu("Set All Script's Default Values")]
	void SetScriptDefaultValues() {
		Utils.SetScriptDefaultValues(FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None));
		Debug.Log("All scripts reset to default values");
	}
}
