using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public enum LevelEnemyType {
	Melee,
	// Ranged, TODO
}

public class GameManager : Singleton<GameManager> {
	public bool IsPaused { get; private set; } = false;
	[HideInInspector] public bool CanUnpause = true;
	private InputAction pauseAction;

	public static event Action OnPauseChange;
	public static event Action EnemiesDie;

	public Transform player;

	private readonly string firstTaunt = "Welcome strange wizard. Prepare to die... here lies your grave...";
	private readonly List<string> taunts = new() {
		"Good job, although you're going to die anyway...",
		"Why do you keep fighting. Just give up...",
		"You're only delaying the inevitable...",
		"Every move you make just digs your grave deeper...",
		"I admire your persistence...",
		"You call that a strategy? Cute...",
		"You're not even worth the effort it takes to kill you...",
		"I've seen corpses with more fight in them...",
		"Your defeat is the only predictable thing about you...",
		"Try harder. I want to at least pretend to be challenged...",
		"The sooner you fall, the sooner I can stop pretending to care...",
		"You think you matter? That's adorable...",
		"Your struggle is entertaining, I'll give you that...",
		"You were doomed from the moment you dared to try...",
		"Just lie down and save us both some time...",
		"Keep going. I haven't laughed this hard in ages...",
		"You're a walking tragedy with delusions of grandeur...",
		"I almost feel bad for you. Almost...",
		"Every second you survive is a miracle of incompetence...",
		"Your best wasn't even close to good enough...",
		"I expected a challenge, not a comedy show...",
		"Even your failures are disappointing...",
		"You're not fighting for victory—just stalling your end...",
		"If ignorance were strength, you'd be unstoppable...",
		"Your downfall is the only thing you're good at...",
		"All this effort, just to be forgotten...",
	};

	private int enemiesAlive = 0;

	private int currentLevel = 0;
	private int currentStage = -1;
	private int currentEnemiesPerStage = 1;
	private int currentStagesPerLevel = 1;

	private int baseEnemiesPerStage = 1;
	private int baseStagesPerLevel = 1;
	private readonly int maxBaseStagesPerLevel = 7;

	private float stageDelay = 4f;

	private readonly float enemySpeedVariance = 0.5f;
	private readonly float enemyDamageVariance = 1.5f;

	private float baseEnemySpeed = 1f;
	private float baseEnemyDamage = 1f;

	private int InclusiveRand(int min, int max) {
		return UnityEngine.Random.Range(min, max + 1);
	}

	private void SpawnEnemy(LevelEnemyType type) {
		var prefab = enemyPrefabs[type];
		if (prefab == null) {
			Debug.LogError($"No prefab found for enemy type: {type}");
			return;
		}

		// Remove closest spawn
		BoxCollider closestCollider = enemySpawnPoints.OrderBy(collider => Vector3.Distance(collider.bounds.center, player.position)).First();
		List<BoxCollider> remainingColliders = enemySpawnPoints.Where(c => c != closestCollider).ToList();

		// Get a random point in selected random collider
		BoxCollider collider = remainingColliders.Rand();
		Vector3 spawn = GetRandomPointInBox(collider);

		Instantiate(prefab, spawn, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), enemyParent).GetComponent<EnemyController>().Initialize(
			baseEnemySpeed + UnityEngine.Random.Range(-enemySpeedVariance, enemySpeedVariance),
			baseEnemyDamage + UnityEngine.Random.Range(-enemyDamageVariance, enemyDamageVariance)
		);
	}

	public void OnEnemyDead() {
		if (IsPaused) return;

		enemiesAlive--;
		if (enemiesAlive <= 0) {
			StartCoroutine(NextStageCoroutine());
		}
	}

	IEnumerator NextStageCoroutine(float? customDelay = null) {
		if (currentLevel == 0) TauntTextController.Instance.SetText(firstTaunt);

		currentStage++;
		baseEnemiesPerStage += InclusiveRand(-1, 4);
		currentEnemiesPerStage = Mathf.Max(1, baseEnemiesPerStage + InclusiveRand(-2, 2));

		if (currentStage >= currentStagesPerLevel) {
			// Next level
			currentLevel++;
			currentStage = 0;
			baseStagesPerLevel += InclusiveRand(-1, 3);
			baseStagesPerLevel = Mathf.Clamp(baseStagesPerLevel, 1, maxBaseStagesPerLevel);
			currentStagesPerLevel = Mathf.Max(1, baseStagesPerLevel + InclusiveRand(-2, 2));

			baseEnemySpeed += UnityEngine.Random.Range(0.1f, 0.5f);
			baseEnemyDamage += UnityEngine.Random.Range(0.1f, 0.5f);

			TauntTextController.Instance.SetText(taunts.Rand());
		}

		yield return new WaitForSeconds((customDelay != null) ? (float)customDelay : stageDelay);

		for (int i = 0; i < currentEnemiesPerStage; i++) {
			enemiesAlive++;
			SpawnEnemy(LevelEnemyType.Melee); // Add more l8r
		}
	}

	public LevelEnemyType[] enemyPrefabKeys;
	public GameObject[] enemyPrefabValues;
	private Dictionary<LevelEnemyType, GameObject> enemyPrefabs;

	public Transform enemySpawnPointsParent;
	private List<BoxCollider> enemySpawnPoints = new();

	public static Vector3 GetRandomPointInBox(BoxCollider boxCollider) {
		Vector3 localCenter = boxCollider.center;
		Vector3 localSize = boxCollider.size;

		Vector3 localRandomPoint = new(
			UnityEngine.Random.Range(-localSize.x / 2f, localSize.x / 2f),
			UnityEngine.Random.Range(-localSize.y / 2f, localSize.y / 2f),
			UnityEngine.Random.Range(-localSize.z / 2f, localSize.z / 2f)
		);

		localRandomPoint += localCenter;
		return boxCollider.transform.TransformPoint(localRandomPoint);
	}

	public Transform enemyParent;

	public TextMeshProUGUI progressText;
	private float levelProgressBarWidth = 0f;
	public Image levelProgressBarOverlay;

	public TextMeshProUGUI endText;

	void Start() {
		// pauseAction = InputSystem.actions.FindAction("Pause");
		// pauseAction.performed += _ => { if (CanUnpause) SetPause(!IsPaused); };

		enemyPrefabs = new Dictionary<LevelEnemyType, GameObject>();
		for (int i = 0; i < enemyPrefabKeys.Length; i++) {
			enemyPrefabs[enemyPrefabKeys[i]] = enemyPrefabValues[i];
		}
		if (enemyPrefabKeys.Length != enemyPrefabValues.Length) {
			Debug.LogError("Enemy prefab keys and values arrays must have the same length.");
		}

		levelProgressBarWidth = levelProgressBarOverlay.rectTransform.rect.width;

		enemySpawnPoints.AddRange(enemySpawnPointsParent.GetComponentsInChildren<BoxCollider>());

		StartCoroutine(NextStageCoroutine(5f));
	}

	void Update() {
		float current = levelProgressBarOverlay.rectTransform.rect.width / levelProgressBarWidth;
		float target = Mathf.Clamp01(1f - (enemiesAlive / (float)currentEnemiesPerStage));

		float ratio = Utils.EaseTowards(current, target, 5f, 2f);
		levelProgressBarOverlay.rectTransform.sizeDelta = new Vector2(levelProgressBarWidth * ratio, levelProgressBarOverlay.rectTransform.rect.height);

		progressText.text = $"Level: {currentLevel + 1}, Wave: {currentStage + 1}";

		// if (Keyboard.current.tKey.wasReleasedThisFrame) {
		// 	StartCoroutine(NextStageCoroutine(0f));
		// }
	}

	public void EndGame() {
		endText.text = $"That's it? All that struggle, just to die like everyone else. Pathetic.\nLevel: {currentLevel + 1}";
		endText.gameObject.SetActive(true);

		SetPause(true, false);
		EnemiesDie?.Invoke();
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
