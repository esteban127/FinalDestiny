using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Tiles : MonoBehaviour {

    //Privados:
    private TileMapGen GenRef;

    void Start()
    {
        GenRef = GetComponent<TileMapGen>();
        
        print(IsSurrounded("Layer 0", new Vector3Int(0, 0, 0)));
    }

    public bool IsSurrounded(string layerName,Vector3Int position) 
    {
        TileBase[] other = new TileBase[4];
        Tilemap tm = GameObject.Find(layerName).GetComponent<Tilemap>();

        other[0] = tm.GetTile(new Vector3Int(position.x + 1, position.y, 0));
        other[1] = tm.GetTile(new Vector3Int(position.x, position.y + 1, 0));
        other[2] = tm.GetTile(new Vector3Int(position.x - 1, position.y, 0));
        other[3] = tm.GetTile(new Vector3Int(position.x, position.y - 1, 0));
        foreach (TileBase a in other){
            if (a == null)
                return false;
            else
                return true;
        }

        return false;

    }
}
