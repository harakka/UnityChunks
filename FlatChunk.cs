using UnityEngine;
using System.Collections.Generic;


/* Written using http://wiki.unity3d.com/index.php/ProceduralPrimitives and
 * http://studentgamedev.blogspot.fi/2013/08/unity-voxel-tutorial-part-1-generating.html
 * as references. */
using System;

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

	int YAt(float x, float y) {
		return YAt (Mathf.RoundToInt(x), Mathf.RoundToInt(y));
	}

	int YAt(int x, int z) {
		try {
			return blocks[x,z];
		} catch (IndexOutOfRangeException e) {
			return 0;
		}
	}

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
		mesh.colors32 = Colors.ToArray();

		mesh.Optimize ();
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds();
		
		col.sharedMesh = mesh;
		
		quadCount = 0;
		NewVertices.Clear();
		NewTriangles.Clear();
		NewUv.Clear();
		Colors.Clear();
	}

	void GenFace(int x, int z) {
		var tex = tGrass;
		Vector3[] face1, face2;

		// Which way the quad gets split depends on elevations of different vertices
		if (YAt (x+1, z+1) == YAt (x, z)) {
			face1 = new Vector3[] {
				new Vector3 (x, YAt(x, z), z),			// 0
				new Vector3 (x, YAt(x, z+1), z+1),		// 1
				new Vector3 (x+1, YAt(x+1, z+1), z+1)};	// 2
			face2 = new Vector3[] {
				new Vector3 (x, YAt(x, z), z),			// 3
				new Vector3 (x+1, YAt(x+1, z+1), z+1),	// 4
				new Vector3 (x+1, YAt(x+1, z), z)};		// 5
		} else {
			face1 = new Vector3[] {
				new Vector3 (x, YAt(x, z+1), z+1),		// 0
				new Vector3 (x+1, YAt(x+1, z+1), z+1 ),	// 1
				new Vector3 (x+1, YAt(x+1, z), z)};		// 2
			face2 = new Vector3[] {
				new Vector3 (x, YAt(x, z+1), z+1),		// 3
				new Vector3 (x+1, YAt(x+1, z), z),		// 4
				new Vector3 (x, YAt(x, z), z)};			// 5
		}

		NewVertices.AddRange(face1);
		NewVertices.AddRange(face2);

		Vector3[][] faces = new Vector3[2][];
		faces[0] = face1;
		faces[1] = face2;

		foreach (Vector3[] face in faces) {
			// If a face is not inclined, make it bright
			if (face[0].y == face[1].y && face[0].y == face[2].y) {
				foreach (Vector3 v in face) {
					Colors.Add(Color.grey*1.5f);
				}
			// Otherwise brighten or darken depending on neigboring vertices' elevation
			} else {
				foreach (Vector3 v in face) {
					int neighboringElevatedCount = 0;
					
					// For each neighbor vertex higher than us, we increase the counter
					if (YAt(v.x+1, v.z) > YAt(v.x, v.z))
						neighboringElevatedCount+=1;
					if (YAt(v.x-1, v.z) > YAt(v.x, v.z))
						neighboringElevatedCount+=1;
					if (YAt(v.x, v.z+1) > YAt(v.x, v.z))
						neighboringElevatedCount+=1;
					if (YAt(v.x, v.z-1) > YAt(v.x, v.z))
						neighboringElevatedCount+=1;
					
					// And for each neighboring lower vertex, we decrease it, to balance things out
					if (YAt(v.x+1, v.z) < YAt(v.x, v.z))
						neighboringElevatedCount-=1;
					if (YAt(v.x-1, v.z) < YAt(v.x, v.z))
						neighboringElevatedCount-=1;
					if (YAt(v.x, v.z+1) < YAt(v.x, v.z))
						neighboringElevatedCount-=1;
					if (YAt(v.x, v.z-1) < YAt(v.x, v.z))
						neighboringElevatedCount-=1;
					
					Colors.Add(Color32.Lerp(Color.grey*0.5f, Color.grey*1.5f, (4-neighboringElevatedCount)/4));
				}
			}
		}

		// Add faces for the vertices we just added
		NewTriangles.AddRange(new int[]{
			quadCount*6+0, quadCount*6+1, quadCount*6+2,
			quadCount*6+3, quadCount*6+4, quadCount*6+5});
		
		// UV region coordinates
		NewUv.AddRange(new Vector2[] {
			new Vector2 (tUnit * tex.x, tUnit * tex.y + tUnit),
			new Vector2 (tUnit * tex.x + tUnit, tUnit * tex.y + tUnit),
			new Vector2 (tUnit * tex.x + tUnit, tUnit * tex.y),
			new Vector2 (tUnit * tex.x, tUnit * tex.y + tUnit),
			new Vector2 (tUnit * tex.x + tUnit, tUnit * tex.y),
			new Vector2 (tUnit * tex.x, tUnit * tex.y)});


		// AO vertex colors

		quadCount++;

	}

	void BuildMesh(){
		//Debug.Log("Chunk " + Position + " generating blocks");
		for (int x = 0; x <= blocks.GetLength(0)-1; x++) {
			for (int z = 0; z <= blocks.GetLength(1)-1; z++) {
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
