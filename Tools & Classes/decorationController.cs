using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A controller for all the decorations and things
 * 12-30-19
 */
//Author William Jones
public class decorationController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(10, 10, 10));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
