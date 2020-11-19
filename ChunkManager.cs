using UnityEngine;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour {

	public GameObject ChunkPrefab;
	//public Vector3 ChunkSize = new Vector3(16,10,16);
	public readonly int ChunkSize = 100;

	public int ViewDistance = 2;
	//public int WorldSize = 5;
	//public bool Dirty = false;

	public Transform Player;
	//public readonly Transform WorldCenter;

	Dictionary<Coord, Chunk> chunks = new Dictionary<Coord, Chunk>();

	public int YAt(Coord target) {

		/*if (target.X < 0 || target.Z < 0 || target.X >= WorldSize*100 || target.Z >= WorldSize*100) {
			return 250;
		}*/

		/*int chunkX = target.X / ChunkSize.X;
		int chunkZ = target.Z / ChunkSize.Z;*/
		int chunkX = target.X - target.X % ChunkSize;
		int chunkZ = target.Z - target.Z % ChunkSize;
		int blockX = target.X - chunkX;
		int blockZ = target.Z - chunkZ;

		Chunk chunk;
		if (chunks.TryGetValue (new Coord (chunkX, chunkZ), out chunk)) {
			return chunk.YAt (blockX, blockZ);
		}
		//Debug.LogError ("Coordinate lookup failure " + target);
		return 0;
	}

	void Awake () {
		/*for (int x = 0; x < WorldSize; x++) {
			for (int z = 0; z < WorldSize; z++) {
				GameObject prefab = Instantiate(ChunkPrefab, new Vector3(ChunkSize.X*x, 0, ChunkSize.Z*z), Quaternion.identity) as GameObject;
				prefab.transform.parent = transform;
				var chunk = prefab.GetComponent<FlatChunk>();
				var chunkPos = new Coord(ChunkSize.X*x, ChunkSize.Z*z);
				chunks.Add(chunkPos, chunk);
				chunk.Init(ChunkSize, chunkPos);
				//chunk.Generate();
			}
		}*/
	}

	void Start() {
		Player.transform.position = new Vector3(0, 50, 0);
	}
	
	void Update () {
		// TODO rewrite the code from Awake to generate chunks when they enter player's view distance
		//for (int i = 0 - ViewDistance * ChunkSize; i < ViewDistance*ChunkSize; i = i + ChunkSize) {

		// Iterate all chunks within ViewDistance radius of player (actually lying, it's a square with player near middle)
		// TODO: make the view distance actually use a radius centered on chunk player is on
		int playerX = Mathf.RoundToInt(Player.transform.position.x / ChunkSize)*ChunkSize;
		int playerZ = Mathf.RoundToInt(Player.transform.position.z / ChunkSize)*ChunkSize;
		for (int x = playerX - ViewDistance * ChunkSize; x < ViewDistance * ChunkSize; x = x + ChunkSize) {
			for (int z = playerZ - ViewDistance * ChunkSize; z < ViewDistance * ChunkSize; z = z + ChunkSize) {
				Coord chunkPos = new Coord(x, z);
				if (chunks.ContainsKey(chunkPos)) {
					// This chunk already exists
					// TODO: dirty chunk check
				} else {
					// Chunk doesn't exist, generate it
					GameObject prefab = Instantiate(ChunkPrefab, new Vector3(x, 0, z), Quaternion.identity) as GameObject;
					prefab.transform.parent = transform;
					var chunk = prefab.GetComponent<Chunk>();
					//var chunkPos = new Coord(ChunkSize.X*x, ChunkSize.Z*z);
					chunks.Add(chunkPos, chunk);
					chunk.Init(ChunkSize, chunkPos);
					chunk.GetComponent<Renderer>().enabled = true;

				}
			}
		}
	}


	public void LateUpdate() {
		// TODO handle chunk hiding / showing based on player dist
		/*
		Vector2 v1 = new Vector2(Player.position.x, Player.position.z);
		foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
			Vector2 v2 = new Vector2(r.bounds.center.x, r.bounds.center.z);
			if (Vector2.Distance(v1, v2) > ChunkSize*ViewDistance) {
				r.enabled = false;
			} else {
				r.enabled = true;
			}
		}
		*/

	}
}
