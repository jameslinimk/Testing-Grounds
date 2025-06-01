using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public enum LevelEnemyType {
	Melee,
	Ranged,
}

public class LevelStage {
	public Dictionary<LevelEnemyType, int> enemyCounts;
	public float delay;

	public LevelStage(Dictionary<LevelEnemyType, int> enemyCounts, float delay) {
		this.enemyCounts = enemyCounts;
		this.delay = delay;
	}
}

public class Level {
	public List<LevelStage> stages;
}

public class GameManager : Singleton<GameManager> {
	public bool IsPaused { get; private set; } = false;
	[HideInInspector] public bool CanUnpause = true;
	private InputAction pauseAction;

	public static event Action OnPauseChange;

	public Transform player;

	public Level levels = new() {
		stages = new List<LevelStage> {
			new(new () {
				{ LevelEnemyType.Melee, 3 },
				{ LevelEnemyType.Ranged, 0 },
			}, 5f),
			new(new () {
				{ LevelEnemyType.Melee, 6 },
				{ LevelEnemyType.Ranged, 1 },
			}, 5f),
			new(new () {
				{ LevelEnemyType.Melee, 7 },
				{ LevelEnemyType.Ranged, 1 },
			}, 10f),
			new(new () {
				{ LevelEnemyType.Melee, 10 },
				{ LevelEnemyType.Ranged, 3 },
			}, 10f),
		}
	};
	public int currentLevel = 0;

	public LevelEnemyType[] enemyPrefabKeys;
	public GameObject[] enemyPrefabValues;
	private Dictionary<LevelEnemyType, GameObject> enemyPrefabs;

	public GameObject[] enemySpawnPoints;

	public TextMeshProUGUI levelText;
	public TextMeshProUGUI scoreText;
	public Image levelProgressBar;

	public TextMeshProUGUI endText;

	public int score = 0;

	void Start() {
		pauseAction = InputSystem.actions.FindAction("Pause");
		pauseAction.performed += _ => { if (CanUnpause) SetPause(!IsPaused); };

		enemyPrefabs = new Dictionary<LevelEnemyType, GameObject>();
		for (int i = 0; i < enemyPrefabKeys.Length; i++) {
			enemyPrefabs[enemyPrefabKeys[i]] = enemyPrefabValues[i];
		}
		if (enemyPrefabKeys.Length != enemyPrefabValues.Length) {
			Debug.LogError("Enemy prefab keys and values arrays must have the same length.");
		}
	}

	void SpawnEnemy(LevelEnemyType type) {
		var prefab = enemyPrefabs[type];
		if (prefab == null) {
			Debug.LogError($"No prefab found for enemy type: {type}");
			return;
		}

		var spawn = enemySpawnPoints.Rand();
		Debug.Log($"Spawning enemy of type {type} at {spawn.name}");
		// TODO
	}

	public void EndGame() {
		endText.text = $"You died with a score of {score}!";
		endText.gameObject.SetActive(true);

		SetPause(true, false);
	}

	public void SetPause(bool pause, bool canUnpause = true) {
		CanUnpause = canUnpause;
		IsPaused = pause;

		if (pause) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		} else {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		OnPauseChange?.Invoke();
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
