using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour {
	private PlayerController pc;
	private PlayerHealthController healthController;
	private PlayerGunManager gunManager;

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

	[Header("Gun UI")]
	public GunController gunController;
	public TextMeshProUGUI ammoCounterText;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		barDamagedColor = Color.red;
	}

	void Update() {
		UpdateDashUI();
		UpdateStaminaUI();
		UpdateHealthUI();
		UpdateGunUI();
	}

	void Start() {
		pc = GetComponent<PlayerController>();
		healthController = GetComponent<PlayerHealthController>();
		gunManager = GetComponent<PlayerGunManager>();

		barHealthyColor = healthBarOverlay.color;
		staminaBarWidth = staminaBarOverlay.rectTransform.rect.width;
		healthBarWidth = healthBarOverlay.rectTransform.rect.width;

		RefreshScoreText();
	}

	public void SetAlphaTarget(float alpha) {
		dashCDImageAlphaTarget = alpha;
	}

	private void UpdateDashUI() {
		float ratio = Mathf.Clamp01(1 - (Time.time - pc.DashStart - pc.dashDuration) / pc.dashCooldown);
		dashIconOverlay.fillAmount = ratio;

		float secondsLeft = Mathf.Clamp(pc.dashCooldown - (Time.time - pc.DashStart - pc.dashDuration), 0f, pc.dashCooldown);
		if (secondsLeft == 0f) {
			dashCDImageAlphaTarget = 0f;
		} else {
			dashIconText.text = $"{secondsLeft:0.0}s";
		}

		dashIconText.alpha = Utils.EaseTowards(dashIconText.alpha, dashCDImageAlphaTarget, 5f, 2f);
	}

	private void UpdateStaminaUI() {
		float current = staminaBarOverlay.rectTransform.rect.width / staminaBarWidth;
		float target = pc.Stamina / pc.maxStamina;

		float ratio = Utils.EaseTowards(current, target, 5f, 2f);
		staminaBarOverlay.rectTransform.sizeDelta = new Vector2(staminaBarWidth * ratio, staminaBarOverlay.rectTransform.rect.height);
	}

	private void UpdateHealthUI() {
		float current = healthBarOverlay.rectTransform.rect.width / healthBarWidth;
		float target = healthController.health / healthController.maxHealth;

		float ratio = Utils.EaseTowards(current, target, 5f, 2f);
		healthBarOverlay.rectTransform.sizeDelta = new Vector2(healthBarWidth * ratio, healthBarOverlay.rectTransform.rect.height);
		healthBarOverlay.color = Color.Lerp(barDamagedColor, barHealthyColor, ratio); ;
	}

	private void UpdateGunUI() {
		GunConfig gun = gunController.config;
		ammoCounterText.text = $"{gun.weaponName} {gunController.ammo}/{gun.maxAmmo}";
	}

	public void RefreshScoreText() {
		scoreText.text = $"Score: {pc.Score}";
	}
}
