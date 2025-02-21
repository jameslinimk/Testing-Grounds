using TMPro;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour {
	private PlayerController pc;
	private Rigidbody rb;

	[Header("Health Settings")]
	public float maxHealth;
	public float health;
	public TextMeshProUGUI endText;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		maxHealth = 100f;
		health = maxHealth;
	}

	void Start() {
		endText.gameObject.SetActive(false);
		pc = GetComponent<PlayerController>();
		rb = GetComponent<Rigidbody>();
	}

	public void TakeDamage(int damage, Vector3 hitOrigin) {
		health -= damage;
		rb.AddForce((transform.position - hitOrigin).normalized * 10, ForceMode.Impulse);

		if (health <= 0) Die();
	}

	public void Die() {
		endText.text = $"You died with a score of {pc.score}!";
		endText.gameObject.SetActive(true);
		GameManager.Instance.SetPause(true, false);
	}
}
