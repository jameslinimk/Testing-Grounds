using UnityEngine;
using System;

public static class Utils {
	public static float EaseOutQuart(float t) {
		return 1 - Mathf.Pow(1 - t, 4);
	}

	public static float EaseTowards(float current, float target, float speed, float maxDistance) {
		return EaseTowards(current, target, speed, maxDistance, EaseOutQuart);
	}

	public static float EaseTowards(float current, float target, float speed, float maxDistance, Func<float, float> easingFunction) {
		float ratioToMax = easingFunction(Mathf.Clamp01(Mathf.Abs(current - target) / maxDistance));
		float step = speed * ratioToMax * Time.deltaTime;

		return Mathf.MoveTowards(current, target, step);
	}
}
