using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// limite de memoria en timepo real (sistema complejo y terminado) = +400 MB
// FPS en tiempo real estable > 60

public class Generator : MonoBehaviour {

    public Text LogText;
    public GenType Type;
	public GameObject TilePrefab;
	public Vector2 Position;
	public Vector2 Size;
	public Vector2 SpriteSize;
	public GenValue Value;
	public bool RandomRotation;
	public NInterpolation InterpolationType;
	public bool ClampChunk = false;

	public Vector2 Seed = new Vector2(-100000,100000000);
	public int Elevation = 45;
	public int Smooth = 18;
	public int Multiplier = 5;
	public int ChunkSize = 12;
    public float Scale = 0.5f;

	private List<GameObject> TileMap = new List<GameObject>();
	private List<GameObject> ChunkMap = new List<GameObject>();
	private float PixelPerUnit;
    private NoiseBase Noise;
	private Quaternion Rot;
	private Tile TileScript;
    

	void Awake()
	{
		PixelPerUnit = TilePrefab.GetComponent<SpriteRenderer> ().sprite.pixelsPerUnit;
		TileScript = GetComponent<Tile> ();
	}

	void Start () {
		Regenerate ();
	}

	public void Regenerate()
	{
        LogText.text = "Initializing World";
        GameObject Temp;
		int TileDeltaX = 0,TileDeltaY=0;

        if (Type == GenType.UnityType)
            Noise = new NoiseBase((double)Random.Range(Seed.x, Seed.y));
		else
            Noise = new NoiseBase((long)Random.Range(Seed.x, Seed.y));

        Noise.Frequency = ChunkSize;
		Noise.heightAddition = Elevation;
		Noise.heightMultplier = Multiplier;
		Noise.Smooth = Smooth;
		Noise.Interpolation = InterpolationType;
		Noise.ClampChunk = ClampChunk;

		int h = 0;
		float width = (float)SpriteSize.x/PixelPerUnit;//Tile.transform.lossyScale.x;
		float height = (float)SpriteSize.y/PixelPerUnit;//Tile.transform.lossyScale.y;

        LogText.text = "Generating Chunks...";
		ChunkMap.Add (new GameObject("TempChunk"));
		for (float x = Position.x; x < Size.x; x++) 
		{
			TileDeltaX++;

            if (Type == GenType.UnityType)
                h = Noise.GetNoise(x);
            else
                h = Noise.GetNoise((int)(x - Position.x), (int)(Size.y - Position.y));

			for (float y = Position.y; y < Position.y + h; y++) 
			{
				TileDeltaY++;
				if (RandomRotation) 
				{
					switch (Random.Range (1, 4)) 
					{
					case 1:Rot = Quaternion.Euler (0, 0, 90);break;
					case 2:Rot = Quaternion.Euler (0, 0, 180);break;
					case 3:Rot = Quaternion.Euler (0, 0, 270);break;
					case 4:Rot = Quaternion.identity;break;
					}
				} 
				else
					Rot = Quaternion.identity;
				
				Temp = Instantiate (TilePrefab, new Vector3 (x * width, y * height, 0), Rot);
				TileMap.Add (Temp);
				Temp.name = "Tile["+x+","+y+"]";

				if (TileDeltaX > ChunkSize && TileDeltaY > ChunkSize)
				{
					ChunkMap.Add (new GameObject ("TempChunk"));
					TileDeltaX = 0;
					TileDeltaY = 0;
				}

				Temp.transform.SetParent (ChunkMap.Last().transform);
				TileScript.TileLayer.Add((int)Size.y-h);


			}


		}
		//Fin de generacion de mapa:

        LogText.text = "Loading Chunk Data...";
		for (int i = 0; i < ChunkMap.Count; i++) 
		{
			ChunkMap [i].transform.SetParent (gameObject.transform);
			ChunkMap [i].name = "Chunk[" + i + "]";
		}
        
        LogText.text = "Loading Tiles...";
		TileScript.ComputeLights (TileMap);

        LogText.text = "Done...";
	}

	public void ClearAll(){
		for (int i = 0; i < TileMap.Count; i++)
			Destroy (TileMap [i].gameObject);
		TileMap.Clear ();
	}

	public void SetGenValues()
	{
		switch (Value) 
		{
		default: break;
		case GenValue.Smooth:
			Seed = new Vector2 (3101631, 138804906);
			Elevation = 53;
			Smooth = 26;
			Multiplier = 8;
			ChunkSize = 25;
		break;

		case GenValue.Hard:
			Seed = new Vector2 (-33101631, 15239720671);
			Elevation = 75;
			Smooth = 19;
			Multiplier = 5;
			ChunkSize = 11;
		break;

		case GenValue.Hills:
			Seed = new Vector2 (-1000000, 10000000);
			Elevation = 80;
			Smooth = 8;
			Multiplier = 15;
			ChunkSize = 5;
		break;

		}

	}
}

[CustomEditor(typeof(Generator))]
public class GeneratorEditor : Editor 
{
	private Generator Gen;

	public void OnEnable()
	{	
		Gen = (Generator)target;
	}

	public override void OnInspectorGUI()
	{
		GUILayout.Space (10);

        Gen.LogText = (Text)EditorGUILayout.ObjectField("Log Text", Gen.LogText, typeof(Text), true);
		Gen.TilePrefab = (GameObject)EditorGUILayout.ObjectField ("Tile",Gen.TilePrefab,typeof(GameObject),true);
        Gen.Type = (GenType)EditorGUILayout.EnumPopup("Generator Type:", Gen.Type);
		GUILayout.BeginVertical ("Box");
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label ("Properties: [" + Gen.Type + "]");
			GUILayout.Space (5);
			Gen.Position = EditorGUILayout.Vector2Field ("Position",Gen.Position);
			Gen.Size = EditorGUILayout.Vector2Field ("Width/Height",Gen.Size);
			Gen.SpriteSize = EditorGUILayout.Vector2Field ("TileSize",Gen.SpriteSize);
			Gen.RandomRotation = EditorGUILayout.Toggle("Random Rotation",Gen.RandomRotation);
			GUILayout.Space (5);
			Gen.Value = (GenValue)EditorGUILayout.EnumPopup ("[Generator Values]", Gen.Value);
			Gen.SetGenValues ();
			GUILayout.Space (5);
			switch (Gen.Type) 
			{
			default: GUILayout.Label ("No Type Selected"); break;

            case GenType.UnityType:
				Gen.Seed = EditorGUILayout.Vector2Field ("[Float]Seed", Gen.Seed);
				Gen.ClampChunk = EditorGUILayout.Toggle ("Clamp Chunk", Gen.ClampChunk);
				Gen.Elevation=EditorGUILayout.IntField ("Elevation", Gen.Elevation);
				Gen.Smooth=EditorGUILayout.IntField ("Smooth", Gen.Smooth);
				Gen.Multiplier=EditorGUILayout.IntField ("Multiplier", Gen.Multiplier);
				Gen.ChunkSize=EditorGUILayout.IntField ("ChunkSize", Gen.ChunkSize);
			break;

            case GenType.CustomType:
				Gen.Seed = EditorGUILayout.Vector2Field ("[Long]Seed", Gen.Seed);
				Gen.ClampChunk = EditorGUILayout.Toggle ("Clamp Chunk", Gen.ClampChunk);
				Gen.Elevation = EditorGUILayout.IntField ("Elevation", Gen.Elevation);
				Gen.ChunkSize = EditorGUILayout.IntField ("ChunkSize", Gen.ChunkSize);
				Gen.InterpolationType = (NInterpolation)EditorGUILayout.EnumPopup ("Interpolation", Gen.InterpolationType);
			break;
			}
		GUILayout.Space (10);
		/*
		if (GUILayout.Button ("Generate"))
			Gen.Regenerate ();
		if (GUILayout.Button ("Clear All"))
			Gen.ClearAll ();*/
		
		GUILayout.EndVertical ();
		//base.DrawDefaultInspector ();
	}

}