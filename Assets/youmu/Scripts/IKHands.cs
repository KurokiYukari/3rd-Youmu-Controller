using UnityEngine;
using System.Collections;

public class IKHands : MonoBehaviour{
	private Animator animator;
	public Transform leftHandObj;
	public Transform attachLeft;
	[Range(0, 1)] public float leftHandPositionWeight;
	[Range(0, 1)] public float leftHandRotationWeight;
		
	void Awake() {
		animator = GetComponent<Animator>();
	}
		
	void OnAnimatorIK(int layerIndex){
		if(leftHandObj != null){
			animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandPositionWeight);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandRotationWeight);
			animator.SetIKPosition(AvatarIKGoal.LeftHand, attachLeft.position);                    
			animator.SetIKRotation(AvatarIKGoal.LeftHand, attachLeft.rotation);
		}
	}
}