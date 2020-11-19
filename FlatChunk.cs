using UnityEngine;
using System.Collections.Generic;


/* Written using http://wiki.unity3d.com/index.php/ProceduralPrimitives and
 * http://studentgamedev.blogspot.fi/2013/08/unity-voxel-tutorial-part-1-generating.html
 * as references. */

public class FlatChunk : MonoBehaviour {

	public bool Dirty = true;

	//Vector3 Size;
	//Vector2 Position;

	List<Vector3> NewVertices = new List<Vector3>();
    List<Vector2> NewUv = new List<Vector2>();
    List<int> NewTriangles = new List<int>();

	List<Color32> Colors = new List<Color32>();

	FlatChunkManager manager;

	Coord position;
	Coord size;

	Mesh mesh;
	float tUnit = 0.5f;
	Vector2 tGrass = new Vector2(0,1);
	Vector2 tMud = new Vector2(1,1);
	int quadCount;
	MeshCollider col;
	byte[,] blocks;
	PerlinNoiseGen perlin;

	Color32 colorBright = Color.green*1.5f;
	Color32 colorDark = Color.green*0.5f;

	public int YAt(float x, float z) {
		return YAt(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
	}

	public int YAt(int x, int z) {
		try {
			return blocks[x,z];
		} catch (System.IndexOutOfRangeException e) {
			return manager.YAt(new Coord(position.X + x, position.Z + z));
			//return 255;
		}
	}

	public void Init(Coord asize, Coord aposition) {
		//Debug.Log("Chunk " + Position + " initializing");
		manager = GetComponentInParent<FlatChunkManager>();
		size = asize;
		position = aposition;
		blocks = new byte[size.X,size.Z];
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
				var noise = Mathf.FloorToInt(perlin.PerlinNoise((float)(x+position.X)/size.X, (float)(z+position.Z)/size.Z)*size.Z);
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
		Color colorVariance = Color.gray * Random.Range(-0.2f, 0.2f);

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
					Colors.Add(colorBright+colorVariance);
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
					
					Colors.Add(Color32.Lerp(colorDark, colorBright, (4-neighboringElevatedCount)/4)+colorVariance);
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
	void LateUpdate () {
		if (renderer.enabled && Dirty) {
			//Debug.Log("Chunk " + Position + " dirty, generating mesh");
			Generate();
			Dirty = false;
		}
	}
}
