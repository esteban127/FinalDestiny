using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour 
{
	public int MaxSurroundedCount = 1;

	[HideInInspector]
	public List<int> TileLayer;

	private bool Surrounded;
	private RaycastHit2D[] Hits;
	int UpCount,DownCount,LeftCount,RightCount;
	private SpriteRenderer TileRenderer;

	void Awake()
	{
		ResetTile ();
	}
		
	public void ComputeLights(List<GameObject> tiles){
		for (int i = 0; i < tiles.Count; i++) {
			ComputeLightsAt (i, tiles [i]);
		}
	}
		
	public void ComputeLightsAt(int index,GameObject tile)
	{
		//Compoenentes:
		TileRenderer = tile.GetComponent<SpriteRenderer> ();

		//Operaciones:
		IsSurrounded (tile.transform.position,Vector2.up,Vector2.right);
		float Val = Mathf.Min (Mathf.Min (UpCount, DownCount), Mathf.Min (LeftCount, RightCount));
		if (Surrounded) {
			tile.layer = 10;
			TileRenderer.material.SetInt ("_AmbientDetected", 0);
		} else {
			tile.layer = 8;
			TileRenderer.material.SetInt ("_AmbientDetected", 1);
		}

		print (Val);
		Val /= MaxSurroundedCount;
		TileRenderer.material.SetFloat("_Mult",1-Mathf.Clamp01(Val));

		ResetTile ();
	}

	private void IsSurrounded(Vector2 position,Vector2 up,Vector2 right)
	{
		TileRaycast (position, up,ref UpCount);
		TileRaycast (position, -up,ref DownCount);
		TileRaycast (position, -right,ref LeftCount);
		TileRaycast (position, right,ref RightCount);
		Surrounded = (
			UpCount >= MaxSurroundedCount && 
			DownCount >= MaxSurroundedCount && 
			LeftCount >= MaxSurroundedCount && 
			RightCount >= MaxSurroundedCount
		);
	}

	private void TileRaycast(Vector2 start,Vector2 direction,ref int value)
	{
		Hits = Physics2D.RaycastAll (start, direction);
		for (int index = 0; index < Hits.Length; index++) {
			if (Hits [index].collider.gameObject.tag == "Wall") {
				value++;
			}
		}
	}
		
	private void ResetTile()
	{
		UpCount = DownCount = LeftCount = RightCount = 0;
		TileRenderer = null;
		Surrounded = false;

	}
}
