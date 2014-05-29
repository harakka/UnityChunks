using UnityEngine;
using System.Collections.Generic;


/* Written using http://wiki.unity3d.com/index.php/ProceduralPrimitives and
 * http://studentgamedev.blogspot.fi/2013/08/unity-voxel-tutorial-part-1-generating.html
 * as references. */

public class ChunkScript : MonoBehaviour {

	//public ChunkManager Parent;

	public bool Dirty = true;

	Vector3 Size;
	Vector2 Position;

	List<Vector3> NewVertices = new List<Vector3>();
    List<Vector2> NewUv = new List<Vector2>();
    List<int> NewTriangles = new List<int>();
	
	Mesh mesh;
	float tUnit = 0.125f;
	Vector2 tGrass = new Vector2(0,7);
	Vector2 tMud = new Vector2(1,7);
	int squareCount;
	MeshCollider col;
	byte[,,] blocks;

	public void Init(Vector3 size, Vector2 position) {
		Debug.Log("Chunk " + Position + " initializing");
		Size = size;
		Position = position;
		blocks = new byte[(int)Size.x,(int)Size.y,(int)Size.z];
		GenerateTerrainData();

		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		col = GetComponent<MeshCollider>();
	}

	public void Generate() {
		//Debug.Log("Chunk: generating " + Size + " blocks at " + Position);
		//GenerateTerrainData();
		BuildMesh();
		UpdateMesh();
	}

	void GenerateTerrainData() {
		Debug.Log("Chunk " + Position + " generating blocks");
		for (int x = 0; x < blocks.GetLength(0); x++) {
			for (int z = 0; z < blocks.GetLength(2); z++) {
				var noise = Mathf.FloorToInt(Mathf.PerlinNoise((float)((x+Position.x)/Size.x), (float)(z+Position.y)/Size.z)*Size.y);
				for (int y = 0; y < blocks.GetLength(1); y++) {
					if (y > noise) blocks[x,y,z] = 0;
					if (y == noise) blocks[x,y,z] = 1;
					if (y < noise) blocks[x,y,z] = 2;
				}
			}
		}
	}

	void UpdateMesh () {
		mesh.Clear ();
		mesh.vertices = NewVertices.ToArray();
		mesh.triangles = NewTriangles.ToArray();
		mesh.uv = NewUv.ToArray();
		mesh.Optimize ();
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds();
		
		col.sharedMesh = mesh;
		
		squareCount = 0;
		NewVertices.Clear();
		NewTriangles.Clear();
		NewUv.Clear();
	}

	void GenBlock(int x, int y, int z, Vector2 texture1, Vector2 texture2) {
		// Cube's corner coordinates
		Vector3 point0 = new Vector3 (x  , y  , z+1 );
		Vector3 point1 = new Vector3 (x + 1 , y  , z+1 );
		Vector3 point2 = new Vector3 (x + 1 , y , z );
		Vector3 point3 = new Vector3 (x  , y , z );

		Vector3 point4 = new Vector3 (x  , y-1  , z+1 );
		Vector3 point5 = new Vector3 (x + 1 , y-1  , z+1 );
		Vector3 point6 = new Vector3 (x + 1 , y-1 , z );
		Vector3 point7 = new Vector3 (x  , y-1 , z );

		NewVertices.AddRange(new Vector3[] {
			point0, point1, point2, point3,
			point0, point3, point7, point4,
			point3, point2, point6, point7,
			point2, point1, point5, point6,
			point1, point0, point4, point5,
			point7, point6, point5, point4});

		// UV region coordinates
		Vector2 T1uv00 = new Vector2 (tUnit * texture1.x, tUnit * texture1.y);
		Vector2 T1uv10 = new Vector2 (tUnit * texture1.x + tUnit, tUnit * texture1.y);
		Vector2 T1uv01 = new Vector2 (tUnit * texture1.x, tUnit * texture1.y + tUnit);
		Vector2 T1uv11 = new Vector2 (tUnit * texture1.x + tUnit, tUnit * texture1.y + tUnit);

		Vector2 T2uv00 = new Vector2 (tUnit * texture2.x, tUnit * texture2.y);
		Vector2 T2uv10 = new Vector2 (tUnit * texture2.x + tUnit, tUnit * texture2.y);
		Vector2 T2uv01 = new Vector2 (tUnit * texture2.x, tUnit * texture2.y + tUnit);
		Vector2 T2uv11 = new Vector2 (tUnit * texture2.x + tUnit, tUnit * texture2.y + tUnit);


		NewUv.AddRange(new Vector2[] {
			T2uv01, T2uv11, T2uv10, T2uv00,
			T1uv01, T1uv11, T1uv10, T1uv00,
			T1uv01, T1uv11, T1uv10, T1uv00,
			T1uv01, T1uv11, T1uv10, T1uv00,
			T1uv01, T1uv11, T1uv10, T1uv00,
			T1uv01, T1uv11, T1uv10, T1uv00
		});

		// Explanation: squareCount = number of currently existing "squares" (actually blocks)
		// *24 = there's 24 vertices per block
		// So squareCount*24 = vertices of current block
		// The 4*1, 4*2 etc gives us the vertices of that particular face.
		// Top = 0, Left = 1, Front = 2, etc.

		NewTriangles.AddRange(new int[]{
			// Top
			squareCount*24+0, squareCount*24+1, squareCount*24+2,
			squareCount*24+0, squareCount*24+2, squareCount*24+3,			
			
			// Left
			squareCount*24+0 + 4 * 1, squareCount*24+1 + 4 * 1, squareCount*24+2 + 4 * 1,
			squareCount*24+0 + 4 * 1, squareCount*24+2 + 4 * 1, squareCount*24+3 + 4 * 1,
			
			// Front
			squareCount*24+0 + 4 * 2, squareCount*24+1 + 4 * 2, squareCount*24+2 + 4 * 2,
			squareCount*24+0 + 4 * 2, squareCount*24+2 + 4 * 2, squareCount*24+3 + 4 * 2,
			
			// Right
			squareCount*24+0 + 4 * 3, squareCount*24+1 + 4 * 3, squareCount*24+2 + 4 * 3,
			squareCount*24+0 + 4 * 3, squareCount*24+2 + 4 * 3, squareCount*24+3 + 4 * 3,
			
			// Back
			squareCount*24+0 + 4 * 4, squareCount*24+1 + 4 * 4, squareCount*24+2 + 4 * 4,
			squareCount*24+0 + 4 * 4, squareCount*24+2 + 4 * 4, squareCount*24+3 + 4 * 4,
			
			// Bottom
			squareCount*24+0 + 4 * 5, squareCount*24+1 + 4 * 5, squareCount*24+2 + 4 * 5,
			squareCount*24+0 + 4 * 5, squareCount*24+2 + 4 * 5, squareCount*24+3 + 4 * 5,
		});

		squareCount++;
	}
	

	void BuildMesh(){
		Debug.Log("Chunk " + Position + " generating blocks");
		for (int x = 0; x < blocks.GetLength(0); x++) {
			for (int z = 0; z < blocks.GetLength(2); z++) {
				for (int y = 0; y < blocks.GetLength(1); y++) {
					if (blocks[x,y,z] == 1) GenBlock (x, y, z, tMud, tGrass);
					if (blocks[x,y,z] == 2) GenBlock (x, y, z, tMud, tMud);
				}
			}
		}
	}
		
	// Update is called once per frame
	void Update () {
		if (renderer.enabled && Dirty) {
			Debug.Log("Chunk " + Position + " dirty, generating mesh");
			Generate();
			Dirty = false;
		}
	}
}
