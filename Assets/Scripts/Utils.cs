using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour {

	public static int ToIndex(int x,int y,int width){
		return (y * width + x);
	}

    public static float FRandom(float value,float min, float max) {
        value=Mathf.Clamp01(value);
        return value * (max - min) + min;
    }

	public static Vector3 GetLocalUpVector(Transform trans) {
		return trans.worldToLocalMatrix.MultiplyVector (trans.up);
	}

	public static Vector3 GetLocalForwardVector(Transform trans) {
		return trans.worldToLocalMatrix.MultiplyVector (trans.forward);
	}

	public static Vector3 GetLocalRightVector(Transform trans) {
		return trans.worldToLocalMatrix.MultiplyVector (trans.right);
	}

	public static Vector3 GetWorldUpVector(Transform trans) {
		return trans.localToWorldMatrix.MultiplyVector (trans.up);
	}

	public static Vector3 GetWorldForwardVector(Transform trans) {
		return trans.localToWorldMatrix.MultiplyVector (trans.forward);
	}

	public static Vector3 GetWorldRightVector(Transform trans) {
		return trans.localToWorldMatrix.MultiplyVector (trans.right);
	}
}
