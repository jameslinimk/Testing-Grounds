using System.ComponentModel;
using UnityEngine;

public class CrosshairController : MonoBehaviour {
	private static CrosshairController _instance;
	public static CrosshairController Instance => _instance;

	private void Awake() {
		if (_instance != null && _instance != this) {
			Destroy(gameObject);
			return;
		}

		_instance = this;
		DontDestroyOnLoad(gameObject);
	}

	[System.Serializable]
	public struct CrosshairElement {
		public CircleGraphic graphic;
		public float size;
		public float thickness;
		[HideInInspector] public RectTransform rectTransform;
	}

	[Header("Crosshair Settings")]

	[Header("Crosshair Components")]
	public CrosshairElement centerDot;
	public CrosshairElement centerDotBorder;
	public CrosshairElement outerCircle;
	public CrosshairElement outerCircleBorder;

	[Header("Crosshair Colors")]
	public Color primaryColor;
	public Color borderColor;

	[ContextMenu("Default values")]
	void DefaultValues() {
		Utils.SetDefaultValues(this);

		centerDot.size = 4f;
		centerDot.thickness = 0f;

		centerDotBorder.size = 0f;
		centerDotBorder.thickness = 0.3f;

		outerCircle.size = 40f;
		outerCircle.thickness = 1.5f;

		outerCircleBorder.size = 0f;
		outerCircleBorder.thickness = 0.1f;

		primaryColor = Color.white;
		borderColor = Color.black;
	}

	[ContextMenu("Apply crosshair")]
	void Start() {
		centerDot.rectTransform = centerDot.graphic.GetComponent<RectTransform>();
		centerDotBorder.rectTransform = centerDotBorder.graphic.GetComponent<RectTransform>();
		outerCircle.rectTransform = outerCircle.graphic.GetComponent<RectTransform>();
		outerCircleBorder.rectTransform = outerCircleBorder.graphic.GetComponent<RectTransform>();

		UpdateCrosshair();
	}

	private void SetElementSize(ref CrosshairElement element, float size, float extraThickness = 0) {
		float totalSize = size + (extraThickness * 2);
		element.rectTransform.sizeDelta = new Vector2(totalSize, totalSize);
	}

	private void UpdateCrosshair() {
		// Colors
		centerDot.graphic.color = primaryColor;
		outerCircle.graphic.color = primaryColor;
		centerDotBorder.graphic.color = borderColor;
		outerCircleBorder.graphic.color = borderColor;

		// Sizes
		SetElementSize(ref centerDot, centerDot.size);
		SetElementSize(ref centerDotBorder, centerDot.size, centerDotBorder.thickness);

		SetElementSize(ref outerCircle, outerCircle.size);
		outerCircle.graphic.edgeThickness = outerCircle.thickness;

		SetElementSize(ref outerCircleBorder, outerCircle.size, outerCircleBorder.thickness);
		outerCircleBorder.graphic.edgeThickness = outerCircle.thickness + (outerCircleBorder.thickness * 2);
	}

	private void SetOuterSize(float size) {
		outerCircle.size = size;
		SetElementSize(ref outerCircle, outerCircle.size);
		SetElementSize(ref outerCircleBorder, outerCircle.size, outerCircleBorder.thickness);
	}

	private float outerCircleSizeTarget = 0f;

	public void UpdateOuterCircleSize(float currentSpread) {
		outerCircleSizeTarget = currentSpread * 10;
	}

	public void Shoot(float duration = 0f) {
		SetOuterSize(outerCircle.size + 10f);
	}

	void Update() {
		SetOuterSize(Mathf.MoveTowards(outerCircle.size, outerCircleSizeTarget, 10f * Time.deltaTime));

		// TODO crosshair stuff
	}
}
