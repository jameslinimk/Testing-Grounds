using UnityEngine;

public class CrosshairController : MonoBehaviour {
	[Header("Crosshair Components")]
	public CircleGraphic centerDotBorder;
	public CircleGraphic centerDot;
	public CircleGraphic outerCircleBorder;
	public CircleGraphic outerCircle;

	[Header("Crosshair Settings")]
	public float centerDotSize;
	public float centerDotBorderThickness;
	public float outerCircleSize;
	public float outerCircleThickness;
	public float outerCircleBorderThickness;

	[Header("Crosshair Colors")]
	public Color primaryColor;
	public Color borderColor;

	[ContextMenu("Apply crosshair")]
	void Start() {
		centerDot.color = primaryColor;
		outerCircle.color = primaryColor;
		centerDotBorder.color = borderColor;
		outerCircleBorder.color = borderColor;

		centerDot.GetComponent<RectTransform>().sizeDelta = new Vector2(centerDotSize, centerDotSize);
		centerDotBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(centerDotSize + centerDotBorderThickness * 2, centerDotSize + centerDotBorderThickness * 2);

		outerCircle.GetComponent<RectTransform>().sizeDelta = new Vector2(outerCircleSize, outerCircleSize);
		outerCircle.edgeThickness = outerCircleThickness;
		outerCircleBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(outerCircleSize + outerCircleBorderThickness * 2, outerCircleSize + outerCircleBorderThickness * 2);
		outerCircleBorder.edgeThickness = outerCircleThickness + outerCircleBorderThickness * 2;
	}

	void Update() {
		Start();
	}
}
