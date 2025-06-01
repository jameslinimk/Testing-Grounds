using System.ComponentModel;
using TMPro;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour {
	private PlayerController pc;
	private Rigidbody rb;
	private Animator animator;

	[Header("Health Settings")]
	[DefaultValue(100f)] public float maxHealth;
	[DefaultValue(100f)] public float health;
	public bool isDead => health <= 0;
	public TextMeshProUGUI endText;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	void Start() {
		animator = transform.GetChild(0).GetComponent<Animator>();
		endText.gameObject.SetActive(false);
		pc = GetComponent<PlayerController>();
		rb = GetComponent<Rigidbody>();
	}

	public void TakeDamage(float damage, Vector3 hitOrigin) {
		health -= damage;
		rb.AddForce((transform.position - hitOrigin).normalized * 10, ForceMode.Impulse);

		if (health <= 0) Die(hitOrigin);
	}

	public void Die(Vector3? hitOrigin = null) {
		if (hitOrigin == null) hitOrigin = Vector3.forward;

		Vector3 opposite = -hitOrigin.Value;
		opposite.y = 0f;
		opposite.Normalize();

		var directions = new Vector2[] { new(0, 1), new(0, -1), new(-1, 0), new(1, 0) };

		Vector2 inputDir = new(opposite.x, opposite.z);
		Vector2 bestDir = directions[0];
		float maxDot = Vector2.Dot(inputDir, directions[0]);

		for (int i = 1; i < directions.Length; i++) {
			float dot = Vector2.Dot(inputDir, directions[i]);
			if (dot > maxDot) {
				maxDot = dot;
				bestDir = directions[i];
			}
		}

		animator.SetFloat("DeathHorizontal", bestDir.x);
		animator.SetFloat("DeathVertical", bestDir.y);
		animator.SetTrigger("Die");

		rb.isKinematic = true;

		endText.text = $"You died with a score of {pc.Score}!";
		endText.gameObject.SetActive(true);
		// GameManager.Instance.SetPause(true, false);
	}
}
