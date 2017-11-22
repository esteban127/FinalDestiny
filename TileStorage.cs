using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct TileInterator 
{
    public const int TileExtendedCapacity = 7;
    
    public Vector2Int Size;
    public TileBase[] TileData;
    public TileBase[] TileDataExtended;
    public string Name;
    
    public TileInterator(int width,int height,string name = "None") 
    {
        Size = new Vector2Int(width, height);
        TileData = new TileBase[width*height];
        TileDataExtended = new TileBase[TileExtendedCapacity];
        Name = name;
    }

    public void SetTileAt(int x,int y,TileBase data)
    {
        TileData[Utils.ToIndex(x, y, Size.x)] = data;
    }

    public void SetTileExAt(int index, TileBase data)
    {
        TileDataExtended[index] = data;
    }

    public TileBase GetTileAt(int x, int y) 
    {
        if ((x < Size.y && y < Size.x) && (x >= 0 && y >= 0))
            return TileData[Utils.ToIndex(x,y,Size.x)];
        else
            return null;
    }

    public TileBase GetTileAt(Vector2Int pos)
    {
        return GetTileAt(pos.x, pos.y);
    }

    public TileBase GetTileExAt(int index)
    {
        if (index >= 0 && index < 6)
            return TileDataExtended[index];
        else
            return null;
    }

    public void Clear()
    {
        if (TileData == null)
            return;
        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
                TileData[Utils.ToIndex(x,y,Size.x)] = null;
    }

}

public class TileStorage : MonoBehaviour 
{
    public List<TileInterator> Data;
    public int Width, Height;
    public float PixelsPerUnit;

    public List<TileInterator> GetTileList() 
    {
        return Data;
    }

    public TileInterator[] GetTileArray()
    {
        return Data.ToArray();
    }
}

