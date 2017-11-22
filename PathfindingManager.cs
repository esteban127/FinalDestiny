using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PathfindingManager : MonoBehaviour 
{
    public GameObject Target;
    
    private AstarPath PathComponent;
    private TileStorage TileStorageComponent;

	void Awake () {
        PathComponent = GetComponent<AstarPath>();
        TileStorageComponent = Target.GetComponent<TileStorage>();
	}

    void Start() 
    {
        PathComponent.data.gridGraph.nodeSize = ((TileStorageComponent.Width + TileStorageComponent.Height) / 2) / TileStorageComponent.PixelsPerUnit;
        PathComponent.Scan();
    }
	
	void Update () {
		
	}
}
