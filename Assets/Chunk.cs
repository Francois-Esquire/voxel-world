using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[Serializable]
class BlockData {
    public Block.BlockType[,,] matrix;

    public BlockData() {}

    public BlockData(Block[,,] b) {
        matrix = new Block.BlockType[World.chunkSize, World.chunkSize, World.chunkSize];

        for (int z = 0; z < World.chunkSize; z++)
            for (int y = 0; y < World.chunkSize; y++)
                for (int x = 0; x < World.chunkSize; x++) matrix[x, y, z] = b[x, y, z].element;
    }
}

public class Chunk {

    public GameObject chunk;
    public Block[,,] chunkData;
    public Material cubeMaterial;

    public enum ChunkStatus { DONE, DRAW, KEEP };
    public ChunkStatus status;

    BlockData bd;

    string BuildChunkFileName(Vector3 v) {
        return Application.persistentDataPath +
                          "/savedata/Chunk_" +
                          (int)v.x + "_" +
                          (int)v.y + "_" +
                          (int)v.z + "_" +
                          World.chunkSize + "_" +
                          World.radius + "_" +
                          ".dat";
    }

    bool Load() {
        string chunkFile = BuildChunkFileName(chunk.transform.position);

        if (File.Exists(chunkFile)) {
            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Open(chunkFile, FileMode.Open);

            bd = new BlockData();

            bd = (BlockData)bf.Deserialize(file);

            file.Close();

            return true;
        }

        return false;
    }

    public void Save() {
        string chunkFile = BuildChunkFileName(chunk.transform.position);

        if (File.Exists(chunkFile) == false)
            Directory.CreateDirectory(Path.GetDirectoryName(chunkFile));
        
        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = File.Open(chunkFile, FileMode.OpenOrCreate);

        bd = new BlockData(chunkData);

        bf.Serialize(file, bd);

        file.Close();
    }

    public void Redraw() {
        GameObject.DestroyImmediate(chunk.GetComponent<MeshFilter>());
        GameObject.DestroyImmediate(chunk.GetComponent<MeshRenderer>());
        GameObject.DestroyImmediate(chunk.GetComponent<Collider>());

        DrawChunk();
    }

    public Chunk(string name, Vector3 position, Material mat) {
        chunk = new GameObject(name);

        chunk.transform.position = position;

        cubeMaterial = mat;

        BuildChunk();
    }

    void BuildChunk() {
        // check if data for this block exists
        bool dataFromFile = false;
        dataFromFile = Load();

  		chunkData = new Block[World.chunkSize, World.chunkSize, World.chunkSize];

  		for (int z = 0; z < World.chunkSize; z++)
  			for (int y = 0; y < World.chunkSize; y++)
  				for (int x = 0; x < World.chunkSize; x++) {
                    Vector3 pos = new Vector3(x, y, z);

                    if (dataFromFile) {
                        chunkData[x, y, z] = new Block(
                            bd.matrix[x, y, z],
                            pos,
                            this
                        );

                        continue;
                    }

                    int worldX = (int)(x + chunk.transform.position.x);
                    int worldY = (int)(y + chunk.transform.position.y);
                    int worldZ = (int)(z + chunk.transform.position.z);

                    Block.BlockType type;

                    if (Utils.FBM3D(worldX, worldY, worldZ, 0.1f, 3) < 0.285f) type = Block.BlockType.AIR;
                    else if (worldY <= Utils.GenerateHeight(worldX, worldZ, 25, 1)) {
                        if (Utils.FBM3D(worldX, worldY, worldZ, 0.01f, 2) < 0.23f) type = Block.BlockType.DIAMOND;
                        else type = Block.BlockType.STONE;
                    }
                    else if (worldY == Utils.GenerateHeight(worldX, worldZ)) type = Block.BlockType.GRASS;
                    else if (worldY < Utils.GenerateHeight(worldX, worldZ)) type = Block.BlockType.DIRT;
                    else type = Block.BlockType.AIR;

                    chunkData[x, y, z] = new Block(
                        type,
                        pos,
                        this
                    );

                    status = ChunkStatus.DRAW;
                }
  	}

    public void DrawChunk() {
        for (int z = 0; z < World.chunkSize; z++)
            for (int y = 0; y < World.chunkSize; y++)
                for (int x = 0; x < World.chunkSize; x++) chunkData[x, y, z].Draw();

        CombineQuads();

        MeshCollider collider = chunk.gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;

        collider.sharedMesh = chunk.transform.GetComponent<MeshFilter>().mesh;

        status = ChunkStatus.DONE;
    }

    void CombineQuads() {
        // 1. Combine all children meshes
        MeshFilter[] meshFilters = chunk.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        // 2. Create a new mesh on the parent object
        MeshFilter mf = (MeshFilter) chunk.gameObject.AddComponent(typeof(MeshFilter));
        mf.mesh = new Mesh();

        // 3. Add combined meshes on children as the parents mesh
        mf.mesh.CombineMeshes(combine);

        // 4. Create a renderer for the parent
        MeshRenderer renderer = chunk.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = cubeMaterial;

        // 5. Delete all uncombined children
        foreach (Transform quad in chunk.transform) GameObject.Destroy(quad.gameObject);
    }
}
