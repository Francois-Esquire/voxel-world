using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {

	public Material cubeMaterial;
	public Block[,,] chunkData;
	public GameObject chunk;

	void BuildChunk() {
		chunkData = new Block[World.chunkSize, World.chunkSize, World.chunkSize];

		for (int z = 0; z < World.chunkSize; z++)
			for (int y = 0; y < World.chunkSize; y++)
				for (int x = 0; x < World.chunkSize; x++) {
					Vector3 pos = new Vector3(x, y, z);

                    Block.BlockType type;

                    int worldX = (int)(x + chunk.transform.position.x);
                    int worldY = (int)(y + chunk.transform.position.y);
                    int worldZ = (int)(z + chunk.transform.position.z);

                    //if (Utils.FBM3D(worldX, worldY, worldZ, 0.1f, 3) < 0.285f) type = Block.BlockType.AIR;
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
						chunk.gameObject,
						this
					);
				}
	}

	public void DrawChunk() {
		for (int z = 0; z < World.chunkSize; z++)
			for (int y = 0; y < World.chunkSize; y++)
				for (int x = 0; x < World.chunkSize; x++) {
					chunkData[x, y, z].Draw();
				}

		CombineQuads();

        MeshCollider collider = chunk.gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
        collider.sharedMesh = chunk.transform.GetComponent<MeshFilter>().mesh;
	}

	public Chunk(Vector3 position, Material c) {
		chunk = new GameObject(World.BuildChunkName(position));
		chunk.transform.position = position;
		cubeMaterial = c;

		BuildChunk();
	}

	void CombineQuads() {
		// Combine all children meshes
		MeshFilter[] meshFilters = chunk.GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];

		int i = 0;
		while (i < meshFilters.Length) {
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			i++;
		}

		// create a new mesh on the parent object
		MeshFilter mf = (MeshFilter) chunk.gameObject.AddComponent(typeof(MeshFilter));

		mf.mesh = new Mesh();

		// add combined meshes on children as the parents mesh.
		mf.mesh.CombineMeshes(combine);

		MeshRenderer renderer = chunk.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

		renderer.material = cubeMaterial;

		foreach (Transform quad in chunk.transform) {
			GameObject.Destroy(quad.gameObject);
		}
	}
}
