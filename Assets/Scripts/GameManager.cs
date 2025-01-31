using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager Instance { get; private set; }
	public bool IsPaused { get; private set; } = false;
	public PlayerController Player { get; private set; }

	void Awake() {
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

	public void SetPlayer(PlayerController player) {
		Player = player;
	}
}
