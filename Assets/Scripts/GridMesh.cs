using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridMesh : MonoBehaviour 
{
	public Vector2 Size;



	private Mesh GenMesh;
	private Vector3[] Vertex;
	private Vector2[] Uv;
	private Vector4[] Tangents;
	private int[] Triangles;



	// Use this for initialization
	void Awake(){
		GenMesh = GetComponent<MeshFilter> ().mesh;
		GenMesh.name = "Procedural Mesh";

		Vertex = new Vector3[(int)(Size.x * Size.y)];
		Uv = new Vector2[Vertex.Length];
		Tangents = new Vector4[Vertex.Length];
		Vector4 Tangent = new Vector4 (1.0f,0.0f,0.0f,-1.0f);
		for (int i = 0, y = 0; y < Size.y; y++) 
		{
			for (int x = 0; x < Size.x; x++) 
			{
				Vertex[i] = new Vector3(x, y, 0);
				Uv [i] = new Vector2 (x / Size.x, y / Size.y);
				Tangents [i] = Tangent;
				i++;
			}
		}

		Triangles = new int[(int)(Size.x * Size.y * 6)];
        int tIndex = 0;
        /*
		int ti = 0;//indice de triangulos avanza de a 6.
		Triangles[ti] = 0;//valor en y
		Triangles[ti + 3] = Triangles[ti + 2] = 1;//valor en y + 1 
		Triangles[ti + 4] = Triangles[ti + 1] = (int)Size.x;//tam.x + x
		Triangles[ti + 5] = (int)Size.x + 1;//tam.x + x + 1
        */
      
		GenMesh.vertices = Vertex;
		GenMesh.triangles = Triangles;
		GenMesh.uv = Uv;
		GenMesh.RecalculateNormals ();

	}

	void OnDrawGizmos()
	{
		if (Vertex == null) 
			return;
		
		Gizmos.color = Color.white;
		for (int i = 0; i < Vertex.Length; i++) {
			Gizmos.DrawSphere(Vertex[i], 0.1f);
		}
	}

    void DrawCallback(int index,int tIndex)
    {
        Triangles[tIndex] = index;
        Triangles[tIndex + 3] = Triangles[tIndex + 2] = index + 1;
        Triangles[tIndex + 4] = Triangles[tIndex + 1] = (int)(index + Size.x);
        Triangles[tIndex + 5] = (int)(index + Size.x + 1);
    }

}
