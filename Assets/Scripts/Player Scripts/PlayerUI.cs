using UnityEngine;

public partial class PlayerController {
	void UpdateUI() {
		UpdateDashUI();
		UpdateStaminaUI();
		UpdateHealthUI();
	}

	void UpdateDashUI() {
		float ratio = Mathf.Clamp01(1 - (Time.time - dashStart - dashDuration) / dashCooldown);
		dashCDImage.fillAmount = ratio;

		float secondsLeft = Mathf.Clamp(dashCooldown - (Time.time - dashStart - dashDuration), 0f, dashCooldown);
		if (secondsLeft == 0f) {
			dashCDImageAlphaTarget = 0f;
		} else {
			dashCDText.text = $"{secondsLeft:0.0}s";
		}

		dashCDText.alpha = Utils.EaseTowards(dashCDText.alpha, dashCDImageAlphaTarget, 5f, 2f);
	}

	void UpdateStaminaUI() {
		if (staminaBarWidth == 0f) staminaBarWidth = staminaBarOverlay.rectTransform.rect.width;

		float current = staminaBarOverlay.rectTransform.rect.width / staminaBarWidth;
		float target = Stamina / maxStamina;

		float ratio = Utils.EaseTowards(current, target, 5f, 2f);
		staminaBarOverlay.rectTransform.sizeDelta = new Vector2(staminaBarWidth * ratio, staminaBarOverlay.rectTransform.rect.height);
	}

	void UpdateHealthUI() {
		if (healthBarWidth == 0f) healthBarWidth = healthBarOverlay.rectTransform.rect.width;

		float current = healthBarOverlay.rectTransform.rect.width / healthBarWidth;
		float target = health / maxHealth;

		float ratio = Utils.EaseTowards(current, target, 5f, 2f);
		healthBarOverlay.rectTransform.sizeDelta = new Vector2(healthBarWidth * ratio, healthBarOverlay.rectTransform.rect.height);

		// Bar color
		if (barHealthyColor == null) barHealthyColor = healthBarOverlay.color;
		healthBarOverlay.color = Color.Lerp(barDamagedColor, (Color)barHealthyColor, ratio); ;
	}

	void UpdateScoreText() {
		scoreText.text = $"Score: {score}";
	}
}
