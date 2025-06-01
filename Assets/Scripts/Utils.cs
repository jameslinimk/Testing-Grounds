using UnityEngine;
using System;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;

public static class Utils {
	public static float EaseOutQuart(float t) {
		return 1 - Mathf.Pow(1 - t, 4);
	}

	public static float EaseTowards(float current, float target, float speed, float maxDistance) {
		return EaseTowards(current, target, speed, maxDistance, EaseOutQuart);
	}

	public static float EaseTowards(float current, float target, float speed, float maxDistance, Func<float, float> easingFunction) {
		if (Mathf.Abs(current - target) < 0.001f) return target;

		float ratioToMax = easingFunction(Mathf.Clamp01(Mathf.Abs(current - target) / maxDistance));
		float step = speed * ratioToMax * Time.deltaTime;

		return Mathf.MoveTowards(current, target, step);
	}

	public static int WrapAround(int index, int max) {
		return ((index % max) + max) % max;
	}

	public static bool ContainsLayer(this LayerMask mask, int layer) {
		return (mask & (1 << layer)) != 0;
	}

	public static bool Is<T>(this T source, params T[] list) {
		if (source == null) throw new ArgumentNullException(nameof(source), "Source cannot be null.");
		return list.Contains(source);
	}

	public static T Rand<T>(this List<T> source) {
		if (source == null || source.Count == 0) throw new ArgumentException("Source list cannot be null or empty.", nameof(source));
		return source[UnityEngine.Random.Range(0, source.Count)];
	}

	public static T Rand<T>(this T[] source) {
		if (source == null || source.Length == 0) throw new ArgumentException("Source array cannot be null or empty.", nameof(source));
		return source[UnityEngine.Random.Range(0, source.Length)];
	}

	public static T Rand<T>(this Dictionary<T, float> source) {
		if (source == null || source.Count == 0) throw new ArgumentException("Source dictionary cannot be null or empty.", nameof(source));

		float totalWeight = source.Values.Sum();
		if (totalWeight <= 0) throw new InvalidOperationException("Total weight must be greater than zero.");
		float randomValue = UnityEngine.Random.Range(0, totalWeight);

		float cumulativeWeight = 0f;
		foreach (var kvp in source) {
			cumulativeWeight += kvp.Value;
			if (randomValue < cumulativeWeight) {
				return kvp.Key;
			}
		}
		throw new InvalidOperationException("Failed to select a random item from the dictionary.");
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

	private static readonly Vector4[] s_UnitCube = {
		new(-0.5f,  0.5f, -0.5f, 1),
		new(0.5f,  0.5f, -0.5f, 1),
		new(0.5f, -0.5f, -0.5f, 1),
		new(-0.5f, -0.5f, -0.5f, 1),

		new(-0.5f,  0.5f,  0.5f, 1),
		new(0.5f,  0.5f,  0.5f, 1),
		new(0.5f, -0.5f,  0.5f, 1),
		new(-0.5f, -0.5f,  0.5f, 1)
	};

	public static void DrawBox(Vector4 pos, Vector3 size, Color color, float duration = 0) {
		Vector4[] v = s_UnitCube;
		Vector4 sz = new Vector4(size.x, size.y, size.z, 1);
		for (int i = 0; i < 4; i++) {
			var s = pos + Vector4.Scale(v[i], sz);
			var e = pos + Vector4.Scale(v[(i + 1) % 4], sz);
			Debug.DrawLine(s, e, color, duration);
		}
		for (int i = 0; i < 4; i++) {
			var s = pos + Vector4.Scale(v[4 + i], sz);
			var e = pos + Vector4.Scale(v[4 + ((i + 1) % 4)], sz);
			Debug.DrawLine(s, e, color, duration);
		}
		for (int i = 0; i < 4; i++) {
			var s = pos + Vector4.Scale(v[i], sz);
			var e = pos + Vector4.Scale(v[i + 4], sz);
			Debug.DrawLine(s, e, color, duration);
		}
	}

	public static void DrawBox(Matrix4x4 transform, Color color, float duration = 0) {
		Vector4[] v = s_UnitCube;
		Matrix4x4 m = transform;
		for (int i = 0; i < 4; i++) {
			var s = m * v[i];
			var e = m * v[(i + 1) % 4];
			Debug.DrawLine(s, e, color, duration);
		}
		for (int i = 0; i < 4; i++) {
			var s = m * v[4 + i];
			var e = m * v[4 + ((i + 1) % 4)];
			Debug.DrawLine(s, e, color, duration);
		}
		for (int i = 0; i < 4; i++) {
			var s = m * v[i];
			var e = m * v[i + 4];
			Debug.DrawLine(s, e, color, duration);
		}
	}

	public static Vector2 GetClosestCardinalDirection(Vector2 input) {
		if (input == Vector2.zero) return Vector2.zero;

		Vector2 normalizedInput = input.normalized;

		var cardinalDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

		Vector2 closest = Vector2.zero;
		float maxDot = -Mathf.Infinity;

		foreach (Vector2 dir in cardinalDirections) {
			float dot = Vector2.Dot(normalizedInput, dir);
			if (dot > maxDot) {
				maxDot = dot;
				closest = dir;
			}
		}

		return closest;
	}

	public static void SetDefaultValues(object obj) {
		FieldInfo[] props = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (FieldInfo prop in props) {
			var d = prop.GetCustomAttribute<DefaultValueAttribute>();
			if (d != null) prop.SetValue(obj, d.Value);
		}
	}

	public static void CheckScriptDefaultValues(MonoBehaviour[] scripts) {
		foreach (MonoBehaviour script in scripts) {
			FieldInfo[] props = script.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (FieldInfo prop in props) {
				var d = prop.GetCustomAttribute<DefaultValueAttribute>();
				if (prop == null) {
					Debug.LogWarning($"{script.name}.{prop.Name} is null!");
				} else if (d != null && !prop.GetValue(script).Equals(d.Value)) {
					Debug.LogWarning($"{script.name}.{prop.Name} doesn't match: Default: {d.Value} | Current: {prop.GetValue(script)}");
				}
			}
		}
	}

	public static void SetScriptDefaultValues(MonoBehaviour[] scripts) {
		foreach (MonoBehaviour script in scripts) {
			FieldInfo[] props = script.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (FieldInfo prop in props) {
				var d = prop.GetCustomAttribute<DefaultValueAttribute>();
				if (d == null) continue;
				prop.SetValue(script, d.Value);
			}
		}
	}
}

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	private static T _instance;
	public static T Instance {
		get {
			if (_instance == null) {
				_instance = FindFirstObjectByType<T>();
				if (_instance == null) {
					GameObject singletonObject = new(typeof(T).Name);
					_instance = singletonObject.AddComponent<T>();
				}
			}
			return _instance;
		}
	}

	protected virtual void Awake() {
		if (_instance != null && _instance != this) {
			Destroy(gameObject);
			return;
		}

		_instance = this as T;
		if (gameObject.transform.parent == null) {
			DontDestroyOnLoad(gameObject);
		}
	}
}
