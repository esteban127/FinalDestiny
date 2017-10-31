using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public enum GenType
{
    None,
    UnityType,
    UnityTypePrime,
    DynamicMoveType,
    CustomType
}

public enum GenValue
{
    Smooth,
    Hard,
    Hills,
    Custom
}

[System.Serializable]
public struct TerrainType
{
    public string Name;
    public Color Colour;
    public float Height;
    public int id;
}

[System.Serializable]
public enum EncodingType 
{ 
    None,
    JPG,
    PNG,
    EXR
}

[RequireComponent(typeof(Grid))]
public class TileMapGen : MonoBehaviour 
{
    public Rect Transform;
    public NInterpolation InterpolationType;
    public GenType Type;
    public bool ClampChunk = false;
    public bool SortingAuto = true;
    public GenValue Value;

    public Vector2 Seed = new Vector2(-100000, 1000000);
    public int Elevation = 45;
    public int Smooth = 18;
    public int Multiplier = 5;
    public int Frequency = 12;
    public float Scale = 0.5f;
    public int ChunkSize = 1;

    //Dynamic Move Type:
    private System.Random Rand;
    public int MoveMode;
    public bool ResetOnMove = true;
    //Dynamic Move Type.

    //Unity Prime Type:
    public bool ActiveRegions;
    public TerrainType[] Regions;
    public FilterMode RegionsFilterMode;
    public Vector2 OctavesOffset;
    public Vector2 OctavesRange = new Vector2(-100000,100000);
    public int Octaves = 1;
    public float Persistance = 1;
    public float Lacunarity = 1;
    public EncodingType EncodeType = EncodingType.JPG;
    public Material GeneratedMaterial;
    //Unity Prime Type.

    public TileBase Cached;
    public TileBase[] Tiles;
    public int SelectedTile = 0;
    public int SelectedLayer = 0;

    //privados:
    private Grid GridSystem;
    private Tilemap[] LayerMap;
    private NoiseBase Noise;
    private Texture2D GeneratedTexture;
    private Vector2 OldLocation;
    private bool Copy;
    private Rect CopyLocation;


    void Awake() 
    {
        if (Tiles.Length > 0 && Tiles[0] != null) 
        {
            TileData temp = new TileData();
            Tiles[0].GetTileData(new Vector3Int(0, 0, 0), null, ref temp);
            GridSystem = GetComponent<Grid>();
            GridSystem.cellSize = new Vector3(temp.sprite.texture.width / temp.sprite.pixelsPerUnit, temp.sprite.texture.height / temp.sprite.pixelsPerUnit, 0.0f);
        }
    }

    void Start() 
    {
        GenerateLayer();   
    }

    void Update() 
    {
        /*var deltaPos = LayerMap[SelectedLayer].WorldToCell(GameObject.FindGameObjectWithTag("Player").transform.position);
        var deltaChunkWorld = LayerMap[SelectedLayer].WorldToCell(LayerMap[SelectedLayer].transform.position);
        var deltaSize = LayerMap[SelectedLayer].CellToWorld(new Vector3Int(deltaChunkWorld.x + ChunkSize,0,0));
        int deltaLayerSelect = SelectedLayer + 1;
        if (deltaLayerSelect >= LayerMap.Length)
            deltaLayerSelect = 0;
        if (deltaPos.x > (deltaChunkWorld.x + ChunkSize / 2))
        {

        }
        else
        { 
        
        }*/
    }

    public void Init() {
        LayerMap = GetComponentsInChildren<Tilemap>();
        if (Octaves <= 0)
            Octaves = 1;
        if (ChunkSize <= 0)
            ChunkSize = 1;
    }

    public void GenerateLayer(bool preview = false) 
    {
        Init();
        int TileDeltaX = 0;

        switch (Type) 
        {
            default: break;
            case GenType.UnityType: Noise = new NoiseBase((double)Random.Range(Seed.x, Seed.y)); break;
            case GenType.CustomType: Noise = new NoiseBase((long)Random.Range(Seed.x, Seed.y)); break;
            case GenType.DynamicMoveType: Rand = new System.Random((int)Random.Range(Seed.x,Seed.y)); break;
        }

        LayerMap[SelectedLayer].gameObject.SetActive(false);

        switch (Type) 
        {
            //Custom Type - Unity Type se generan aca
            default:
                int height = 0;

                Noise.Frequency = Frequency;
		        Noise.heightAddition = Elevation;
		        Noise.heightMultplier = Multiplier;
		        Noise.Smooth = Smooth;
		        Noise.Interpolation = InterpolationType;
		        Noise.ClampChunk = ClampChunk;

                for (int x = (int)Transform.x; x < Transform.width; x++) 
                {
                    TileDeltaX++;
                    switch (Type)
                    {
                        default: break;
                        case GenType.UnityType:
                            height = Noise.GetNoise(x); 
                        break;
                        case GenType.CustomType:
                            height = Noise.GetNoise((int)(x - Transform.x), (int)(Transform.height - Transform.y)); 
                        break;
                    }

                    for (int y = (int)Transform.y; y < Transform.y + height; y++)
                    {
                        SetTileAt(x, y, preview);
                        if (Copy)
                            SetTileAt((int)CopyLocation.x + x, (int)CopyLocation.y + y, preview);
                        /*if (TileDeltaX <= ChunkSize)
                            SetTileAt(x - (SelectedLayer * ChunkSize), y, preview);
                        else
                        {
                            if (SelectedLayer < LayerMap.Length)
                                SelectedLayer++;
                            TileDeltaX = 0;
                            SetTileAt(x - (SelectedLayer * ChunkSize), y, preview);
                        }*/
                    }
                }
                
                
            break;
            
            //Unity Prime se genera aca:
            case GenType.UnityTypePrime:
                float[,] noiseMap;
                Noise = new NoiseBase();
                Noise.OctavesOffset = OctavesOffset;
                Noise.OctavesRange = OctavesRange;
                Noise.Seed = (int)Random.Range(Seed.x, Seed.y);
                noiseMap = Noise.GetNoise((int)Transform.width, (int)Transform.height, Scale, Octaves, Persistance, Lacunarity);
                var w = noiseMap.GetLength(0);
                var h = noiseMap.GetLength(1);
                
                //Cosntuctor de Textura y material:
                GeneratedTexture = new Texture2D(w, h);
                Color[] colorMap = new Color[w * h];
                for(int y = 0; y < h; y++ )
                {
                    for(int x = 0; x < w; x++)
                    {
                        if(ActiveRegions){
                            float currentHeight = noiseMap[x, y];
                            for (int i = 0; i < Regions.Length; i++) { 
                                if(currentHeight <= Regions[i].Height)
                                    colorMap[Utils.ToIndex(x,y,w)] = Regions[i].Colour;
                            }
                        }
                        else
                            colorMap[Utils.ToIndex(x, y, w)] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                    }
                }
                GeneratedTexture.filterMode = RegionsFilterMode;
                GeneratedTexture.SetPixels(colorMap);
                GeneratedTexture.Apply();
                if(GeneratedMaterial != null)
                    GeneratedMaterial.mainTexture = GeneratedTexture;
                //Generacion de Mapa (inicio):
                for (int x = (int)Transform.x; x < w; x++)
                {
                    TileDeltaX++;
                    for (int y = (int)Transform.y; y < h; y++)
                    {
                        for (int i = 0; i < Regions.Length; i++) 
                        {
                            if(colorMap[Utils.ToIndex(x,y,w)] == Regions[i].Colour)
                                SelectedTile = Regions[i].id;
                        }

                        SetTileAt(x , y, preview);
                        if (Copy)
                            SetTileAt((int)CopyLocation.x + x, (int)CopyLocation.y + y, preview);
                            
                        
                            
                        /*if (TileDeltaX <= ChunkSize)
                            SetTileAt(x-(SelectedLayer*ChunkSize), y, preview);
                        else 
                        {
                            if(SelectedLayer < LayerMap.Length)
                                SelectedLayer++;
                            TileDeltaX = 0;
                            SetTileAt(x - (SelectedLayer * ChunkSize), y, preview);
                        }*/
                    }
                }
                //Generacion de Mapa (fin)
            return;

            //Dynamic Move Type se genera aca:
            case GenType.DynamicMoveType:
                if (ResetOnMove) {
                    OldLocation.x = Transform.x;
                    OldLocation.y = Transform.y;
                }
                
                for (int i = 0; i < Octaves; i++) 
                {
                    switch (Rand.Next(0, 5)) 
                    {
                        case 0: Transform.x--; break;
                        case 1: Transform.x++; break;
                        case 2: Transform.y--; break;
                        case 3: Transform.y++; break;
                    }

                    switch (MoveMode) {
                        default: break;
                        case 0: SetTileAt((int)Transform.x, (int)Transform.y, preview); break;
                        case 1: RemoveTileAt((int)Transform.x, (int)Transform.y, preview); break;
                        case 2: Cached = RemoveTileAt((int)Transform.x, (int)Transform.y, preview); break;
                    }

                    
                }
                if (ResetOnMove)
                {
                    Transform.x = OldLocation.x;
                    Transform.y = OldLocation.y;
                }
            break;

        }
        LayerMap[SelectedLayer].gameObject.SetActive(true);

        SelectedLayer = 0;
       
    }

    public void ClearLayer(bool preview = false) 
    {
        if (!preview)
            LayerMap[SelectedLayer].ClearAllTiles();
        else
            LayerMap[SelectedLayer].ClearAllEditorPreviewTiles();
    }

    public void RemoveLayer(bool preview = false) 
    {
        for (int y = 0; y < Transform.height+Elevation; y++)
        {
            for (int x = 0; x < Transform.width; x++)
            {
                RemoveTileAt(x, y, preview);
                
            }     
        }
    }

    public void SetTileAt(int x, int y, bool preview)
    {
        if (SelectedTile >= Tiles.Length)
        {
            Debug.LogError("Tile ID[" + SelectedTile + "] is equal or greather than maximum tile capacity");
            return;
        }

        if (!preview)
        {
            LayerMap[SelectedLayer].SetTile(new Vector3Int(x, y, 0), Tiles[SelectedTile]);
        }
        else
        {
            LayerMap[SelectedLayer].SetEditorPreviewTile(new Vector3Int(x, y, 0), Tiles[SelectedTile]);
        }
            
        
    }

    public TileBase RemoveTileAt(int x, int y, bool preview = false) 
    {
        if (LayerMap[SelectedLayer].GetTile(new Vector3Int(x, y, 0)) == null && !preview)
            return null;
        
        TileBase cached = LayerMap[SelectedLayer].GetTile(new Vector3Int(x, y, 0));
        if (!preview)
        {
            LayerMap[SelectedLayer].SetTile(new Vector3Int(x, y, 0), null);
            return cached;
        }
        else 
        {
            LayerMap[SelectedLayer].SetEditorPreviewTile(new Vector3Int(x, y, 0), null);
            return cached;
        }
        
    }

    public void ChangeChunkLocation(Vector3Int oldPos,Vector3Int newPos)
    {
        Rect temp = Transform;
        Transform = new Rect(-ChunkSize, 0, Transform.x, ChunkSize);
        Copy = true;
        CopyLocation = new Rect(ChunkSize+temp.width, 0, 0, 0);
        SelectedLayer = 1;
        SelectedTile = 0;
        GenerateLayer();

        Transform = temp;
        
        /*LayerMap[SelectedLayer].gameObject.SetActive(false);
        for (int tileX = oldPos.x; tileX < oldPos.x+ChunkSize; tileX++)
        {
            for (int tileY = oldPos.y; tileY < oldPos.y+ChunkSize; tileY++)
            {
                Cached = LayerMap[SelectedLayer].GetTile(new Vector3Int(tileX, tileY, 0));
                if (Cached != null)
                {
                    LayerMap[SelectedLayer].SetTile(new Vector3Int(tileX, tileY, 0), null);
                    LayerMap[SelectedLayer].SetTile(newPos, Cached);
                }
                
            }
        }
        LayerMap[SelectedLayer].gameObject.SetActive(true);*/
        
        /*for (int x = startx; x <= endx; x += 1)
        {
            for (int y = starty; y >= endy; y -= 1)
            {
                int nx = Helper.Mod (x, World.Width);
                int ny = Helper.Mod (y, World.Height);

                TileData tile = World.GetTile (nx, ny);
                tile.Refresh();
            }
        }*/
        /*
        public static int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }    */
    }

    public void SetGenValues()
    {
        switch (Value)
        {
            default: break;
            case GenValue.Smooth:
                Seed = new Vector2(3101631, 138804906);
                Elevation = 53;
                Smooth = 26;
                Multiplier = 8;
                Frequency = 25;
                break;

            case GenValue.Hard:
                Seed = new Vector2(-33101631, 15239720671);
                Elevation = 75;
                Smooth = 19;
                Multiplier = 5;
                Frequency = 11;
                break;

            case GenValue.Hills:
                Seed = new Vector2(-1000000, 10000000);
                Elevation = 80;
                Smooth = 8;
                Multiplier = 15;
                Frequency = 5;
                break;

        }

    }

    public Texture2D GetGeneratedTexture()
    {
        return GeneratedTexture;
    }

    public Tilemap GetLayer(int index){
        return LayerMap[index];
    }

    public Tilemap[] GetLayers()
    {
        return LayerMap;
    }

}

//Custom Inspector Editor:
[CustomEditor(typeof(TileMapGen))]
public class TileMapGenEditor : Editor
{
    private TileMapGen Gen;
    private int Tab,TabMode,LayerSelect = 0,TileSelect = 0;
    private int LastTileLength;
    private bool RandomBaseTile = true,AutoUpdate = false,Preview = true;
    private string FilePath = "GeneratedImage";
    
    //Estilos:
    private GUIStyle labelStyle;

    public void OnEnable()
    {
        Gen = (TileMapGen)target;
        labelStyle = new GUIStyle();
    }

    public override void OnInspectorGUI()
    {
        
        GUILayout.Space(10);

        Tab = GUILayout.Toolbar (Tab, new string[] {"Generator", "Layers", "Tiles"});
        switch (Tab)
        {
            default: break;
            case 0: GeneratorTab(); break;
            case 1: LayersTab(); break;
            case 2: TilesTab(); break;
        }

        if (AutoUpdate && Gen.Type == GenType.UnityTypePrime)
            Gen.GenerateLayer(true);
    }

    private void GeneratorTab()
    {
        GUILayout.Space(10);
        Gen.Type = (GenType)EditorGUILayout.EnumPopup("Generator Type:", Gen.Type);
        GUILayout.BeginVertical("Box");
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Properties: [" + Gen.Type + "]");
        GUILayout.Space(5);
        Gen.Transform = EditorGUILayout.RectField("Transform", Gen.Transform);
        GUILayout.Space(5);

        if (Gen.Type != GenType.UnityTypePrime) 
        { 
            Gen.Value = (GenValue)EditorGUILayout.EnumPopup("[Generator Values]", Gen.Value);
            Gen.SetGenValues();
            GUILayout.Space(5);
        }

        Gen.ChunkSize = EditorGUILayout.IntField("Chunk Size", Gen.ChunkSize);
        switch (Gen.Type)
        {
            default: GUILayout.Label("No Type Selected"); break;

            case GenType.UnityType:
                Gen.Seed = EditorGUILayout.Vector2Field("[Float]Seed", Gen.Seed);
                Gen.ClampChunk = EditorGUILayout.Toggle("Clamp Chunk", Gen.ClampChunk);
                Gen.Elevation = EditorGUILayout.IntField("Elevation", Gen.Elevation);
                Gen.Smooth = EditorGUILayout.IntField("Smooth", Gen.Smooth);
                Gen.Multiplier = EditorGUILayout.IntField("Multiplier", Gen.Multiplier);
                Gen.Frequency = EditorGUILayout.IntField("Frequency", Gen.Frequency);
                break;

            case GenType.UnityTypePrime:
                Gen.Transform.x = 0;Gen.Transform.y = 0;
                Gen.Seed = EditorGUILayout.Vector2Field("[Float]Seed", Gen.Seed);
                
                Gen.GeneratedMaterial = (Material)EditorGUILayout.ObjectField("Material", Gen.GeneratedMaterial, typeof(Material), true);
                Gen.Scale = EditorGUILayout.FloatField("Scale", Gen.Scale);
                
                Gen.Octaves = EditorGUILayout.IntField("Octaves", Gen.Octaves);
                Gen.OctavesRange = EditorGUILayout.Vector2Field("Octaves Range", Gen.OctavesRange);
                Gen.OctavesOffset = EditorGUILayout.Vector2Field("Octaves Offset", Gen.OctavesOffset);
                Gen.Persistance = EditorGUILayout.Slider("Persistance", Gen.Persistance, 0.0f, 1.0f);
                Gen.Lacunarity = EditorGUILayout.FloatField("Lacunarity", Gen.Lacunarity);
                
            break;

            case GenType.CustomType:
                Gen.Seed = EditorGUILayout.Vector2Field("[Long]Seed", Gen.Seed);
                Gen.ClampChunk = EditorGUILayout.Toggle("Clamp Chunk", Gen.ClampChunk);
                Gen.Elevation = EditorGUILayout.IntField("Elevation", Gen.Elevation);
                Gen.Frequency = EditorGUILayout.IntField("Frequency", Gen.Frequency);
                Gen.InterpolationType = (NInterpolation)EditorGUILayout.EnumPopup("Interpolation", Gen.InterpolationType);
           break;

           case GenType.DynamicMoveType:
                Gen.Transform.width = 0;Gen.Transform.height = 0;
                Gen.Octaves = EditorGUILayout.IntField("Moves Count", Gen.Octaves);
           break;
        }
        GUILayout.Space(10);
        
        GUILayout.EndVertical();
    }

    private void LayersTab()
    {
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<-") && LayerSelect > 0)
            LayerSelect--;
        LayerSelect = EditorGUILayout.IntField(LayerSelect, GUILayout.MaxWidth(30.0f));
        if (GUILayout.Button("->") && LayerSelect < Gen.transform.childCount - 1)
            LayerSelect++;
        GUILayout.EndHorizontal();
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Selected Layer [" + LayerSelect + "]");
        Gen.SelectedLayer = LayerSelect;

        switch (Gen.Type) 
        {
            default: 
            if (LayerSelect >= 0) {
                GUILayout.BeginVertical("Box");
                labelStyle.normal.textColor = Color.blue;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("Layer Name: " + Gen.transform.GetChild(LayerSelect).name,labelStyle);

                RandomBaseTile = EditorGUILayout.Toggle("Random Base Tile", RandomBaseTile);
                Gen.SelectedTile = RandomBaseTile ? Random.Range(0, Gen.Tiles.Length) : EditorGUILayout.IntField("Base Layer Tile: ", Gen.SelectedTile);
            
                GUILayout.EndVertical();
            }
            break;

            case GenType.UnityTypePrime:
                AutoUpdate = EditorGUILayout.Toggle("Auto Update", AutoUpdate);
                Gen.ActiveRegions = EditorGUILayout.Toggle("Active Regions", Gen.ActiveRegions);
                if (Gen.ActiveRegions) 
                {
                    Gen.RegionsFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Regions Filter Mode", Gen.RegionsFilterMode);
                    //Array en el inspector personalizado:
                    var serializedObject = new SerializedObject(target);
                    var property = serializedObject.FindProperty("Regions");
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(property, true);
                    serializedObject.ApplyModifiedProperties();
                    //fin de array personalizado
                }


                if (Gen.GetGeneratedTexture() != null) 
                {
                    string path = FilePath;
                    GUILayout.BeginVertical("Box");
                    byte[] data = new byte[Gen.GetGeneratedTexture().width * Gen.GetGeneratedTexture().height];
                    FilePath = EditorGUILayout.TextField("Filename", FilePath);
                    Gen.EncodeType = (EncodingType)EditorGUILayout.EnumPopup("Encoding Type", Gen.EncodeType);
                    if (GUILayout.Button("SaveToFile"))
                    {
                        switch (Gen.EncodeType)
                        {
                            default: data = Gen.GetGeneratedTexture().EncodeToJPG(); break;
                            case EncodingType.JPG: FilePath += ".jpg"; data = Gen.GetGeneratedTexture().EncodeToJPG(); break;
                            case EncodingType.PNG: FilePath += ".png"; data = Gen.GetGeneratedTexture().EncodeToPNG(); break;
                            case EncodingType.EXR: FilePath += ".exr"; data = Gen.GetGeneratedTexture().EncodeToEXR(); break;
                        }
                        System.IO.File.WriteAllBytes(FilePath, data);
                        FilePath = path;
                    }
                    GUILayout.EndVertical();
                }  
            break;

            case GenType.DynamicMoveType:
            
            if (LayerSelect >= 0)
            {
                GUILayout.BeginVertical("Box");
                labelStyle.normal.textColor = Color.blue;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("Layer Name: " + Gen.transform.GetChild(LayerSelect).name, labelStyle);

                Gen.ResetOnMove = EditorGUILayout.Toggle("Reset On Move",Gen.ResetOnMove);
                RandomBaseTile = EditorGUILayout.Toggle("Random Base Tile", RandomBaseTile);
                Gen.SelectedTile = RandomBaseTile ? Random.Range(0, Gen.Tiles.Length) : EditorGUILayout.IntField("Base Layer Tile: ", Gen.SelectedTile);
                GUILayout.Space(10);
                TabMode = GUILayout.Toolbar(TabMode, new string[] { "Populate", "Erase" , "Cut" });
                Gen.MoveMode = TabMode;

                GUILayout.EndVertical();
            }    
            break;
        }

        GUILayout.Label("Editor Layer Generator");
        Preview=EditorGUILayout.Toggle("Is Editor Preview", Preview);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
            Gen.GenerateLayer(Preview);
        if (GUILayout.Button("Clear"))
            Gen.RemoveLayer(Preview);//Gen.ClearLayer(true);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void TilesTab()
    {
        Rect LastRect;
        TileData tData = new TileData();
        GUILayout.Space(10);
        //Gen.TilesLenght = EditorGUILayout.IntField("Size", Gen.TilesLenght);
        
        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty("Tiles");
        serializedObject.Update();
        EditorGUILayout.PropertyField(property, true);
        serializedObject.ApplyModifiedProperties();
        
        

        GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("<-") && TileSelect > 0)
                TileSelect--;
            GUILayout.Label("Tile: [" + TileSelect + "]");
            if (GUILayout.Button("->") && TileSelect < Gen.Tiles.Length-1)
                TileSelect++;
        GUILayout.EndHorizontal();
        //Propiedades:
        GUILayout.BeginVertical();
            Gen.Tiles[TileSelect] = (TileBase)EditorGUILayout.ObjectField("Tile", Gen.Tiles[TileSelect], typeof(TileBase),true);
            if (Gen.Tiles[TileSelect] != null)
            {
                GUILayout.Label("Name: " + Gen.Tiles[TileSelect].name);
                Gen.Tiles[TileSelect].GetTileData(new Vector3Int(0, 0, 0), null, ref tData);
                GUILayout.Space(80);
                LastRect = GUILayoutUtility.GetLastRect();
                LastRect.width = 50; LastRect.height = 50;
                LastRect.y += 15; LastRect.x = Screen.width / 2 - (LastRect.width / 2);
                GUI.DrawTexture(LastRect, tData.sprite.texture);
                
            }
            
        GUILayout.EndVertical();

        if (GUILayout.Button("Chunk Location"))
        {
            Gen.ChangeChunkLocation(new Vector3Int(0, 0, 0), new Vector3Int(256, 0, 0));
        }
    }
}
