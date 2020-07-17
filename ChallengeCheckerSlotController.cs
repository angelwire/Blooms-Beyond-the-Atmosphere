using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//updated: 11-25-19
//version: .02
//Author: William Jones
/*
 * Controls the individual slots for the challenge checkers
 */


public class ChallengeCheckerSlotController : MonoBehaviour
{
    int meshIndex;
    int colorIndex;
    public enum MeshType { PETAL, STEM, LEAF, PISTIL}
    MeshType myType;
    GameObject flowerPartObject;
    float rotationSpeed = 20;
    FlowerProperties propertiesScript;

    //Find the flower properties script
    public void Start()
    {
        propertiesScript = GameObject.Find("World").GetComponent<FlowerProperties>();
    }

    //Rotate the element
    public void Update()
    {
        if (flowerPartObject != null)
        {
            flowerPartObject.transform.RotateAround(transform.position, transform.up, rotationSpeed * Time.deltaTime);
        }
    }

    //Resets the element to the given index
    public void setIndex(MeshType inType, int inMeshIndex, int inColorIndex)
    {
        myType = inType;
        meshIndex = inMeshIndex;
        colorIndex = inColorIndex;

        Material setMaterial;
        GameObject setObject;
        Color setColor;
        float scaleAmount; //How much to scale the mesh
        float xRotation; //How much rotation to apply to the mesh at the start
        float yOffset; //How far in the slot to offset the element (0 = 0, 1 is bottom of slot, -1 is top)


        //Be sure that there is a properties script
        if (propertiesScript == null)
        {
            propertiesScript = GameObject.Find("World").GetComponent<FlowerProperties>();
        }

        //Sets various values depending on the type
        switch (myType)
        {
            case MeshType.PISTIL:
                setMaterial = propertiesScript.pistilMaterial;
                setObject = propertiesScript.flowerPistil[meshIndex];
                scaleAmount = 100;
                xRotation = 15;
                yOffset = .25f;
                break;
            case MeshType.PETAL:
                setMaterial = propertiesScript.petalMaterial;
                setObject = propertiesScript.flowerPetal[meshIndex];
                scaleAmount = 120;
                xRotation = 20;
                yOffset = .25f;
                break;
            case MeshType.LEAF:
                setMaterial = propertiesScript.petalMaterial;
                setObject = propertiesScript.flowerLeaf[meshIndex];
                scaleAmount = 100;
                xRotation = 15;
                yOffset = 0f;
                break;
            case MeshType.STEM:
                setMaterial = propertiesScript.stemMaterial;
                setObject = propertiesScript.flowerStem[meshIndex];
                scaleAmount = 200;
                xRotation = 5;
                yOffset = 1f;
                break;
            default:
                setMaterial = propertiesScript.stemMaterial;
                setObject = propertiesScript.flowerStem[meshIndex];
                scaleAmount = 200;
                xRotation = 5;
                yOffset = .5f;
                break;
        }

        //What color to set the flower part
        setColor = propertiesScript.flowerColors[colorIndex];
        
        if (flowerPartObject == null) //If the part hasn't been created yet
        {
            //Instantiate a flower part
            flowerPartObject = Instantiate(setObject, gameObject.transform);

            //Compute the size of the flower part model
            float meshHeight = flowerPartObject.GetComponent<MeshFilter>().mesh.bounds.size.y * 2;
            float meshWidth =
                Mathf.Max((flowerPartObject.GetComponent<MeshFilter>().mesh.bounds.size.x * 2),
                (flowerPartObject.GetComponent<MeshFilter>().mesh.bounds.size.z * 2));

            //Scale it and rotate it to make it easier to see
            flowerPartObject.transform.localScale = Vector3.one * (1 / (Mathf.Max(meshHeight, meshWidth)));
            flowerPartObject.transform.RotateAround(transform.position, transform.right, xRotation);

            //Set the material, color, and layer
            flowerPartObject.GetComponent<MeshRenderer>().sharedMaterial = setMaterial;
            flowerPartObject.GetComponent<MeshRenderer>().material.color = setColor;
            flowerPartObject.layer = LayerMask.NameToLayer("UI");

            //Scale it depending on the given amount (from the switch statement)
            flowerPartObject.transform.localScale = flowerPartObject.transform.localScale * scaleAmount;
            
            //Compute the y offset depending on the mesh size
            if (meshWidth > meshHeight)
            {
                yOffset *= meshHeight / meshWidth;
            }
            //Move the model depending on the computed offset
            flowerPartObject.transform.Translate(-Vector3.up * yOffset, Space.Self);

            //Leaves have a unique origin so move then so that they'll spin from the middle of the geometry
            if (myType == MeshType.LEAF)
            {
                flowerPartObject.transform.Translate(Vector3.right * .5f, Space.Self);
            }
        }
        else
        {
            //Destroy the object first
            GameObject.Destroy(flowerPartObject);

            //Instantiate a new flower part
            flowerPartObject = Instantiate(setObject, gameObject.transform);

            //Compute the size of the flower part model
            float meshHeight = flowerPartObject.GetComponent<MeshFilter>().mesh.bounds.size.y * 2;
            float meshWidth =
                Mathf.Max((flowerPartObject.GetComponent<MeshFilter>().mesh.bounds.size.x * 2),
                (flowerPartObject.GetComponent<MeshFilter>().mesh.bounds.size.z * 2));

            //Scale it and rotate it to make it easier to see
            flowerPartObject.transform.localScale = Vector3.one * (1 / (Mathf.Max(meshHeight, meshWidth)));
            flowerPartObject.transform.RotateAround(transform.position, transform.right, xRotation);

            //Set the material, color, and layer
            flowerPartObject.GetComponent<MeshRenderer>().sharedMaterial = setMaterial;
            flowerPartObject.GetComponent<MeshRenderer>().material.color = setColor;
            flowerPartObject.layer = LayerMask.NameToLayer("UI");

            //Scale it depending on the given amount (from the switch statement)
            flowerPartObject.transform.localScale = flowerPartObject.transform.localScale * scaleAmount;

            //Compute the y offset depending on the mesh size
            if (meshWidth > meshHeight)
            {
                yOffset *= meshHeight / meshWidth;
            }
            //Move the model depending on the computed offset
            flowerPartObject.transform.Translate(-Vector3.up * yOffset, Space.Self);

            //Leaves have a unique origin so move then so that they'll spin from the middle of the geometry
            if (myType == MeshType.LEAF)
            {
                flowerPartObject.transform.Translate(Vector3.right * .5f, Space.Self);
            }
        }
    }

    //Compares the slot's element to the given element index 
    public void compareIndex(int inMeshIndex, int inColorIndex)
    {
        //Assume everything matches
        if (inColorIndex == colorIndex && inMeshIndex == meshIndex)
        {
            GetComponent<Image>().color = Color.green;
        }
        //Assume the mesh matches and ask if the color matches
        else if (inColorIndex == colorIndex || inMeshIndex == meshIndex)
        {
            GetComponent<Image>().color = new Color(1,.5f,0);
        }
        //Regardless of the previous color, set it to red if the mesh doesn't match
        else
        {
            GetComponent<Image>().color = Color.red;
        }
    }

    //Resets the slot's color to white because there is not flower to compare it to
    public void doNotCompareIndex()
    {
        GetComponent<Image>().color = Color.white;
    }
}
