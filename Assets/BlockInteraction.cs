using System.Collections.Generic;
using UnityEngine;

public class BlockInteraction : MonoBehaviour {

    public GameObject cam;
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;

            //for mouse clicking
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            //if ( Physics.Raycast (ray,out hit,10)) {

            //for cross hairs
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 10)) {
                Vector3 hitBlock = hit.point - hit.normal / 2.0f;

                int x = (int)(Mathf.Round(hitBlock.x) - hit.collider.transform.position.x);
                int y = (int)(Mathf.Round(hitBlock.y) - hit.collider.transform.position.y);
                int z = (int)(Mathf.Round(hitBlock.z) - hit.collider.transform.position.z);

                Chunk hitc;

                if (World.chunks.TryGetValue(hit.collider.gameObject.name, out hitc) && hitc.chunkData[x, y, z].HitBlock()) {
                    List<string> updates = new List<string>();

                    float chunkX = hitc.chunk.transform.position.x;
                    float chunkY = hitc.chunk.transform.position.y;
                    float chunkZ = hitc.chunk.transform.position.z;

                    // could we use HasSolidNeighbour to check instead?
                    // update neighbors?
                    if (x == 0)
                        updates.Add(World.BuildChunkName(new Vector3(chunkX - World.chunkSize, chunkY, chunkZ)));
                    if (x == World.chunkSize - 1)
                        updates.Add(World.BuildChunkName(new Vector3(chunkX + World.chunkSize, chunkY, chunkZ)));
                    if (y == 0)
                        updates.Add(World.BuildChunkName(new Vector3(chunkX, chunkY - World.chunkSize, chunkZ)));
                    if (y == World.chunkSize - 1)
                        updates.Add(World.BuildChunkName(new Vector3(chunkX, chunkY + World.chunkSize, chunkZ)));
                    if (z == 0)
                        updates.Add(World.BuildChunkName(new Vector3(chunkX, chunkY, chunkZ - World.chunkSize)));
                    if (z == World.chunkSize - 1)
                        updates.Add(World.BuildChunkName(new Vector3(chunkX, chunkY, chunkZ + World.chunkSize)));

                    foreach (string cname in updates) {
                        Chunk c;

                        if (World.chunks.TryGetValue(cname, out c)) c.Redraw();
                    }
                }
            }
        }
	}
}
