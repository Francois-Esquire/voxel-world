using System.Collections;
using System.Collections.Generic;
using Realtime.Messaging.Internal;
using UnityEngine;

public class World : MonoBehaviour {

    public GameObject player;
	public Material textureAtlas;

    public static ConcurrentDictionary<string, Chunk> chunks;

    public static List<string> garbage = new List<string>();

    CoroutineQueue queue;

    public static uint maxRoutines = 1000;

	public static int columnHeight = 16;
	public static int chunkSize = 16;
	public static int worldSize = 1;
    public static int radius = 4;

    Vector3 lastBuildPos;

    IEnumerator BuildRecursiveWorld(int x, int y, int z, int rad) {
        rad--;
        if (rad <= 0) yield break;

        //build chunk front
        BuildChunk(x, y, z + 1, rad);

        //build chunk back
        BuildChunk(x, y, z - 1, rad);

        //build chunk left
        BuildChunk(x + 1, y, z, rad);

        //build chunk right
        BuildChunk(x - 1, y, z, rad);

        //build chunk top
        BuildChunk(x, y + 1, z, rad);

        //build chunk bottom
        BuildChunk(x, y - 1, z, rad);

        yield return null;
    }

    IEnumerator DrawChunks() {
        foreach (KeyValuePair<string, Chunk> c in chunks) {
            Chunk _chunk = c.Value;

            if (_chunk.status == Chunk.ChunkStatus.DRAW) _chunk.DrawChunk();

            if (_chunk.chunk && Vector3.Distance(player.transform.position, _chunk.chunk.transform.position) > radius * chunkSize)
                garbage.Add(c.Key);

            yield return null;
        }
    }

    IEnumerator Dump() {
        for (int i = 0; i < garbage.Count; i++) {
            string n = garbage[i];

            Chunk c;

            if (chunks.TryGetValue(n, out c)) {
                Destroy(c.chunk);

                chunks.TryRemove(n, out c);

                garbage.Remove(n);

                yield return null;
            }
        }
    }

    void Flush() {
        queue.Run(Dump());
    }

    void DrawWorld() {
        queue.Run(DrawChunks());
    }

    void BuildWorld() {
        queue.Run(BuildRecursiveWorld(
            (int)(player.transform.position.x / chunkSize),
            (int)(player.transform.position.y / chunkSize),
            (int)(player.transform.position.z / chunkSize),
            radius
        ));
    }

    void BuildChunk(int x, int y, int z, int rad) {
        BuildChunkAt(x, y, z);

        queue.Run(BuildRecursiveWorld(x, y, z, rad));
    }

    void BuildChunkAt(int x, int y, int z) {
        Vector3 chunkPosition = new Vector3(
                        x * chunkSize,
                        y * chunkSize,
                        z * chunkSize
                    );

        string n = BuildChunkName(chunkPosition);

        Chunk c;

        if (!chunks.TryGetValue(n, out c))
        {
            c = new Chunk(chunkPosition, textureAtlas, n);

            c.chunk.transform.parent = this.transform;

            chunks.TryAdd(c.chunk.name, c);
        }
    }

    public static string BuildChunkName(Vector3 v) {
        return (int)v.x + "_" + (int)v.y + "_" + (int)v.z;
    }

    void Start() {
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        queue = new CoroutineQueue(maxRoutines, StartCoroutine);

        chunks = new ConcurrentDictionary<string, Chunk>();

        // set player as inactive
        player.SetActive(false);

        Vector3 ppos = player.transform.position;

        // set positions
        lastBuildPos = player.transform.position = new Vector3(
            ppos.x,
            Utils.GenerateHeight(ppos.x, ppos.z) + 1,
            ppos.z
        );

        // genesis chunk
        BuildChunkAt(
            (int)(player.transform.position.x/chunkSize),
            (int)(player.transform.position.y/chunkSize),
            (int)(player.transform.position.z/chunkSize)
        );

        DrawWorld();

        BuildWorld();
    }

    void Update() {
        Vector3 movement = lastBuildPos - player.transform.position;

        if (movement.magnitude > chunkSize) {
            lastBuildPos = player.transform.position;

            StopCoroutine("BuildRecursiveWorld");

            BuildWorld();
        }

        if (player.activeSelf == false) player.SetActive(true);

        DrawWorld();
        Flush();
    }
}
