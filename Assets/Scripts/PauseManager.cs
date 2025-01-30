using UnityEngine;

public class PauseManager : MonoBehaviour {
	public static PauseManager Instance { get; private set; }
	public bool IsPaused { get; private set; } = false;

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void SetPause(bool pause) {
		IsPaused = pause;
		Time.timeScale = pause ? 0 : 1;
	}
}
