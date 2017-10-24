using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public struct IdleProperties
{
	public Sprite Idle;
	public Vector3 Position;
	public Vector3 Scale;
	public Quaternion Rotation;
}

[System.Serializable]
public struct PlayerIdleSet
{
	public List<IdleProperties> Body;
	public List<IdleProperties> ForeArm;
	public List<IdleProperties> BackArm;
}

public enum Gender 
{
	Male = 0,
	Female = 1,
	None = 2

}

public class Player : MonoBehaviour {

	//Publicos:
	public PlayerIdleSet MaleIdle;
	public PlayerIdleSet FemaleIdle;
	public Gender PlayerGender;
	public int CurrentIdle;
	public float Speed = 5.5f;
	public float Jump = 10.0f;
	public bool FlipOnCenter = true;
	public bool Left;

	//Privados:
	private SpriteRenderer[] SR;
	private Transform[] BodyParts;
	private Animator Anim;
	private AnimatorOverrideController AnimOverride;
	private Camera Cam;

	private float HandRotation;
	private Vector3 MouseWorld;
	private float MoveH;

	//Fisicas:
	private Rigidbody2D PhysicsBody;
	private Collider2D Collision;

	//Funciones: 
	void Awake()
	{
		SR = GetComponentsInChildren<SpriteRenderer>();
		PhysicsBody = GetComponent<Rigidbody2D> ();
		Collision = GetComponent<Collider2D> ();
		BodyParts = GetComponentsInChildren<Transform> ();
		Anim = GetComponent<Animator> ();
		Cam = GetComponentInChildren<Camera> ();


		AnimOverride = new AnimatorOverrideController (Anim.runtimeAnimatorController);
		Anim.runtimeAnimatorController = AnimOverride;
	}

	void Start () 
	{
		
	}
		

	// Update is called once per frame
	void Update () 
	{
		Anim.SetInteger ("Type", (int)PlayerGender);

		//MouseWorld = Cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,Cam.nearClipPlane));
		/*HandRotation = Mathf.Atan2(MouseWorld.y - SR[2].transform.position.y,MouseWorld.x - SR[2].transform.position.x);
		HandRotation = HandRotation * (180.0f / Mathf.PI);
		SR [2].transform.localRotation = Quaternion.Euler (0, 0, HandRotation);*/

		if (FlipOnCenter) {
			if (Input.mousePosition.x < (Cam.pixelWidth / 2))
				Flip (true);
			else
				Flip (false);
		}

		//Movimiento:
		if (Input.GetKeyDown (KeyCode.A)) {
			Anim.SetBool ("Walking", true);
			Anim.SetBool ("HandEmpty", true);
			Flip (true);
		}
		if (Input.GetKeyUp (KeyCode.A)) {
			Anim.SetBool ("Walking", false);
			Anim.SetBool ("HandEmpty", false);

		}
		if (Input.GetKeyDown (KeyCode.D)) {
			Anim.SetBool ("Walking", true);
			Anim.SetBool ("HandEmpty", true);
			Flip (false);
		}
		if (Input.GetKeyUp (KeyCode.D)) {
			Anim.SetBool ("Walking", false);
			Anim.SetBool ("HandEmpty", false);
		}

	}

	void FixedUpdate()
	{
		MoveH = Input.GetAxis ("Horizontal");

		if (Input.GetKey (KeyCode.LeftShift)) {
			Speed = 11;
		} else
			Speed = 5.5f;
		PhysicsBody.velocity = new Vector2 (MoveH * Speed, PhysicsBody.velocity.y);

		if(Input.GetKeyDown(KeyCode.W))
			PhysicsBody.AddForce (Vector2.up * Jump);
	}

	void LateUpdate()
	{
		if ( !Anim.GetBool ("Walking") )
		{
			switch (PlayerGender) 
			{
			case Gender.Male:
				SR [1].sprite = MaleIdle.Body [CurrentIdle].Idle;
				BodyParts [2].transform.localPosition = MaleIdle.Body [CurrentIdle].Position;

				SR [2].sprite = MaleIdle.ForeArm [CurrentIdle].Idle;
				if (Left) {
					BodyParts [3].transform.localPosition = MaleIdle.BackArm[CurrentIdle].Position;
				} else {
					BodyParts [3].transform.localPosition = MaleIdle.ForeArm [CurrentIdle].Position;
				}



				SR [3].sprite = MaleIdle.BackArm [CurrentIdle].Idle;
				if (Left) {
					//BodyParts [4].transform.localPosition = MaleIdle.BackArm[CurrentIdle].Position;
					BodyParts [4].transform.localPosition = MaleIdle.ForeArm [CurrentIdle].Position;
				} else {
					BodyParts [4].transform.localPosition = MaleIdle.BackArm [CurrentIdle].Position;

				}

				break;
			case Gender.Female:
				SR [1].sprite = FemaleIdle.Body [CurrentIdle].Idle;
				break;
			case Gender.None:
				break;
			}
		}
	}

	private void Flip(bool flip)
	{
		SR [0].flipX = flip;
		SR [1].flipX = flip;
		SR [2].flipX = flip;
		SR [3].flipX = flip;
		Left = flip;
	}

	public void SetAnimationClip(string name,AnimationClip clip)
	{
		AnimOverride [name] = clip;
	}
}
	
/// <summary>
/// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>

[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor 
{
	private Player PR;
	private int SelectedGenderCount;

	public void OnEnable()
	{
		PR = (Player)target;
	}

	public override void OnInspectorGUI ()
	{
		//base.OnInspectorGUI ();
		GUILayout.Space (10);
		GUILayout.BeginVertical ();
		PR.PlayerGender = (Gender)EditorGUILayout.EnumPopup (PR.PlayerGender);
		switch (PR.PlayerGender) 
		{
			case Gender.Male: SelectedGenderCount = PR.MaleIdle.Body.Count; break;
			case Gender.Female:  SelectedGenderCount = PR.FemaleIdle.Body.Count; break;
		}
		GUILayout.Space(15);
		if (GUILayout.Button ("Add "+PR.PlayerGender+" Idle")) {
			switch (PR.PlayerGender) 
			{
			case Gender.Male: 
				PR.MaleIdle.Body.Add (new IdleProperties());
				PR.MaleIdle.ForeArm.Add (new IdleProperties());
				PR.MaleIdle.BackArm.Add (new IdleProperties());
			break;

			case Gender.Female:
				PR.FemaleIdle.Body.Add (new IdleProperties());
				PR.FemaleIdle.ForeArm.Add (new IdleProperties());
				PR.FemaleIdle.BackArm.Add (new IdleProperties());
			break;
			}
		}
		//Menu de seleccion de Idle de genero:
		GUILayout.BeginHorizontal ("Box");
		if (GUILayout.Button ("<-") && PR.CurrentIdle > 0) 
			PR.CurrentIdle--;
		GUILayout.Label ("Selected: [" + PR.CurrentIdle + "]");
		if (GUILayout.Button ("->") && PR.CurrentIdle < SelectedGenderCount-1) 
			PR.CurrentIdle++;
		GUILayout.EndHorizontal ();

		//Configuracion de idle:
		if(PR.CurrentIdle < SelectedGenderCount && PR.CurrentIdle >= 0)
		{
			GUILayout.Space(5);
			GUILayout.BeginVertical ();

			switch (PR.PlayerGender) {
			case Gender.Male:
				GUI.backgroundColor = new Color(0f,0f,0.8f,0.2f);
				GUILayout.Label ("Male", GUI.skin.box);
				ObjectListInspector (PR.MaleIdle, PR.CurrentIdle);
				break;

			case Gender.Female:
				GUI.backgroundColor = new Color(0.8f,0f,0.8f,0.2f);
				GUILayout.Label ("Female", GUI.skin.box);
				ObjectListInspector (PR.FemaleIdle, PR.CurrentIdle);
				break;

			}
			GUI.backgroundColor = Color.white;
			GUILayout.EndVertical ();

			if (GUILayout.Button ("Remove "+PR.PlayerGender+" Idle")) 
			{
				switch (PR.PlayerGender) 
				{
				case Gender.Male:
					PR.MaleIdle.Body.RemoveAt (PR.CurrentIdle);
					PR.MaleIdle.ForeArm.RemoveAt (PR.CurrentIdle);
					PR.MaleIdle.BackArm.RemoveAt (PR.CurrentIdle);
				break;

				case Gender.Female:
					PR.FemaleIdle.Body.RemoveAt (PR.CurrentIdle);
					PR.FemaleIdle.ForeArm.RemoveAt (PR.CurrentIdle);
					PR.FemaleIdle.BackArm.RemoveAt (PR.CurrentIdle);
				break;
				}

				return;
			}


		}
		GUILayout.EndVertical ();
		PR.FlipOnCenter = EditorGUILayout.Toggle ("Flip On Center",PR.FlipOnCenter);
		GUILayout.Label ("Player Stats");
		GUILayout.BeginVertical ("Box");

			GUILayout.BeginHorizontal ();
				GUILayout.Label ("Walk Speed: ");
				PR.Speed = EditorGUILayout.FloatField (PR.Speed);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
				GUILayout.Label ("Jump Power: ");
				PR.Jump = EditorGUILayout.FloatField (PR.Jump);
			GUILayout.EndHorizontal ();

		GUILayout.EndVertical ();
	}


	private void ObjectListInspector(PlayerIdleSet Obj,int index)
	{
		if (index >= 0 && index < SelectedGenderCount) 
		{
			GUILayout.BeginVertical ("Box");
				EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
				GUILayout.BeginHorizontal ();
					GUILayout.Label ("Body:");
					EditorGUILayout.ObjectField (Obj.Body [index].Idle, typeof(Sprite), true);
				GUILayout.EndHorizontal ();
				EditorGUILayout.Vector3Field ("Position", Obj.Body [index].Position);
				GUILayout.Space (10);
				EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
				GUILayout.BeginHorizontal ();
					GUILayout.Label ("ForeArm:");
					EditorGUILayout.ObjectField (Obj.ForeArm [index].Idle, typeof(Sprite), true);
				GUILayout.EndHorizontal ();
				EditorGUILayout.Vector3Field ("Position", Obj.ForeArm [index].Position);
				GUILayout.Space (10);
				EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
				GUILayout.BeginHorizontal ();
					GUILayout.Label ("BackArm:");
					EditorGUILayout.ObjectField (Obj.BackArm [index].Idle, typeof(Sprite), true);
				GUILayout.EndHorizontal ();
				EditorGUILayout.Vector3Field ("Position", Obj.BackArm [index].Position);
				EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
			GUILayout.EndVertical ();
		}
	}
}