using UnityEngine;
using System.Collections.Generic;


/* Written using http://wiki.unity3d.com/index.php/ProceduralPrimitives and
 * http://studentgamedev.blogspot.fi/2013/08/unity-voxel-tutorial-part-1-generating.html
 * as references. */

public class FlatChunk : MonoBehaviour {

	public bool Dirty = true;

	Vector3 Size;
	Vector2 Position;

	List<Vector3> NewVertices = new List<Vector3>();
    List<Vector2> NewUv = new List<Vector2>();
    List<int> NewTriangles = new List<int>();

	List<Color32> Colors = new List<Color32>();
	
	Mesh mesh;
	float tUnit = 0.125f;
	Vector2 tGrass = new Vector2(0,7);
	Vector2 tMud = new Vector2(1,7);
	int quadCount;
	MeshCollider col;
	byte[,] blocks;
	PerlinNoiseGen perlin;

	public void Init(Vector3 size, Vector2 position) {
		//Debug.Log("Chunk " + Position + " initializing");
		Size = size;
		Position = position;
		blocks = new byte[(int)Size.x,(int)Size.z];
		GenerateTerrainData();

		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		col = GetComponent<MeshCollider>();
	}

	public void Generate() {
		BuildMesh();
		UpdateMesh();
	}

	void GenerateTerrainData() {
		perlin = GetComponentInParent<PerlinNoiseGen>();
		//Debug.Log("Chunk " + Position + " generating blocks");
		for (int x = 0; x < blocks.GetLength(0); x++) {
			for (int z = 0; z < blocks.GetLength(1); z++) {
				var noise = Mathf.FloorToInt(perlin.PerlinNoise((x+Position.x)/Size.x, (z+Position.y)/Size.z)*Size.y);
				blocks[x,z] = (byte)noise;		//FIXME: ugly cast
			}
		}
	}

	void UpdateMesh () {
		mesh.Clear ();

		mesh.vertices = NewVertices.ToArray();

		mesh.triangles = NewTriangles.ToArray();
		mesh.uv = NewUv.ToArray();
		//mesh.colors32 = Colors.ToArray();

		mesh.Optimize ();
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds();
		
		col.sharedMesh = mesh;
		
		quadCount = 0;
		NewVertices.Clear();
		NewTriangles.Clear();
		NewUv.Clear();
		//Colors.Clear();
	}

	void GenFace(int x, int z) {
		var tex = tGrass;

		NewVertices.AddRange(new Vector3[] {
			new Vector3 (x, blocks[x, z+1], z+1 ),
			new Vector3 (x+1, blocks[x+1, z+1], z+1 ),
			new Vector3 (x+1, blocks[x+1, z], z ),
			new Vector3 (x, blocks[x, z], z )});

		// Add faces for the vertices we just added
		NewTriangles.AddRange(new int[]{
			quadCount*4+0, quadCount*4+1, quadCount*4+2,
			quadCount*4+0, quadCount*4+2, quadCount*4+3});
		
		// UV region coordinates
		NewUv.AddRange(new Vector2[] {
			new Vector2 (tUnit * tex.x, tUnit * tex.y + tUnit),
			new Vector2 (tUnit * tex.x + tUnit, tUnit * tex.y + tUnit),
			new Vector2 (tUnit * tex.x + tUnit, tUnit * tex.y),
			new Vector2 (tUnit * tex.x, tUnit * tex.y)});
		
		quadCount++;

	}

	void BuildMesh(){
		//Debug.Log("Chunk " + Position + " generating blocks");
		for (int x = 0; x < blocks.GetLength(0)-1; x++) {
			for (int z = 0; z < blocks.GetLength(1)-1; z++) {
				GenFace(x, z);
			}
		}
	}
		
	// Update is called once per frame
	void Update () {
		if (renderer.enabled && Dirty) {
			//Debug.Log("Chunk " + Position + " dirty, generating mesh");
			Generate();
			Dirty = false;
		}
	}
}
