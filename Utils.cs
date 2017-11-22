using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class Utils : MonoBehaviour {

    private static float interpolation;

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

    public static TileData GetTileData(TileBase tile)
    {
        TileData data = new TileData();
        tile.GetTileData(new Vector3Int(0, 0, 0), null, ref data);
        return data;
    }

    public static Vector3Int GetLeftTile(Vector3Int position) {
        return new Vector3Int(position.x - 1, position.y, 0);
    }

    public static Vector3Int GetRightTile(Vector3Int position)
    {
        return new Vector3Int(position.x + 1, position.y, 0);
    }

    public static Vector3Int GetUpTile(Vector3Int position)
    {
        return new Vector3Int(position.x, position.y + 1, 0);
    }

    public static Vector3Int GetDownTile(Vector3Int position)
    {
        return new Vector3Int(position.x, position.y - 1, 0);
    }

    public static int Sign(float Value) 
    {
        if (Value < 0)
            return -1;
        else if (Value > 0)
            return 1;
        else
            return 0;
    }

    public static float FInterpTo(float Current, float Target, float InterpSpeed, float DeltaTime)
    { 
        if( InterpSpeed <= 0.0f )
	        return Target;
        float Dist = Target - Current;
	    float DeltaMove = Dist * Mathf.Clamp(DeltaTime * InterpSpeed, 0.0f, 1.0f);
	    return Current + DeltaMove;
    }

}

public class EUtils : MonoBehaviour
{ 
    public static void EditorPropertyField(Object target,string name)
    {
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(name);
        serializedObject.Update();
        EditorGUILayout.PropertyField(property, true);
        serializedObject.ApplyModifiedProperties();
    }
}