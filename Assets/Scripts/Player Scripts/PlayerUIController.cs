using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerUIController : MonoBehaviour {
	private PlayerController pc;
	private PlayerHealthController healthController;

	[Header("Score UI")]
	public TextMeshProUGUI scoreText;

	[Header("Dash UI")]
	public TextMeshProUGUI dashIconText;
	public Image dashIconOverlay;
	private float dashCDImageAlphaTarget = 1f;

	[Header("Stamina UI")]
	private float staminaBarWidth = 0f;
	public Image staminaBarOverlay;

	[Header("Health UI")]
	private float healthBarWidth = 0f;
	private Color barHealthyColor;
	public Image healthBarOverlay;
	public Color barDamagedColor;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		barDamagedColor = Color.red;
	}

	void Update() {
		UpdateDashUI();
		UpdateStaminaUI();
		UpdateHealthUI();
		UpdateScoreText();
	}

	void Start() {
		pc = GetComponent<PlayerController>();
		healthController = GetComponent<PlayerHealthController>();
		barHealthyColor = healthBarOverlay.color;
		staminaBarWidth = staminaBarOverlay.rectTransform.rect.width;
		healthBarWidth = healthBarOverlay.rectTransform.rect.width;
	}

	public void SetAlphaTarget(float alpha) {
		dashCDImageAlphaTarget = alpha;
	}

	void UpdateDashUI() {
		float ratio = Mathf.Clamp01(1 - (Time.time - pc.dashStart - pc.dashDuration) / pc.dashCooldown);
		dashIconOverlay.fillAmount = ratio;

		float secondsLeft = Mathf.Clamp(pc.dashCooldown - (Time.time - pc.dashStart - pc.dashDuration), 0f, pc.dashCooldown);
		if (secondsLeft == 0f) {
			dashCDImageAlphaTarget = 0f;
		} else {
			dashIconText.text = $"{secondsLeft:0.0}s";
		}

		dashIconText.alpha = Utils.EaseTowards(dashIconText.alpha, dashCDImageAlphaTarget, 5f, 2f);
	}

	void UpdateStaminaUI() {
		float current = staminaBarOverlay.rectTransform.rect.width / staminaBarWidth;
		float target = pc.Stamina / pc.maxStamina;

		float ratio = Utils.EaseTowards(current, target, 5f, 2f);
		staminaBarOverlay.rectTransform.sizeDelta = new Vector2(staminaBarWidth * ratio, staminaBarOverlay.rectTransform.rect.height);
	}

	void UpdateHealthUI() {
		float current = healthBarOverlay.rectTransform.rect.width / healthBarWidth;
		float target = healthController.health / healthController.maxHealth;

		float ratio = Utils.EaseTowards(current, target, 5f, 2f);
		healthBarOverlay.rectTransform.sizeDelta = new Vector2(healthBarWidth * ratio, healthBarOverlay.rectTransform.rect.height);
		healthBarOverlay.color = Color.Lerp(barDamagedColor, barHealthyColor, ratio); ;
	}

	public void UpdateScoreText() {
		scoreText.text = $"Score: {pc.score}";
	}
}
