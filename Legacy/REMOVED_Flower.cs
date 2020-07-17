//created: 10-11-19
//updated: 10-24-19
//version: .02
/*
 * Flower object to contain methods and propertied and stuff for each individual flower
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REMOVED_Flower : MonoBehaviour
{
    //Strings and stuff
    private readonly string MESH_PATH = "Assets/Models/";

    public FlowerObj flowerGridInstance;

    //Values between 0-15 for each element
    int petalIndex = 0;
    int stemIndex = 0;
    int pistilIndex = 0;
    int leafIndex = 0;

    //Values between 0-15 for each color
    int petalColorIndex = 0;
    int stemColorIndex = 0;
    int pistilColorIndex = 0;
    int leafColorIndex = 0;

    GameObject petalObject, stemObject, pistilObject;
    List<GameObject> leafObjects = new List<GameObject>();

    //The flower properties game object
    private FlowerProperties flowerProperties;

    //Build and color the flower
    void Start()
    {
        stemObject = transform.GetChild(0).gameObject;
        buildFlower(Random.Range(0,16), Random.Range(0, 16), Random.Range(0, 16), Random.Range(0, 16));
        colorFlower(Random.Range(0, 16), Random.Range(0, 16), Random.Range(0, 16), Random.Range(0, 16));
    }


    //Set the flower's color to the given index
    public void colorFlower(int inStem, int inLeaf, int inPetal, int inPistil)
    {
        petalColorIndex = inPetal;
        pistilColorIndex = inPistil;
        stemColorIndex = inStem;
        leafColorIndex = inLeaf;

        stemObject.GetComponent<MeshRenderer>().material.color = GameObject.Find("World").GetComponent<FlowerProperties>().flowerColors[inStem];
        pistilObject.GetComponent<MeshRenderer>().material.color = GameObject.Find("World").GetComponent<FlowerProperties>().flowerColors[inPistil];
        petalObject.GetComponent<MeshRenderer>().material.color = GameObject.Find("World").GetComponent<FlowerProperties>().flowerColors[inPetal];
        foreach (GameObject leaf in leafObjects)
        {
            leaf.GetComponent<MeshRenderer>().material.color = GameObject.Find("World").GetComponent<FlowerProperties>().flowerColors[inLeaf];
        }
    }

    //Creates a flower from the given full index
    public void createFlowerFromIndex(uint flowerIndex)
    {
        petalIndex = flowerProperties.getPetalIndexFromIndex(flowerIndex);
        pistilIndex = flowerProperties.getPistilIndexFromIndex(flowerIndex);
        stemIndex = flowerProperties.getStemIndexFromIndex(flowerIndex);
        leafIndex = flowerProperties.getLeafIndexFromIndex(flowerIndex);
        petalColorIndex = flowerProperties.getPetalColorIndexFromIndex(flowerIndex);
        pistilColorIndex = flowerProperties.getPistilColorIndexFromIndex(flowerIndex);
        stemColorIndex = flowerProperties.getStemColorIndexFromIndex(flowerIndex);
        leafColorIndex = flowerProperties.getLeafColorIndexFromIndex(flowerIndex);
        rebuildFlower();
        recolorFlower();
    }

    //Builds a flower with the given index for each element
    public void buildFlower(int inStem, int inLeaf, int inPetal, int inPistil)
    {
        //Set the indexes
        stemIndex = inStem;
        petalIndex = inPetal;
        pistilIndex = inPistil;
        leafIndex = inLeaf;

        //Destroy the flower before building a new one
        //Destroy the stem that destroys everything
        if (stemObject != null)
        {
            GameObject.Destroy(stemObject);
        }
        //Clear the leaves
        leafObjects.Clear();

        //Instantiate the given stem object
        stemObject = Instantiate(getStemObjectFromIndex(inStem),transform);
        Material stemMat = GameObject.Find("World").GetComponent<FlowerProperties>().stemMaterial;
        stemObject.GetComponent<MeshRenderer>().material = stemMat;

        //Add leaves and petal to the stem
        bool petalHasBeenFound = false;
        foreach (Transform child in stemObject.transform)
        {
            if (!petalHasBeenFound && child.gameObject.name.ToString().Equals("spetal"))
            {
                //It's the petal so it has been found
                petalHasBeenFound = true;
                //Now spawn it
                petalObject = Instantiate(getPetalObjectFromIndex(inPetal), child);
                Material petalMat = GameObject.Find("World").GetComponent<FlowerProperties>().petalMaterial;
                petalObject.GetComponent<MeshRenderer>().material = petalMat;
            }
            else
            {
                //It's a leaf so spawn it
                GameObject leaf = Instantiate(getLeafObjectFromIndex(inLeaf), child);
                Material leafMat = GameObject.Find("World").GetComponent<FlowerProperties>().leafMaterial;
                leaf.GetComponent<MeshRenderer>().material = leafMat;
                leafObjects.Add(leaf);
            }
        }

        //Add pistil to the petal
        pistilObject = Instantiate(getPistilObjectFromIndex(inPistil), petalObject.transform.GetChild(0));
        Material pistilMat = GameObject.Find("World").GetComponent<FlowerProperties>().pistilMaterial;
        pistilObject.GetComponent<MeshRenderer>().material = pistilMat;
    }

    //Builds the flower from its own index
    private void rebuildFlower()
    {
        buildFlower(stemIndex, leafIndex, petalIndex, pistilIndex);
    }

    //Colors the flower from its own colors
    private void recolorFlower()
    {
        colorFlower(stemColorIndex, leafColorIndex, petalColorIndex, pistilColorIndex);
    }

    //Getters for the gameObjects
    public GameObject getStemObjectFromIndex(int inStem)
    {
        return GameObject.Find("World").GetComponent<FlowerProperties>().flowerStem[inStem];
    }
    public GameObject getPetalObjectFromIndex(int inStem)
    {
        return GameObject.Find("World").GetComponent<FlowerProperties>().flowerPetal[inStem];
    }
    public GameObject getLeafObjectFromIndex(int inStem)
    {
        return GameObject.Find("World").GetComponent<FlowerProperties>().flowerLeaf[inStem];
    }
    public GameObject getPistilObjectFromIndex(int inStem)
    {
        return GameObject.Find("World").GetComponent<FlowerProperties>().flowerPistil[inStem];
    }

    //Returns the long form index of the flower
    public uint getFlowerIndex()
    {
        uint returnIndex = 0;
        returnIndex += (uint)petalIndex;
        returnIndex *= 16;
        returnIndex += (uint)petalColorIndex;
        returnIndex *= 16;
        returnIndex += (uint)stemIndex;
        returnIndex *= 16;
        returnIndex += (uint)stemColorIndex;
        returnIndex *= 16;
        returnIndex += (uint)leafIndex;
        returnIndex *= 16;
        returnIndex += (uint)leafColorIndex;
        returnIndex *= 16;
        returnIndex += (uint)pistilIndex;
        returnIndex *= 16;
        returnIndex += (uint)pistilColorIndex;
        return returnIndex;
    }
}
