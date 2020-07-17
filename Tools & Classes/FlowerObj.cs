using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//updated: 11-25-19
//version: .06
/*
 * Flower object that is stored in the world grid
 * Author Vance Howald
 */

//now that parent exists calculations should be able to be simplified using parent pos and ChunkRealtivePos

public class FlowerObj : Placeable
{
    public const int OBJECT_INDEX = 0;
    public const int COLOR_INDEX = 1;
    public int[] petalIndex = { 0, 0 }; //Object, Color
    public int[] stemIndex = { 0, 0 };
    public int[] pistilIndex = { 0, 0 };
    public int[] leafIndex = { 0, 0 };
    public float timeToSpawn;//can be condensed into single var probs
    public Transform parent;//now that parent 
    public bool alive = false;
    public GameObject particles = null;

    private void Start()
    {
    }

    public FlowerObj()
    {
        isFlower = true;
    }

    //Constructor
    public void init(int[] petalIndex, int[] stemIndex, int[] pistilIndex, int[] leafIndex, int[] flowerGridPos, Transform parent)
    {
        this.petalIndex = petalIndex;
        this.stemIndex = stemIndex;
        this.pistilIndex = pistilIndex;
        this.leafIndex = leafIndex;
        this.flowerGridPos = flowerGridPos;
        this.parent = parent;
        //v uiScale is set in the prefab inspector v
        //uiScale = 1.7f;
        id = getFlowerIndex();
        isFlower = true;
        timeToSpawn = 0;
    }

    //Constructor with a flower
    public void init(FlowerObj f)
    {
        this.petalIndex = f.petalIndex;
        this.stemIndex = f.stemIndex;
        this.pistilIndex = f.pistilIndex;
        this.leafIndex = f.leafIndex;
        this.flowerGridPos = f.flowerGridPos;
        this.parent = f.parent;
        //uiScale = 1f;
        id = getFlowerIndex();
        isFlower = true;
        this.timeToSpawn = f.timeToSpawn;
    }

    public void init(string flowerID, int[] flowerGridPos, Transform parent)
    {
        int[] petalIndex= { Convert.ToInt32(""+flowerID[0], 16), Convert.ToInt32("" + flowerID[1], 16)};
        int[] stemIndex = { Convert.ToInt32("" + flowerID[2], 16), Convert.ToInt32("" + flowerID[3], 16) };
        int[] leafIndex = { Convert.ToInt32("" + flowerID[4], 16), Convert.ToInt32("" + flowerID[5], 16) };
        int[] pistilIndex = { Convert.ToInt32("" + flowerID[6], 16), Convert.ToInt32("" + flowerID[7], 16) };
        init(petalIndex, stemIndex, pistilIndex, leafIndex, flowerGridPos, parent);
    }

    //Returns the long form index of the flower
    //Petal-Petal_Color-Stem-Stem_Color-Leaf-Leaf_Color-Pistil-Pistil_Color
    public uint getFlowerIndex()//William
    {
        uint returnIndex = 0;
        returnIndex += (uint)petalIndex[0];
        returnIndex *= 16;
        returnIndex += (uint)petalIndex[1];
        returnIndex *= 16;
        returnIndex += (uint)stemIndex[0];
        returnIndex *= 16;
        returnIndex += (uint)stemIndex[1];
        returnIndex *= 16;
        returnIndex += (uint)leafIndex[0];
        returnIndex *= 16;
        returnIndex += (uint)leafIndex[1];
        returnIndex *= 16;
        returnIndex += (uint)pistilIndex[0];
        returnIndex *= 16;
        returnIndex += (uint)pistilIndex[1];
        return returnIndex;
    }

    /*int IComparable.CompareTo(object y)
    {
        FlowerObj yObj = (FlowerObj)y;
        if (timeToSpawn > yObj.timeToSpawn)
        {
            return 1;
        }
        if (timeToSpawn < yObj.timeToSpawn)
        {
            return -1;
        }
        return 0;
    }*/

    public int CompareTo(object y)//objects are equal if the x and y are the same
    {
        FlowerObj yObj = (FlowerObj)y;
        if (flowerGridPos[0] == yObj.flowerGridPos[0] && flowerGridPos[1] == yObj.flowerGridPos[1])
        {
            return 0;
        }
        return 1;
    }

    public override string ToString() //returns the time to spawn for debug purposes
    {
        return "" + timeToSpawn;
    }

    public override string save()
    {
        Vector3 pos = transform.position;
        Vector3 rot = transform.eulerAngles;
        string toReturn = "0|" + petalIndex[0] + "|" + petalIndex[1] + "|" + stemIndex[0] + "|" + stemIndex[1] + "|" + pistilIndex[0] + "|" + pistilIndex[1] + "|" + leafIndex[0] + "|" + leafIndex[1] + "|" + flowerGridPos[0] + "|" + flowerGridPos[1] + "|" + pos.x + "|" + pos.y+"|"+pos.z + "|" + (int)rot.x + "|" + (int)rot.y + "|" + (int)rot.z;
        return toReturn;
    }

    public override bool canBePlaced(RaycastHit hit)
    {
        int[] flowerGridLoc = WorldManager.instance.worldPointToFlowerGrid(hit.point);//find what grid the point is in
        return WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]] == null || WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]].isPlaceable();
    }

    public override Vector3 posClamp(RaycastHit hit)
    {

        //This is to limit where in the cell the player can place the flower
        float chunkOverFlower = (float)WorldManager.instance.chunkWidth / WorldManager.instance.FlowerWidthPerChunk;
        Vector3 flowerCellCenterPosition = VectorUtilities.VectorAdd((VectorUtilities.VectorFloor(hit.point * chunkOverFlower) / chunkOverFlower), (chunkOverFlower * .5f));
        float maxDistance = (chunkOverFlower * .5f) * WorldManager.flowerMaxCenterDistance;
        Vector3 flowerDistanceFromCenter = VectorUtilities.VectorClamp(hit.point - flowerCellCenterPosition, -maxDistance, maxDistance);
        return(flowerCellCenterPosition + flowerDistanceFromCenter);

    }

    public override GameObject cloneObj(GameObject model, Placeable data, RaycastHit hit)
    {
        int[] flowerGridLoc = WorldManager.instance.worldPointToFlowerGrid(hit.point);//find what grid the point is in
        GameObject newFlower = GameObject.Instantiate(model, posClamp(hit), Quaternion.identity, hit.transform);//make a copy
        newFlower.GetComponent<setCurvaturePosition>().baseYPosition = hit.point.y;
        newFlower.name = flowerGridLoc[0] + " " + flowerGridLoc[1];//placement stuff

        //Makes sure the flower is active and all of its components are enabled
        newFlower.SetActive(true);
        newFlower.GetComponent<FlowerObj>().enabled = true;
        newFlower.GetComponent<setCurvaturePosition>().enabled = true;
        newFlower.transform.LookAt(transform.position);
        newFlower.transform.eulerAngles = new Vector3(0, newFlower.transform.eulerAngles.y, 0);
        newFlower.transform.Rotate(new Vector3(0, -90, 0));
        newFlower.SetActive(true);

        FlowerObj currentFlowerObj = newFlower.GetComponent<FlowerObj>();
        WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]] = currentFlowerObj;//move the flower data over
        ((FlowerObj)WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]]).flowerGridPos = new int[] { flowerGridLoc[0], flowerGridLoc[1] };
        WorldManager.instance.RecalcAfterAdd(flowerGridLoc[0], flowerGridLoc[1]);
        //This is to limit where in the cell the player can place the flower

        //clearPos(x, y);
        return newFlower;
    }

    public override void clearPos()
    {
        WorldManager.instance.clearFlowerAt(flowerGridPos[0], flowerGridPos[1]);
    }
    
    //Sets the hover bool
    public override void setHover(bool hover)
    {
        if (hover != isHovered)
        {
            isHovered = hover;
            //Set the layer to the effects layer if it's hovered
            if (hover)
            {
                gameObject.layer = LayerMask.NameToLayer("OutlineEffects");
                foreach (Transform t in transform.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = LayerMask.NameToLayer("OutlineEffects");
                }
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Placeable");
                foreach (Transform t in transform.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = LayerMask.NameToLayer("Placeable");
                }
            }
        }
    }

    public void rebuildFlower()//William
    {
        GameObject flower = this.gameObject;

        FlowerObj flowerObj = this;
        GameObject world = GameObject.Find("World");
        FlowerProperties fp = world.GetComponent<FlowerProperties>();
        GameObject stemObject = transform.GetChild(0).gameObject;

        //Destroy the flower before building a new one
        //Destroy the stem that destroys everything
        if (stemObject != null)
        {
            GameObject.DestroyImmediate(stemObject);
        }
        try
        {
            //Instantiate the given stem object
            stemObject = Instantiate(fp.flowerStem[flowerObj.stemIndex[0]], flower.transform);
        }
        catch
        {
            //Instantiate the given stem object
            stemObject = Instantiate(fp.flowerStem[flowerObj.stemIndex[0]], flower.transform);
        }
        stemObject.layer = LayerMask.NameToLayer("Placeable");

        Material stemMat = fp.stemMaterial;
        stemObject.GetComponent<MeshRenderer>().material = stemMat;
        stemObject.GetComponent<MeshRenderer>().material.color = fp.flowerColors[flowerObj.stemIndex[1]];
        GameObject petalObject = null;
        //Add leaves and petal to the stem
        bool petalHasBeenFound = false;
        foreach (Transform child in stemObject.transform)
        {
            if (!petalHasBeenFound && child.gameObject.name.ToString().Equals("spetal"))
            {
                //It's the petal so it has been found
                petalHasBeenFound = true;
                //Now spawn it
                petalObject = Instantiate(fp.flowerPetal[flowerObj.petalIndex[0]], child);
                petalObject.layer = LayerMask.NameToLayer("Placeable");
                Material petalMat = fp.petalMaterial;
                petalObject.GetComponent<MeshRenderer>().material = petalMat;
                petalObject.GetComponent<MeshRenderer>().material.color = fp.flowerColors[flowerObj.petalIndex[1]];
            }
            else
            {
                //It's a leaf so spawn it
                GameObject leaf = Instantiate(fp.flowerLeaf[flowerObj.leafIndex[0]], child);
                leaf.layer = LayerMask.NameToLayer("Placeable");
                Material leafMat = fp.leafMaterial;
                leaf.GetComponent<MeshRenderer>().material = leafMat;
                leaf.GetComponent<MeshRenderer>().material.color = fp.flowerColors[flowerObj.leafIndex[1]];
            }
        }
        //Add pistil to the petal
        GameObject pistilObject = Instantiate(fp.flowerPistil[flowerObj.pistilIndex[0]], petalObject.transform.GetChild(0));
        pistilObject.layer = LayerMask.NameToLayer("Placeable");
        Material pistilMat = fp.pistilMaterial;

        pistilObject.GetComponent<MeshRenderer>().material = pistilMat;
        pistilObject.GetComponent<MeshRenderer>().material.color = fp.flowerColors[flowerObj.pistilIndex[1]];

        flower.name = flowerObj.flowerGridPos[0] + " " + flowerObj.flowerGridPos[1];
        world.GetComponent<WorldManager>().combineMeshes(flower.transform);
        world.GetComponent<WorldManager>().resizeBoxCollider(flower.gameObject);

        flower.layer = LayerMask.NameToLayer("Placeable");
    }

    public override bool isLivingFlower()
    {
        return alive;
    }

    /*  public FlowerObj(string s, Transform parent)//load
      {
          this.petalIndex = new int[] { int.Parse(processed[0]), int.Parse(processed[1]) };
          this.stemIndex = stemIndex;
          this.pistilIndex = pistilIndex;
          this.leafIndex = leafIndex;
          this.flowerGridPos = flowerGridPos;
          this.parent = parent;
              }*/
}
