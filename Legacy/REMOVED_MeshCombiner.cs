using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//updated: 11-2-19
//version: .02
/*
 * A class used to combine the meshes of a flower into a single mesh
 * This is now handled when the flower is generated and is no longer in use
 */


public class REMOVED_MeshCombiner : MonoBehaviour
{

    //Call this to combine the leaf meshes on a flower
    public void combineMeshes()
    {
        //Store the flower's transform
        Vector3 oldPosition = transform.position;
        Quaternion oldRotation = transform.rotation;

        //Reset the flower's transform
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        //Array to hold the mesh filters for all the children
        List<MeshFilter> meshFilters = new List<MeshFilter>(gameObject.GetComponentsInChildren<MeshFilter>());

        //Clear out the meshes that aren't leafs
        int mm = 0;
        while (mm < meshFilters.Count)
        {
            string goName = meshFilters[mm].gameObject.name;
            if (!goName.Contains("leaf"))
            {
                //If it's not a leaf remove it
                meshFilters.RemoveAt(mm);
            }
            else
            {
                //If it is a leaf skip it
                mm += 1;
            }
        }

        //If there are meshes to combine
        if (meshFilters.Count > 0)
        {
            //Set up the combine instance array
            CombineInstance[] combineInstance = new CombineInstance[meshFilters.Count];

            //Find the material to apply to the mesh that will be created
            Material useMaterial = meshFilters[0].gameObject.GetComponent<MeshRenderer>().sharedMaterial;

            //Loop through the mesh filters
            for(int ii = 0; ii < meshFilters.Count; ii+=1)
            {
                //Set the current combine instance mesh and transform
                combineInstance[ii].mesh = meshFilters[ii].mesh;
                combineInstance[ii].transform = meshFilters[ii].transform.localToWorldMatrix;
            
                //Deactivate the leaf's gameobject (the parent to the object with the mesh)
                meshFilters[ii].gameObject.transform.parent.gameObject.SetActive(false);
            }

            //Combine the mesh and add it
            transform.GetComponent<MeshFilter>().mesh = new Mesh();
            transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combineInstance, true);
            transform.GetComponent<MeshRenderer>().material = useMaterial;
            //transform.gameObject.SetActive(true);
        }

        //Now reset the position
        transform.position = oldPosition;
        transform.rotation = oldRotation;
    }
}
