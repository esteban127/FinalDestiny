using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class CreateAnimationClip : MonoBehaviour {

	// Use this for initialization
	public string Name;
	public List<Sprite> Sprites;
	public int FPS;
	public bool Loop;
	public Transform Path;

	private AnimationClip AnimClip;
	private EditorCurveBinding SpriteBinding;
	private ObjectReferenceKeyframe[] SpriteKeyFrames;

	string GetRelativeName( Transform t, bool includeSelf )
	{
		if( t == null )
			return string.Empty;

		string path = includeSelf ? t.name : string.Empty;
		while( t.parent != null )
		{
			t = t.parent.gameObject.transform;
			path = t.name + "/" + path;
		}

		int i = path.IndexOf( "/" );
		if( i < 0 )
			path = "";
		else
			path = path.Substring( i+1 ) ;

		return path;
	}

	public void CreateClip()
	{
		print (GetRelativeName(Path,false));

		AnimClip = new AnimationClip ();
		AnimClip.name = Name;
		AnimClip.frameRate = FPS;
		if (Loop)
			AnimClip.wrapMode = WrapMode.Loop;
		else
			AnimClip.wrapMode = WrapMode.Default;
		


		SpriteBinding = new EditorCurveBinding ();
		SpriteBinding.type = typeof(SpriteRenderer);
		SpriteBinding.path = GetRelativeName(Path,false);
		SpriteBinding.propertyName = "Sprite Renderer";
	
		SpriteKeyFrames = new ObjectReferenceKeyframe[Sprites.Count];
		for (int i = 0; i < Sprites.Count; i++) 
		{
			SpriteKeyFrames [i] = new ObjectReferenceKeyframe ();
			SpriteKeyFrames [i].time = i;
			SpriteKeyFrames [i].value = Sprites [i];

		}
		AnimationUtility.SetObjectReferenceCurve (AnimClip, SpriteBinding, SpriteKeyFrames);


		AssetDatabase.CreateAsset (AnimClip, "assets/Animations/" + Name + ".anim");
	}


}

[CustomEditor(typeof(CreateAnimationClip))]
public class AnimationBuilder : Editor 
{
	public override void OnInspectorGUI ()
	{
		//DrawDefaultInspector ();
		base.OnInspectorGUI ();
		CreateAnimationClip Builder = (CreateAnimationClip)target;

		if(GUILayout.Button("Create Animation Clip"))
			Builder.CreateClip ();
	}

}