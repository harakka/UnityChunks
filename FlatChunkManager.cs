using UnityEngine;
using System.Collections.Generic;

public class FlatChunkManager : MonoBehaviour {

	public GameObject ChunkPrefab;
	//public Vector3 ChunkSize = new Vector3(16,10,16);
	public Coord ChunkSize = new Coord(100,100);
	public int WorldSize = 5;
	//public bool Dirty = false;

	public Transform Player;
	//public readonly Transform WorldCenter;

	Dictionary<Coord, FlatChunk> chunks = new Dictionary<Coord, FlatChunk>();

	public int YAt(Coord target) {

		if (target.X < 0 || target.Z < 0 || target.X >= WorldSize*100 || target.Z >= WorldSize*100) {
			return 250;
		}

		/*int chunkX = target.X / ChunkSize.X;
		int chunkZ = target.Z / ChunkSize.Z;*/
		int chunkX = target.X - target.X % ChunkSize.X;
		int chunkZ = target.Z - target.Z % ChunkSize.Z;
		int blockX = target.X - chunkX;
		int blockZ = target.Z - chunkZ;

		FlatChunk chunk;
		if (chunks.TryGetValue (new Coord (chunkX, chunkZ), out chunk)) {
			return chunk.YAt (blockX, blockZ);
		}
		//Debug.LogError ("Coordinate lookup failure " + target);
		return 0;
	}

	void Awake () {
		for (int x = 0; x < WorldSize; x++) {
			for (int z = 0; z < WorldSize; z++) {
				GameObject prefab = Instantiate(ChunkPrefab, new Vector3(ChunkSize.X*x, 0, ChunkSize.Z*z), Quaternion.identity) as GameObject;
				prefab.transform.parent = transform;
				var chunk = prefab.GetComponent<FlatChunk>();
				var chunkPos = new Coord(ChunkSize.X*x, ChunkSize.Z*z);
				chunks.Add(chunkPos, chunk);
				chunk.Init(ChunkSize, chunkPos);
				//chunk.Generate();
			}
		}
	}

	void Start() {
		Player.transform.position = new Vector3(ChunkSize.X*WorldSize/2, 15, ChunkSize.Z*WorldSize/2);
	}
	
	void Update () {
		Vector2 v1 = new Vector2(Player.position.x, Player.position.z);
		foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
			Vector2 v2 = new Vector2(r.bounds.center.x, r.bounds.center.z);
			if (Vector2.Distance(v1, v2) > ChunkSize.X*20) {
				r.enabled = false;
			} else {
				r.enabled = true;
			}

			/*
			if (Dirty) {
				foreach(FlatChunk s in GetComponentsInChildren<FlatChunk>()) {
					s.Dirty = true;
				}
				Dirty = false;
			}
			*/
		}


		//UpdateBounds();
	}

	void UpdateBounds() {
		Bounds bounds = renderer.bounds;
		foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
			renderer.bounds.Encapsulate(r.bounds);
		}
		//WorldCenter.position = bounds.center;
	}
}
