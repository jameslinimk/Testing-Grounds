using System.ComponentModel;
using TMPro;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour {
	private PlayerController pc;
	private Rigidbody rb;

	[Header("Health Settings")]
	[DefaultValue(100f)] public float maxHealth;
	[DefaultValue(100f)] public float health;
	public TextMeshProUGUI endText;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	void Start() {
		endText.gameObject.SetActive(false);
		pc = GetComponent<PlayerController>();
		rb = GetComponent<Rigidbody>();
	}

	public void TakeDamage(float damage, Vector3 hitOrigin) {
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
