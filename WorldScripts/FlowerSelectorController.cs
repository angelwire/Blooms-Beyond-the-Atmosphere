using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Author William Jones
public class FlowerSelectorController : MonoBehaviour
{
    MeshFilter myFilter;
    MeshRenderer myRenderer;
    [SerializeField] Material decorationMaterial;
    [SerializeField] Material flowerMaterial;
    private void Start()
    {
        myFilter = GetComponent<MeshFilter>();
        myRenderer = GetComponent<MeshRenderer>();
    }

    public void fitToGameObject(GameObject go)
    {
        if (go.GetComponent<FlowerObj>() != null)
        {
            MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combineInstance = new CombineInstance[meshFilters.Length];
            Vector3 oldPosition = go.transform.position;
            Quaternion oldRotation = go.transform.rotation;
            for (int i= 0; i < meshFilters.Length; i++)
            {
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                combineInstance[i].mesh = meshFilters[i].mesh;
                combineInstance[i].transform = meshFilters[i].transform.localToWorldMatrix * go.transform.worldToLocalMatrix;
            }
            //Combine the mesh and add it
            transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combineInstance, true);
            go.transform.position = oldPosition;
            go.transform.rotation = oldRotation;
            myRenderer.sharedMaterial = flowerMaterial;

            Transform oldParent = transform.parent;
            transform.parent = go.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.parent = oldParent;
            transform.position = new Vector3(transform.position.x, go.GetComponent<setCurvaturePosition>().baseYPosition, transform.position.z);
        }
        else
        {
            if (go.GetComponentInChildren<MeshFilter>() != null) //If there's a mesh to grab
            {
                myFilter.mesh = go.GetComponentInChildren<MeshFilter>().mesh;
            }
            else
            {
                //If there's not a mesh to grab then grab the skinned mesh renderer
                myFilter.mesh = go.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
            }
            myRenderer.sharedMaterial = decorationMaterial;

            Transform oldParent = transform.parent;
            transform.parent = go.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.parent = oldParent;
        }
    }
}
