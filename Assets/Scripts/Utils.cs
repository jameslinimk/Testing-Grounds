using UnityEngine;
using System;
using System.Reflection;
using System.ComponentModel;

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

	private static Vector4[] MakeUnitSphere(int len) {
		Debug.Assert(len > 2);
		var v = new Vector4[len * 3];
		for (int i = 0; i < len; i++) {
			var f = i / (float)len;
			float c = Mathf.Cos(f * (float)(Math.PI * 2.0));
			float s = Mathf.Sin(f * (float)(Math.PI * 2.0));
			v[0 * len + i] = new Vector4(c, s, 0, 1);
			v[1 * len + i] = new Vector4(0, c, s, 1);
			v[2 * len + i] = new Vector4(s, 0, c, 1);
		}
		return v;
	}

	private static readonly Vector4[] s_UnitSphere = MakeUnitSphere(16);

	public static void DrawSphere(Vector4 pos, float radius, Color color, float duration = 0) {
		Vector4[] v = s_UnitSphere;
		int len = s_UnitSphere.Length / 3;
		for (int i = 0; i < len; i++) {
			var sX = pos + radius * v[0 * len + i];
			var eX = pos + radius * v[0 * len + (i + 1) % len];
			var sY = pos + radius * v[1 * len + i];
			var eY = pos + radius * v[1 * len + (i + 1) % len];
			var sZ = pos + radius * v[2 * len + i];
			var eZ = pos + radius * v[2 * len + (i + 1) % len];
			Debug.DrawLine(sX, eX, color, duration);
			Debug.DrawLine(sY, eY, color, duration);
			Debug.DrawLine(sZ, eZ, color, duration);
		}
	}

	public static void DrawPoint(Vector4 pos, float scale, Color color, float duration = 0) {
		var sX = pos + new Vector4(+scale, 0, 0);
		var eX = pos + new Vector4(-scale, 0, 0);
		var sY = pos + new Vector4(0, +scale, 0);
		var eY = pos + new Vector4(0, -scale, 0);
		var sZ = pos + new Vector4(0, 0, +scale);
		var eZ = pos + new Vector4(0, 0, -scale);
		Debug.DrawLine(sX, eX, color, duration);
		Debug.DrawLine(sY, eY, color, duration);
		Debug.DrawLine(sZ, eZ, color, duration);
	}

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void SetDefaultValues(object obj) {
		FieldInfo[] props = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (FieldInfo prop in props) {
			var d = prop.GetCustomAttribute<DefaultValueAttribute>();
			if (d != null) prop.SetValue(obj, d.Value);
		}
	}

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void CheckScriptDefaultValues(MonoBehaviour[] scripts) {
		foreach (MonoBehaviour script in scripts) {
			FieldInfo[] props = script.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (FieldInfo prop in props) {
				var d = prop.GetCustomAttribute<DefaultValueAttribute>();
				if (d != null && !prop.GetValue(script).Equals(d.Value)) {
					Debug.LogWarning($"Default value of {script.name}.{prop.Name} does not match current value. Default: {d.Value}, Current: {prop.GetValue(script)}");
				}
			}
		}
	}

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void SetScriptDefaultValues(MonoBehaviour[] scripts) {
		foreach (MonoBehaviour script in scripts) {
			FieldInfo[] props = script.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (FieldInfo prop in props) {
				var d = prop.GetCustomAttribute<DefaultValueAttribute>();
				if (d == null) continue;
				if (!prop.GetValue(script).Equals(d.Value)) {
					Debug.Log($"Default value of {script.name}.{prop.Name} does not match current value. Default: {d.Value}, Current: {prop.GetValue(script)}, changing");
				}
				prop.SetValue(script, d.Value);
			}
		}
	}
}
