//created: 10-4-19
//updated: 11-2-19
//version: .01
//author Vance Howald
//Player script to figure out what chunk the player is in for world loading
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLocScript : MonoBehaviour
{
    public LayerMask toColide;//ground layer
    public GameObject currentChunk = null;
    public bool isSaving = false;

    // Start is called before the first frame update
    void Start()
    {
        ProcessHit();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessHit();
    }

    //Processes the hit
    void ProcessHit()
    {
        if (!isSaving)
        {
            RaycastHit hit;
            //Note 1-1-20: Changed "20.0f" to "transform.position.y + 20.0f" so that the player will still update even when high in the air
            if (Physics.Raycast(transform.position, Vector3.down, out hit, transform.position.y + 20.0f, toColide))//check down, 20 can be higher
            {
                if (hit.collider.gameObject != currentChunk)//is it a different chunk
                {
                    currentChunk = hit.collider.gameObject;//set current chunk
                    WorldManager.instance.updateLoadedChunks(hit.collider.gameObject.GetComponent<Chunk>().worldLoc[0], hit.collider.gameObject.GetComponent<Chunk>().worldLoc[1]);//update the chunk
                }
            }
        }
    }
}
