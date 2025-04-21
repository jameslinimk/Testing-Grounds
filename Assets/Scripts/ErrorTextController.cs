using System.Collections;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class ErrorTextController : Singleton<MonoBehaviour> {
	private TextMeshProUGUI text;
	[DefaultValue(3f)] public float fadeSpeed;

	[ContextMenu("Default Values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);
	}

	void Start() {
		text = GetComponent<TextMeshProUGUI>();
	}

	private Coroutine fadeCoroutine;

	public void SetText(string message, float duration = 2f) {
		text.text = message;
		if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
		fadeCoroutine = StartCoroutine(FadeOut(duration));
	}

	IEnumerator FadeOut(float duration) {
		yield return new WaitForSeconds(duration);

		float alpha = text.color.a;
		while (alpha > 0) {
			alpha -= fadeSpeed * Time.deltaTime;
			text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
			yield return null;
		}
		text.text = "";
		text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
	}
}
