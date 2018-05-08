using UnityEngine;

public class Block {
	static string prefix = "ScriptedMesh";

	enum Cubeside { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };

	static Vector3 p0 = new Vector3( -0.5f, -0.5f,  0.5f );
	static Vector3 p1 = new Vector3(  0.5f, -0.5f,  0.5f );
	static Vector3 p2 = new Vector3(  0.5f, -0.5f, -0.5f );
	static Vector3 p3 = new Vector3( -0.5f, -0.5f, -0.5f );
	static Vector3 p4 = new Vector3( -0.5f,  0.5f,  0.5f );
	static Vector3 p5 = new Vector3(  0.5f,  0.5f,  0.5f );
	static Vector3 p6 = new Vector3(  0.5f,  0.5f, -0.5f );
	static Vector3 p7 = new Vector3( -0.5f,  0.5f, -0.5f );

	public enum BlockType { GRASS, DIRT, STONE, BEDROCK, REDSTONE, DIAMOND, AIR };

	static Vector2[,] blockUVs = {
		{ /* GRASS TOP */
		    new Vector2( 0.125f,  0.375f  ), new Vector2( 0.1875f, 0.375f  ),
		    new Vector2( 0.125f,  0.4375f ), new Vector2( 0.1875f, 0.4375f )
		},
		{ /* GRASS SIDE */
		    new Vector2( 0.1875f, 0.9375f ), new Vector2( 0.25f, 0.9375f ),
		    new Vector2( 0.1875f, 1.0f    ), new Vector2( 0.25f, 1.0f    )
		},
		{ /* DIRT */
		    new Vector2( 0.125f, 0.9375f ), new Vector2( 0.1875f, 0.9375f ),
		    new Vector2( 0.125f, 1.0f    ), new Vector2( 0.1875f, 1.0f    )
		},
		{ /* STONE */
		    new Vector2( 0, 0.875f  ), new Vector2( 0.0625f, 0.875f  ),
		    new Vector2( 0, 0.9375f ), new Vector2( 0.0625f, 0.9375f )
		},
		{ /*BEDROCK*/
				new Vector2( 0.3125f, 0.8125f ), new Vector2( 0.375f, 0.8125f),
				new Vector2( 0.3125f, 0.875f  ), new Vector2( 0.375f, 0.875f )
		},
		{ /*REDSTONE*/
				new Vector2( 0.1875f, 0.75f   ), new Vector2( 0.25f, 0.75f   ),
				new Vector2( 0.1875f, 0.8125f ), new Vector2( 0.25f, 0.8125f )
		},
		{ /* DIAMOND */
		    new Vector2( 0.125f, 0.75f   ), new Vector2( 0.1875f, 0.75f   ),
		    new Vector2( 0.125f, 0.8125f ), new Vector2( 0.1875f, 0.8125f )
		},
	};

	Chunk owner;
	Vector3 position;
	BlockType element;

	public bool isSolid;

	public Block(BlockType b, Vector3 pos, Chunk c) {
		element = b;
		position = pos;
		owner = c;

		switch(element) {
			default:
				isSolid = true;
				break;
			case BlockType.AIR:
				isSolid = false;
				break;
		}
	}

	public void Draw() {
		if (isSolid) {
			// Culling
			if (!HasSolidNeighbour((int)position.x, (int)position.y, (int)position.z + 1))
				CreateQuad(Cubeside.FRONT);
			if (!HasSolidNeighbour((int)position.x, (int)position.y, (int)position.z - 1))
				CreateQuad(Cubeside.BACK);
			if (!HasSolidNeighbour((int)position.x, (int)position.y + 1, (int)position.z))
				CreateQuad(Cubeside.TOP);
			if (!HasSolidNeighbour((int)position.x, (int)position.y - 1, (int)position.z))
				CreateQuad(Cubeside.BOTTOM);
			if (!HasSolidNeighbour((int)position.x + 1, (int)position.y, (int)position.z))
				CreateQuad(Cubeside.RIGHT);
			if (!HasSolidNeighbour((int)position.x - 1, (int)position.y, (int)position.z))
				CreateQuad(Cubeside.LEFT);
		}
	}

	void CreateQuad(Cubeside side) {
		GameObject quad = new GameObject("quad");

		quad.transform.position = position;
		quad.transform.parent = owner.chunk.gameObject.transform;

		MeshFilter meshFilter = (MeshFilter) quad.AddComponent(typeof(MeshFilter));

		Mesh mesh = meshFilter.mesh = new Mesh();

		mesh.name = prefix + side.ToString();

		Vector3[] vertices = new Vector3[4];
		Vector3[] normals = new Vector3[4];

		// All Possible UV's
		Vector2 uv00 = new Vector2( 0f, 0f );
		Vector2 uv10 = new Vector2( 1f, 0f );
		Vector2 uv01 = new Vector2( 0f, 1f );
		Vector2 uv11 = new Vector2( 1f, 1f );

		if (element == BlockType.GRASS && side == Cubeside.TOP) {
			uv00 = blockUVs[0, 0];
			uv10 = blockUVs[0, 1];
			uv01 = blockUVs[0, 2];
			uv11 = blockUVs[0, 3];
		} else if (element == BlockType.GRASS && side == Cubeside.BOTTOM) {
			uv00 = blockUVs[(int)(BlockType.DIRT + 1), 0];
			uv10 = blockUVs[(int)(BlockType.DIRT + 1), 1];
			uv01 = blockUVs[(int)(BlockType.DIRT + 1), 2];
			uv11 = blockUVs[(int)(BlockType.DIRT + 1), 3];
		} else {
			uv00 = blockUVs[(int)(element + 1), 0];
			uv10 = blockUVs[(int)(element + 1), 1];
			uv01 = blockUVs[(int)(element + 1), 2];
			uv11 = blockUVs[(int)(element + 1), 3];
		}

		switch(side) {
			case Cubeside.BOTTOM:
				vertices = new Vector3[] { p0, p1, p2, p3 };
				normals = new Vector3[] {	Vector3.down,
																	Vector3.down,
																	Vector3.down,
																	Vector3.down };
			break;
			case Cubeside.TOP:
				vertices = new Vector3[] { p7, p6, p5, p4 };
				normals = new Vector3[] {	Vector3.up,
																	Vector3.up,
																	Vector3.up,
																	Vector3.up };
			break;
			case Cubeside.LEFT:
				vertices = new Vector3[] { p7, p4, p0, p3 };
				normals = new Vector3[] {	Vector3.left,
																	Vector3.left,
																	Vector3.left,
																	Vector3.left };
			break;
			case Cubeside.RIGHT:
				vertices = new Vector3[] { p5, p6, p2, p1 };
				normals = new Vector3[] {	Vector3.right,
																	Vector3.right,
																	Vector3.right,
																	Vector3.right };
			break;
			case Cubeside.FRONT:
				vertices = new Vector3[] { p4, p5, p1, p0 };
				normals = new Vector3[] {	Vector3.forward,
																	Vector3.forward,
																	Vector3.forward,
																	Vector3.forward };
			break;
			case Cubeside.BACK:
				vertices = new Vector3[] { p6, p7, p3, p2 };
				normals = new Vector3[] {	Vector3.back,
																	Vector3.back,
																	Vector3.back,
																	Vector3.back };
			break;
		}

		mesh.vertices = vertices;
		mesh.normals = normals;

		mesh.uv = new Vector2[] { uv11, uv01, uv00, uv10 };
		mesh.triangles = new int[] { 3, 1, 0, 3, 2, 1 };

		mesh.RecalculateBounds();
	}

	int ConvertBlockIndexToLocal(int i) {
		if (i == -1)
			i = World.chunkSize - 1;
		else if (i == World.chunkSize)
			i = 0;
		return i;
	}

	public bool HasSolidNeighbour(int x, int y, int z) {
		Block[,,] chunks;

		if (
			x < 0 || x >= World.chunkSize ||
			y < 0 || y >= World.chunkSize ||
			z < 0 || z >= World.chunkSize
		) {
			Vector3 neighborurChunkPos = owner.chunk.gameObject.transform.position + new Vector3(
				(x - (int)position.x) * World.chunkSize,
				(y - (int)position.y) * World.chunkSize,
				(z - (int)position.z) * World.chunkSize
			);

			string nName = World.BuildChunkName(neighborurChunkPos);

			Chunk nChunk;

			if (World.chunks.TryGetValue(nName, out nChunk)) {
				x = ConvertBlockIndexToLocal(x);
				y = ConvertBlockIndexToLocal(y);
				z = ConvertBlockIndexToLocal(z);
				chunks = nChunk.chunkData;
			} else return false;

		} else
			chunks = owner.chunkData;

		try {
			return chunks[x, y, z].isSolid;
		} catch(System.IndexOutOfRangeException ex) {
			/* Catcher - No Need To Do Anything */
		}

		return false;
	}
}
