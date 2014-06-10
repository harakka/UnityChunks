using UnityEngine;
using System.Collections;

public class ChunkManager : MonoBehaviour {

	public GameObject ChunkPrefab;
	public Vector3 ChunkSize = new Vector3(16,10,16);
	public int WorldSize = 20;

	public Transform Player;
	//public readonly Transform WorldCenter;


	void Awake () {
	}

	void Start() {
		Player.transform.position = new Vector3(ChunkSize.x*WorldSize/2, ChunkSize.y * 2, ChunkSize.z*WorldSize/2);

		for (int x = 0; x < WorldSize; x++) {
			for (int y = 0; y < WorldSize; y++) {
				GameObject prefab = Instantiate(ChunkPrefab, new Vector3(ChunkSize.x*x, 0, ChunkSize.z*y), Quaternion.identity) as GameObject;
				prefab.transform.parent = transform;
				var chunk = prefab.GetComponent<ChunkScript>();
				chunk.Init(ChunkSize, new Vector2(ChunkSize.x*x,ChunkSize.z*y));
				//chunk.Generate();
			}
		}
	}
	
	void Update () {
		Vector2 v1 = new Vector2(Player.position.x, Player.position.z);
		foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
			Vector2 v2 = new Vector2(r.bounds.center.x, r.bounds.center.z);
			if (Vector2.Distance(v1, v2) > ChunkSize.x*2) {
				r.enabled = false;
			} else {
				r.enabled = true;
			}
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
