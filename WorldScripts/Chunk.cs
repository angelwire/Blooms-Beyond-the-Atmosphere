//created: 9-30-19
//updated: 11-2-19
//version: .01
//author Vance Howald
//This is the chunk's loc data
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int[] worldLoc=new int[2]; // x y

    //Update the mesh bounds so it doesn't get culled
    void Start()
    {
        gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(10, 10, 10));
    }
}
