using UnityEngine;

public class PreventRootMotion : MonoBehaviour {
	private Animator animator;

	void Start() {
		animator = GetComponent<Animator>();
	}

	void OnAnimatorMove() {
		// Prevent root motion application
		// Do NOT set transform.position = animator.rootPosition
	}
}
