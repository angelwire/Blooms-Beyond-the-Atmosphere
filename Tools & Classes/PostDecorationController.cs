using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Author William Jones
public class PostDecorationController : ConnectablePlaceable
{
    Camera mainCamera;

    public override bool canBePlaced(RaycastHit hit)
    {
        int[] flowerGridLoc = WorldManager.instance.worldPointToFlowerGrid(hit.point);//find what grid the point is in
        return WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]] == null || WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]].isPlaceable();
    }

    public override GameObject cloneObj(GameObject model, Placeable data, RaycastHit hit)
    {
        int[] flowerGridLoc = WorldManager.instance.worldPointToFlowerGrid(hit.point);//find what grid the point is in
        flowerGridPos = flowerGridLoc;
        GameObject newFlower = GameObject.Instantiate(model, posClamp(hit), Quaternion.identity, hit.transform);//make a copy
        newFlower.name = flowerGridLoc[0] + " " + flowerGridLoc[1];//placement stuff
        //Makes sure the flower is active and all of its components are enabled
        newFlower.SetActive(true);
        newFlower.GetComponent<PostDecorationController>().enabled = true;
        newFlower.transform.LookAt(transform.position);
        newFlower.transform.eulerAngles = new Vector3(0, 0, 0);
        newFlower.SetActive(true);
        newFlower.transform.localScale = Vector3.one;
        //Update the connectors
        newFlower.GetComponent<PostDecorationController>().updateConnectors();
        newFlower.GetComponent<PostDecorationController>().setHover(false);
        setHover(false);

        PostDecorationController currentFlowerObj = newFlower.GetComponent<PostDecorationController>();
        WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]] = currentFlowerObj;//move the flower data over

        return newFlower;
    }

    public override Vector3 posClamp(RaycastHit hit)
    {

        //This is to limit where in the cell the player can place the flower
        float chunkOverFlower = (float)WorldManager.instance.chunkWidth / WorldManager.instance.FlowerWidthPerChunk;
        Vector3 flowerCellCenterPosition = VectorUtilities.VectorAdd((VectorUtilities.VectorFloor(hit.point * chunkOverFlower) / chunkOverFlower), (chunkOverFlower * .5f));
        flowerCellCenterPosition.y = 0;//for some reason y turns int -.5
        float maxDistance = (chunkOverFlower * .5f) * WorldManager.flowerMaxCenterDistance;
        Vector3 flowerDistanceFromCenter = VectorUtilities.VectorClamp(hit.point - flowerCellCenterPosition, 0, 0);
        return (flowerCellCenterPosition + flowerDistanceFromCenter);
    }

    // Start is called before the first frame update
    void Start()
    {
        //Set the bounds so it won't get clipped prematurely
        gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(10, 10, 10));
        mainCamera = Camera.main;
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
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Placeable");
            }
        }
    }

    public override void clearPos()
    {
        WorldManager.instance.placableGrid[flowerGridPos[0], flowerGridPos[1]] = null;
        WorldManager.instance.updateFlowerPos(flowerGridPos[0], flowerGridPos[1]);

        //Clear out all connectors connected to this 
        clearConnectors();

    }

    /// <summary>
    /// Checks the 4 adjacent cells for fence posts and adds a connector
    /// </summary>
    /// <param name="updateForce">Should the override array update, generally you want it true but on loading this should be false</param>
    public override void updateConnectors(bool updateForce=true)
    {
        //Debug.Log("Updating Connectors for object: " + this.gameObject.name);

        WorldManager world = WorldManager.instance;
        int checkX;
        int checkY;
        Placeable checkPlaceable;

        //Check left
        checkX = world.trueX(flowerGridPos[0] - 1);
        checkY = world.trueY(flowerGridPos[1]);
        checkPlaceable = world.placableGrid[checkX, checkY];
        if (checkPlaceable != null)
        {
            //Debug.Log("To the left is: " + checkPlaceable.gameObject.name);
            if (!checkPlaceable.isFlower && checkPlaceable is ConnectablePlaceable)
            {
                if (checkPlaceable.id != id && updateForce)
                {
                    forcedConnectors[1] = true;
                }
                hardConnectToPlaceable(checkPlaceable, 90f, 1);
            }
        }

        //Check up
        checkX = world.trueX(flowerGridPos[0]);
        checkY = world.trueY(flowerGridPos[1] - 1);
        checkPlaceable = world.placableGrid[checkX, checkY];
        if (checkPlaceable != null)
        {
            //Debug.Log("To the up is: " + checkPlaceable.gameObject.name);
            if (!checkPlaceable.isFlower && checkPlaceable is ConnectablePlaceable)
            {
                if (checkPlaceable.id != id && updateForce)
                {
                    forcedConnectors[0] = true;
                }
                hardConnectToPlaceable(checkPlaceable, 0f, 0);
            }
        }

        //Check down
        checkX = world.trueX(flowerGridPos[0]);
        checkY = world.trueY(flowerGridPos[1] + 1);
        checkPlaceable = world.placableGrid[checkX, checkY];
        if (checkPlaceable != null)
        {
            //Debug.Log("To the down is: " + checkPlaceable.gameObject.name);
            if (!checkPlaceable.isFlower && checkPlaceable is ConnectablePlaceable)
            {
                if (checkPlaceable.id != id && updateForce)
                {
                    forcedConnectors[2] = true;
                }
                hardConnectToPlaceable(checkPlaceable, 180f, 2);
            }
        }

        //Check right
        checkX = world.trueX(flowerGridPos[0] + 1);
        checkY = world.trueY(flowerGridPos[1]);
        checkPlaceable = world.placableGrid[checkX, checkY];
        if (checkPlaceable != null)
        {
            //Debug.Log("To the right is: " + checkPlaceable.gameObject.name);
            if (!checkPlaceable.isFlower && checkPlaceable is ConnectablePlaceable)
            {
                if (checkPlaceable.id != id && updateForce)
                {
                    forcedConnectors[3] = true;
                }
                hardConnectToPlaceable(checkPlaceable, 270f, 3);
            }
        }
    }

    /// <summary>
    /// Builds a prefab to connect the current placeable to the given placeable
    /// </summary>
    /// <param name="connectPlaceable"></param>
    /// <param name="angle"></param>
    public override void hardConnectToPlaceable(Placeable connectPlaceable, float angle, int forcedConnectorIndex)
    {
        ConnectablePlaceable downconverted = (ConnectablePlaceable)connectPlaceable;
        int otherObjectsForcedIndex=-1;
        if (downconverted is PostDecorationController)
        {
            otherObjectsForcedIndex = WorldManager.instance.mod(forcedConnectorIndex+2, 4);
        }
        else if (downconverted is ArchControler)
        {
            otherObjectsForcedIndex = ((ArchControler)downconverted).getForcedIndex(flowerGridPos[0], flowerGridPos[1]);
        }
        if (downconverted.id != id && forcedConnectors[forcedConnectorIndex] == false)
        { }
        else
        {
            downconverted.forcedConnectors[otherObjectsForcedIndex] = false;
            foreach (GameObject p in connectorList)
            {
                if (downconverted.connectorList.Contains(p))
                    return;
            }
            //Debug.Log("Hard connecting");
            GameObject connectorPrefab = Instantiate(GameObject.Find("World").GetComponent<FlowerProperties>().getConnectorPrefab(this.id), gameObject.transform);
            connectorPrefab.transform.localPosition = Vector3.zero;
            connectorPrefab.transform.localRotation = Quaternion.Euler(0, angle, 0);
            connectorList.Add(connectorPrefab);
            ((ConnectablePlaceable)connectPlaceable).softConnectToPlaceable(connectorPrefab);
        }
    }

    /// <summary>
    /// Connects the given post to the current post
    /// (only by adding a reference to the connector, does not instantiate a new object)
    /// </summary>
    /// <param name="connector"></param>
    public override void softConnectToPlaceable(GameObject connector)
    {
        cleanConnectorList();
        //Debug.Log("Soft connecting");
        connectorList.Add(connector);
    }

    /// <summary>
    /// Clears all connectors connected to this post
    /// </summary>
    public override void clearConnectors()
    {
        foreach (GameObject go in connectorList)
        {
            if (go != null)
            {
                GameObject.DestroyImmediate(go);
            }
        }
        cleanConnectorList();
    }

    /// <summary>
    /// Removes all null references from the list
    /// </summary>
    public override void cleanConnectorList()
    {
        int ii = 0;
        while(ii<connectorList.Count)
        {
            if (connectorList[ii] == null)
            {
                connectorList.RemoveAt(ii);
            }
            else
            {
                ii += 1;
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        Vector3 camPos = mainCamera.transform.position;
        //Get the squared distance
        float xDifference = pos.x - camPos.x;
        float zDifference = pos.z - camPos.z;
        float distanceFromCamera = (xDifference * xDifference) + (zDifference * zDifference);

        //Scale the item so it slowly transitions into the view
        float maxFlowerDistance = 600;
        float minFlowerDistance = 200;
        float scaleAmount = Mathf.Clamp(1 - ((distanceFromCamera - minFlowerDistance) / (maxFlowerDistance - minFlowerDistance)), .01f, 1);
        transform.transform.localScale = new Vector3(1f, scaleAmount, 1f);
    }

    public override void load()
    {
        if(PlayerPrefs.HasKey(WarpDriveController.instance.currentPlanetID+" "+flowerGridPos[0]+"x"+ flowerGridPos[1]+"y_post"))
        {
            string key = PlayerPrefs.GetString(WarpDriveController.instance.currentPlanetID + " " + flowerGridPos[0] + "x" + flowerGridPos[1] + "y_post");
            for (int i = 0; i < 4; i++)
            {
                forcedConnectors[i] = key.ToCharArray()[i] == '1' ? true : false;
            }
        }
    }

    public override string save()
    {
        PlayerPrefs.DeleteKey(WarpDriveController.instance.currentPlanetID + " " + flowerGridPos[0] + "x" + flowerGridPos[1] + "y_post");
        string toSet = "";
        for (int i = 0; i < 4; i++)
        {
            toSet += forcedConnectors[i] == true ? "1" : "0";
        }
        PlayerPrefs.SetString(WarpDriveController.instance.currentPlanetID + " " + flowerGridPos[0] + "x" + flowerGridPos[1] + "y_post", toSet);
        return base.save();
    }

}
