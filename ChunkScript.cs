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

	List<Color32> Colors = new List<Color32>();
	
	Mesh mesh;
	float tUnit = 0.125f;
	Vector2 tGrass = new Vector2(0,7);
	Vector2 tMud = new Vector2(1,7);
	int quadCount;
	MeshCollider col;
	byte[,,] blocks;

	enum Neighbor {
		FrontNW, FrontN, FrontNE, FrontE, FrontSE, FrontS, FrontSW, FrontW, FrontCenter,
		MidNW, MidN, MidNE, MidE, MidSE, MidS, MidSW, MidW, MidCenter,
		BackNW, BackN, BackNE, BackE, BackSE, BackS, BackSW, BackW, BackCenter
	}

	bool IsNeighborSolid(int x, int y, int z, Neighbor n) {
		if (NeighborBlockType(x, y, z, n) == 0) return false;
		else return true;
	}

	byte NeighborBlockType(int x, int y, int z, Neighbor n) {
		int xOff = 0, yOff = 0, zOff = 0;
		switch (n) {
		case Neighbor.FrontNW:
			xOff = -1;
			yOff = 1;
			zOff = -1;
			break;

		case Neighbor.FrontN:
			xOff = 0;
			yOff = 1;
			zOff = -1;
			break;

		case Neighbor.FrontNE:
			xOff = 1;
			yOff = 1;
			zOff = -1;
			break;

		case Neighbor.FrontE:
			xOff = 1;
			yOff = 0;
			zOff = -1;
			break;

		case Neighbor.FrontSE:
			xOff = 1;
			yOff = -1;
			zOff = -1;
			break;

		case Neighbor.FrontS:
			xOff = 0;
			yOff = -1;
			zOff = -1;
			break;

		case Neighbor.FrontSW:
			xOff = -1;
			yOff = -1;
			zOff = -1;
			break;

		case Neighbor.FrontW:
			xOff = -1;
			yOff = 0;
			zOff = -1;
			break;

		case Neighbor.FrontCenter:
			xOff = 0;
			yOff = 0;
			zOff = -1;
			break;

		case Neighbor.MidNW:
			xOff = -1;
			yOff = 1;
			zOff = 0;
			break;

		case Neighbor.MidN:
			xOff = 0;
			yOff = 1;
			zOff = 0;
			break;

		case Neighbor.MidNE:
			xOff = 1;
			yOff = 1;
			zOff = 0;
			break;

		case Neighbor.MidE:
			xOff = 1;
			yOff = 0;
			zOff = 0;
			break;

		case Neighbor.MidSE:
			xOff = 1;
			yOff = -1;
			zOff = 0;
			break;

		case Neighbor.MidS:
			xOff = 0;
			yOff = -1;
			zOff = 0;
			break;

		case Neighbor.MidSW:
			xOff = -1;
			yOff = -1;
			zOff = 0;
			break;

		case Neighbor.MidW:
			xOff = -1;
			yOff = 0;
			zOff = 0;
			break;

		case Neighbor.MidCenter:
			xOff = 0;
			yOff = 0;
			zOff = 0;
			break;

		case Neighbor.BackNW:
			xOff = -1;
			yOff = 1;
			zOff = 1;
			break;

		case Neighbor.BackN:
			xOff = 0;
			yOff = 1;
			zOff = 1;
			break;

		case Neighbor.BackNE:
			xOff = 1;
			yOff = 1;
			zOff = 1;
			break;

		case Neighbor.BackE:
			xOff = 1;
			yOff = 0;
			zOff = 1;
			break;

		case Neighbor.BackSE:
			xOff = 1;
			yOff = -1;
			zOff = 1;
			break;

		case Neighbor.BackS:
			xOff = 0;
			yOff = -1;
			zOff = 1;
			break;

		case Neighbor.BackSW:
			xOff = -1;
			yOff = -1;
			zOff = 1;
			break;

		case Neighbor.BackW:
			xOff = -1;
			yOff = 0;
			zOff = 1;
			break;

		case Neighbor.BackCenter:
			xOff = 0;
			yOff = 0;
			zOff = 1;
			break;
		default:
			Debug.Log("The famous this should never happen line");
			break;
		}

		return BlockAt(x+xOff, y+yOff, z+zOff);
	}

	byte BlockAt(int x, int y, int z) {
		if (x < 0 || x >= Size.x || y < 0 || y >= Size.y || z < 0 || z >= Size.z) {
			//Debug.Log("DEBUG: nonexistent block access: " + x + "x, " + y + "y, " + z + "z @ " + Position);
			return 0;
		} else {
			return blocks[x,y,z];
		}
	}

	public void Init(Vector3 size, Vector2 position) {
		//Debug.Log("Chunk " + Position + " initializing");
		Size = size;
		Position = position;
		blocks = new byte[(int)Size.x,(int)Size.y,(int)Size.z];
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
		//Debug.Log("Chunk " + Position + " generating blocks");
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

		/*int i = 0;
		while (i < NewVertices.Count) {
			colors[i] = Color32.Lerp(Color.black, Color.white, NewVertices[i].y/Size.y);
			i++;
		}
		Debug.Log(i + " vertices colored");*/

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

		// TODO: move this to right place
		//Color32[] colors = new Color32[NewVertices.Count];
	}

	// AO method from http://0fps.net/2013/07/03/ambient-occlusion-for-minecraft-like-worlds/
	// TODO: handle chunk borders
	void VertexAO(bool side1, bool side2, bool corner) {
		int s1, s2, c;
		s1 = (side1) ? 1: 0;
		s2 = (side2) ? 1: 0;
		c = (corner) ? 1: 0;
		Colors.Add(Color32.Lerp(Color.black, Color.white, (3-(s1+s2+c))/3));
	}

	void GenFaceCommon(Vector2 tex) {
		// Add faces for the vertices we just added
		NewTriangles.AddRange(new int[]{
			quadCount*4+0, quadCount*4+1, quadCount*4+2,
			quadCount*4+0, quadCount*4+2, quadCount*4+3});

		// UV region coordinates
		NewUv.AddRange(new Vector2[] {
			new Vector2 (tUnit * tex.x, tUnit * tex.y + tUnit),
			new Vector2 (tUnit * tex.x + tUnit, tUnit * tex.y + tUnit),
			new Vector2 (tUnit * tex.x + tUnit, tUnit * tex.y),
			new Vector2 (tUnit * tex.x, tUnit * tex.y),
		});

		quadCount++;
	}

	void GenFaceTop(Vector3 pos, byte block) {
		NewVertices.AddRange(new Vector3[] {
			new Vector3 (pos.x, pos.y, pos.z+1 ),
			new Vector3 (pos.x + 1 , pos.y  , pos.z+1 ),
			new Vector3 (pos.x + 1 , pos.y , pos.z ),
			new Vector3 (pos.x  , pos.y , pos.z )});
		if (block == 1) {
			GenFaceCommon(tGrass);
		} else {
			GenFaceCommon(tMud);
		}
	}

	void GenFaceLeft(Vector3 pos) {
		NewVertices.AddRange(new Vector3[] {
			new Vector3 (pos.x, pos.y  , pos.z+1 ),
			new Vector3 (pos.x  , pos.y  , pos.z ),
			new Vector3 (pos.x, pos.y-1, pos.z ),
			new Vector3 (pos.x, pos.y-1 , pos.z+1 )});
			GenFaceCommon(tMud);
	}

	void GenFaceFront(Vector3 pos) {
		NewVertices.AddRange(new Vector3[] {
			new Vector3 (pos.x, pos.y, pos.z ),
			new Vector3 (pos.x + 1 , pos.y, pos.z ),
			new Vector3 (pos.x + 1 , pos.y-1 , pos.z ),
			new Vector3 (pos.x, pos.y-1 , pos.z )});
			GenFaceCommon(tMud);
	}

	void GenFaceRight(Vector3 pos) {
		NewVertices.AddRange(new Vector3[] {
			new Vector3 (pos.x + 1 , pos.y , pos.z ),
			new Vector3 (pos.x + 1 , pos.y , pos.z+1 ),
			new Vector3 (pos.x + 1 , pos.y-1, pos.z+1 ),
			new Vector3 (pos.x + 1 , pos.y-1  , pos.z )});
		GenFaceCommon(tMud);
	}

	void GenFaceBack(Vector3 pos) {
		NewVertices.AddRange(new Vector3[] {
			new Vector3 (pos.x+1, pos.y, pos.z+1 ),
			new Vector3 (pos.x, pos.y  , pos.z+1 ),
			new Vector3 (pos.x, pos.y-1  , pos.z+1 ),
			new Vector3 (pos.x+1, pos.y-1, pos.z+1 )});
		GenFaceCommon(tMud);
	}

	void GenFaceBottom(Vector3 pos) {
		NewVertices.AddRange(new Vector3[] {
			new Vector3 (pos.x, pos.y-1 , pos.z ),
			new Vector3 (pos.x + 1 , pos.y-1 , pos.z ),
			new Vector3 (pos.x + 1 , pos.y-1  , pos.z+1 ),
			new Vector3 (pos.x, pos.y-1, pos.z+1 )});
		GenFaceCommon(tMud);
	}
	
	void GenBlock(int x, int y, int z, Vector2 texture1, Vector2 texture2) {
		if (BlockAt(x,y,z) != 0) {
			if (BlockAt(x, y+1, z) == 0) {
				GenFaceTop(new Vector3(x,y,z), BlockAt(x,y,z));

				VertexAO (IsNeighborSolid(x,y,z, Neighbor.MidNW), IsNeighborSolid(x,y,z, Neighbor.BackN), IsNeighborSolid(x,y,z, Neighbor.BackNW));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.BackN), IsNeighborSolid(x,y,z, Neighbor.MidNE), IsNeighborSolid(x,y,z, Neighbor.BackNE));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.MidNE), IsNeighborSolid(x,y,z, Neighbor.FrontN), IsNeighborSolid(x,y,z, Neighbor.FrontNE));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.MidNW), IsNeighborSolid(x,y,z, Neighbor.FrontN), IsNeighborSolid(x,y,z, Neighbor.FrontNW));
			}
			if (BlockAt(x, y-1, z) == 0) {
				GenFaceBottom(new Vector3(x,y,z));

				VertexAO (IsNeighborSolid(x,y,z, Neighbor.MidSE), IsNeighborSolid(x,y,z, Neighbor.BackS), IsNeighborSolid(x,y,z, Neighbor.BackSE));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.BackS), IsNeighborSolid(x,y,z, Neighbor.MidSW), IsNeighborSolid(x,y,z, Neighbor.BackSW));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.FrontS), IsNeighborSolid(x,y,z, Neighbor.MidSW), IsNeighborSolid(x,y,z, Neighbor.FrontSW));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.FrontS), IsNeighborSolid(x,y,z, Neighbor.MidSE), IsNeighborSolid(x,y,z, Neighbor.FrontSE));
			}
			if (BlockAt(x+1, y, z) == 0) {
				GenFaceRight(new Vector3(x,y,z));

				VertexAO (IsNeighborSolid(x,y,z, Neighbor.FrontE), IsNeighborSolid(x,y,z, Neighbor.MidNE), IsNeighborSolid(x,y,z, Neighbor.FrontNE));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.BackE), IsNeighborSolid(x,y,z, Neighbor.MidNE), IsNeighborSolid(x,y,z, Neighbor.BackNE));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.BackE), IsNeighborSolid(x,y,z, Neighbor.MidSE), IsNeighborSolid(x,y,z, Neighbor.BackSE));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.MidSE), IsNeighborSolid(x,y,z, Neighbor.FrontE), IsNeighborSolid(x,y,z, Neighbor.FrontSE));
			}
			if (BlockAt(x-1, y, z) == 0) {
				GenFaceLeft(new Vector3(x,y,z));

				VertexAO (IsNeighborSolid(x,y,z, Neighbor.BackW), IsNeighborSolid(x,y,z, Neighbor.MidNW), IsNeighborSolid(x,y,z, Neighbor.BackNW));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.MidNW), IsNeighborSolid(x,y,z, Neighbor.FrontW), IsNeighborSolid(x,y,z, Neighbor.FrontNW));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.FrontW), IsNeighborSolid(x,y,z, Neighbor.MidSW), IsNeighborSolid(x,y,z, Neighbor.FrontSW));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.MidSW), IsNeighborSolid(x,y,z, Neighbor.BackW), IsNeighborSolid(x,y,z, Neighbor.BackSW));
			}
			if (BlockAt(x, y, z+1) == 0) {
				GenFaceBack(new Vector3(x,y,z));

				VertexAO (IsNeighborSolid(x,y,z, Neighbor.BackE), IsNeighborSolid(x,y,z, Neighbor.BackN), IsNeighborSolid(x,y,z, Neighbor.BackNE));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.BackN), IsNeighborSolid(x,y,z, Neighbor.BackW), IsNeighborSolid(x,y,z, Neighbor.BackNW));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.BackW), IsNeighborSolid(x,y,z, Neighbor.BackS), IsNeighborSolid(x,y,z, Neighbor.BackSW));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.BackS), IsNeighborSolid(x,y,z, Neighbor.BackE), IsNeighborSolid(x,y,z, Neighbor.BackSE));
			}
			if (BlockAt(x, y, z-1) == 0) {
				GenFaceFront(new Vector3(x,y,z));

				VertexAO (IsNeighborSolid(x,y,z, Neighbor.FrontW), IsNeighborSolid(x,y,z, Neighbor.FrontN), IsNeighborSolid(x,y,z, Neighbor.FrontNW));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.FrontN), IsNeighborSolid(x,y,z, Neighbor.FrontE), IsNeighborSolid(x,y,z, Neighbor.FrontNE));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.FrontE), IsNeighborSolid(x,y,z, Neighbor.FrontS), IsNeighborSolid(x,y,z, Neighbor.FrontSE));
				VertexAO (IsNeighborSolid(x,y,z, Neighbor.FrontS), IsNeighborSolid(x,y,z, Neighbor.FrontW), IsNeighborSolid(x,y,z, Neighbor.FrontSW));
			}
		}
	}
	

	void BuildMesh(){
		//Debug.Log("Chunk " + Position + " generating blocks");
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
			//Debug.Log("Chunk " + Position + " dirty, generating mesh");
			Generate();
			Dirty = false;
		}
	}
}
