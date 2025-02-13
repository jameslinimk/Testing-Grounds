using TMPro;
using UnityEngine;

public partial class PlayerController {
	[Header("Health Settings")]
	public float maxHealth;
	public float health;
	public TextMeshProUGUI endText;

	void DefaultHealthValues() {
		maxHealth = 100f;
		health = maxHealth;
	}

	public void TakeDamage(int damage, Vector3 hitOrigin) {
		health -= damage;
		rb.AddForce((transform.position - hitOrigin).normalized * 10, ForceMode.Impulse);

		if (health <= 0) Die();
	}

	public void Die() {
		endText.text = $"You died with a score of {score}!";
		endText.gameObject.SetActive(true);
		GameManager.Instance.SetPause(true, false);
	}
}
