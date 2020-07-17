using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author Vance Howald
public class ArchControler : ConnectablePlaceable
{
    int[] flowerGridPos2 = { -1,-1};//this needs to be fixed but rn is needed for the object to disapear
    Camera mainCamera;

    public override bool canBePlaced(RaycastHit hit)
    {
        mainCamera = Camera.main;
        int yRot = (int)((mainCamera.transform.parent.eulerAngles.y + 225) / 90);
        int[] flowerGridLoc = WorldManager.instance.worldPointToFlowerGrid(hit.point);//find what grid the point is in
        float chunkOverFlower = (float)WorldManager.instance.chunkWidth / WorldManager.instance.FlowerWidthPerChunk;
        Vector3 flowerCellCenterPosition = VectorUtilities.VectorAdd((VectorUtilities.VectorFloor(hit.point * chunkOverFlower) / chunkOverFlower), (chunkOverFlower * .5f));
        int x2 = flowerGridLoc[0];
        int y2 = flowerGridLoc[1];
        Vector3 distanceFromCenter = hit.point - flowerCellCenterPosition;
        if (yRot % 2 == 0)
        {
            if (distanceFromCenter.x > 0)
            {
                x2+=2;
                flowerGridLoc[0]--;
            }
            else
            {
                x2-=2;
                flowerGridLoc[0]++;
            }
        }
        else
        {
            if (distanceFromCenter.z > 0)
            {
                y2+=2;
                flowerGridLoc[1]--;
            }
            else
            {
                y2-=2;
                flowerGridLoc[1]++;
            }
        }
        flowerGridLoc[0] = WorldManager.instance.trueX(flowerGridLoc[0]);
        flowerGridLoc[1] = WorldManager.instance.trueY(flowerGridLoc[1]);
        x2 = WorldManager.instance.trueX(x2);
        y2 = WorldManager.instance.trueY(y2);
        return (WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]] == null || WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]].isPlaceable()) && (WorldManager.instance.placableGrid[x2, y2] == null || WorldManager.instance.placableGrid[x2, y2].isPlaceable());

    }

    private void Start()
    {
        //Set the bounds so it won't get clipped prematurely
        gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(10, 10, 10));
        mainCamera = Camera.main;
        setHover(false);
    }
    

    public override Vector3 posClamp(RaycastHit hit)
    {
        int yRot = (int)((mainCamera.transform.parent.eulerAngles.y + 225) / 90);
        int[] flowerGridLoc = WorldManager.instance.worldPointToFlowerGrid(hit.point);//find what grid the point is in
        float chunkOverFlower = (float)WorldManager.instance.chunkWidth / WorldManager.instance.FlowerWidthPerChunk;
        Vector3 flowerCellCenterPosition = VectorUtilities.VectorAdd((VectorUtilities.VectorFloor(hit.point * chunkOverFlower) / chunkOverFlower), (chunkOverFlower * .5f));
        Vector3 distanceFromCenter = hit.point - flowerCellCenterPosition;
        if (yRot % 2 == 0)
        {
            if (distanceFromCenter.x > 0)
            {
                flowerCellCenterPosition += Vector3.right * 0.5f;
            }
            else
            {
                flowerCellCenterPosition -= Vector3.right * 0.5f;
            }
        }
        else
        {
            if (distanceFromCenter.z > 0)
            {
                flowerCellCenterPosition += Vector3.forward * 0.5f;
            }
            else
            {
                flowerCellCenterPosition -= Vector3.forward * 0.5f;
            }
        }
        flowerCellCenterPosition.y = 0;
        return flowerCellCenterPosition;
    }

    public override GameObject cloneObj(GameObject model, Placeable data, RaycastHit hit)
    {
        GameObject newFlower = GameObject.Instantiate(model, posClamp(hit), Quaternion.identity, hit.transform);//make a copy
        //newFlower.name = x + " " + y;//placement stuff
        //        newFlower.transform.Rotate(new Vector3(0, 90, 0));
        int yRot = (int)((mainCamera.transform.parent.eulerAngles.y + 225) / 90);
        newFlower.SetActive(true);
        newFlower.transform.localScale = Vector3.one;
        newFlower.transform.LookAt(transform.position); //do rot last to make fences easier
        newFlower.transform.eulerAngles = new Vector3(0, yRot * 90, 0);

        ArchControler currentFlowerObj = newFlower.GetComponent<ArchControler>();

        flowerGridPos = WorldManager.instance.worldPointToFlowerGrid(hit.point);//find what grid the point is in
        float chunkOverFlower = (float)WorldManager.instance.chunkWidth / WorldManager.instance.FlowerWidthPerChunk;
        Vector3 flowerCellCenterPosition = VectorUtilities.VectorAdd((VectorUtilities.VectorFloor(hit.point * chunkOverFlower) / chunkOverFlower), (chunkOverFlower * .5f));
        int x2 = flowerGridPos[0];
        int y2 = flowerGridPos[1];
        Vector3 distanceFromCenter = hit.point - flowerCellCenterPosition;
        if (yRot % 2 == 0)
        {
            if (distanceFromCenter.x > 0)
            {
                x2 += 2;
                flowerGridPos[0]--;
            }
            else
            {
                x2 -= 2;
                flowerGridPos[0]++;
            }
        }
        else
        {
            if (distanceFromCenter.z > 0)
            {
                y2 += 2;
                flowerGridPos[1]--;
            }
            else
            {
                y2 -= 2;
                flowerGridPos[1]++;
            }
        }

        flowerGridPos[0]=WorldManager.instance.trueX(flowerGridPos[0]);
        flowerGridPos[1]=WorldManager.instance.trueY(flowerGridPos[1]);
        flowerGridPos2[0]=WorldManager.instance.trueX(x2);
        flowerGridPos2[1]=WorldManager.instance.trueY(y2);
        WorldManager.instance.removeFromFlowerList(flowerGridPos[0], flowerGridPos[1]);
        WorldManager.instance.removeFromFlowerList(flowerGridPos2[0], flowerGridPos2[1]);
        WorldManager.instance.placableGrid[flowerGridPos[0], flowerGridPos[1]] = currentFlowerObj;//move the flower data over
        WorldManager.instance.placableGrid[flowerGridPos2[0], flowerGridPos2[1]] = currentFlowerObj;//move the flower data over
        currentFlowerObj.flowerGridPos = flowerGridPos;
        currentFlowerObj.flowerGridPos2 = new int[] { flowerGridPos2[0], flowerGridPos2[1] };

        

        currentFlowerObj.updateConnectors();

        return newFlower;    
    }

    public override void clearPos()
    {
        WorldManager.instance.placableGrid[flowerGridPos[0], flowerGridPos[1]] = null;
        WorldManager.instance.placableGrid[flowerGridPos2[0], flowerGridPos2[1]] = null;
        WorldManager.instance.updateFlowerPos(flowerGridPos[0], flowerGridPos[1]);
        WorldManager.instance.updateFlowerPos(flowerGridPos2[0], flowerGridPos2[1]);

        clearConnectors();
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="xPos"></param>
    /// <param name="yPos"></param>
    /// <param name="updateForce">Should the override array update, generally you want it true but on loading this should be false</param>
    private void updateConnectors(int xPos, int yPos, int forceUpdateOffset, bool updateForce=true)
    {
        WorldManager world = WorldManager.instance;
        int checkX;
        int checkY;
        Placeable checkPlaceable;

        //Check left
        checkX = world.trueX(xPos - 1);
        checkY = world.trueY(yPos);
        checkPlaceable = world.placableGrid[checkX, checkY];
        if (checkPlaceable != null)
        {
            Debug.Log("To the left is: " + checkPlaceable.gameObject.name);
            if (!checkPlaceable.isFlower && checkPlaceable.id % 10 == 2)
            {
                if (checkPlaceable.id != id && updateForce)
                {
                    forcedConnectors[1+ forceUpdateOffset] = true;
                }
                hardConnectToPlaceable(checkPlaceable, 90f, 1+ forceUpdateOffset);
            }
        }

        //Check up
        checkX = world.trueX(xPos);
        checkY = world.trueY(yPos - 1);
        checkPlaceable = world.placableGrid[checkX, checkY];
        if (checkPlaceable != null)
        {
            Debug.Log("To the up is: " + checkPlaceable.gameObject.name);
            if (!checkPlaceable.isFlower && checkPlaceable.id % 10 == 2)
            {
                if (checkPlaceable.id != id && updateForce)
                {
                    forcedConnectors[0+ forceUpdateOffset] = true;
                }
                hardConnectToPlaceable(checkPlaceable, 0f, 0 + forceUpdateOffset);
            }
        }

        //Check down
        checkX = world.trueX(xPos);
        checkY = world.trueY(yPos + 1);
        checkPlaceable = world.placableGrid[checkX, checkY];
        if (checkPlaceable != null)
        {
            Debug.Log("To the down is: " + checkPlaceable.gameObject.name);
            if (!checkPlaceable.isFlower && checkPlaceable.id % 10 == 2)
            {
                if (checkPlaceable.id != id && updateForce)
                {
                    forcedConnectors[2+ forceUpdateOffset] = true;
                }
                hardConnectToPlaceable(checkPlaceable, 180f, 2 + forceUpdateOffset);
            }
        }

        //Check right
        checkX = world.trueX(xPos + 1);
        checkY = world.trueY(yPos);
        checkPlaceable = world.placableGrid[checkX, checkY];
        if (checkPlaceable != null)
        {
            Debug.Log("To the right is: " + checkPlaceable.gameObject.name);
            if (!checkPlaceable.isFlower && checkPlaceable.id % 10 == 2)
            {
                if (checkPlaceable.id != id && updateForce)
                {
                    forcedConnectors[3+ forceUpdateOffset] = true;
                }
                hardConnectToPlaceable(checkPlaceable, 270f, 3 + forceUpdateOffset);
            }
        }
    }

    /// <summary>
    /// Checks the 4 adjacent cells for fence posts and adds a connector
    /// </summary>
    /// <param name="updateForce">Should the override array update, generally you want it true but on loading this should be false</param>
    public override void updateConnectors(bool updateForce = true)
    {
        Debug.Log("Updating Connectors for object: " + this.gameObject.name);

        updateConnectors(flowerGridPos[0], flowerGridPos[1], 0, updateForce);
        updateConnectors(flowerGridPos2[0], flowerGridPos2[1], 4, updateForce);
    }

    /// <summary>
    /// Builds a prefab to connect the current placeable to the given placeable
    /// </summary>
    /// <param name="connectPlaceable"></param>
    /// <param name="angle"></param>
    public override void hardConnectToPlaceable(Placeable connectPlaceable, float angle, int forcedConnectorIndex)
    {
        ConnectablePlaceable downconverted = (ConnectablePlaceable)connectPlaceable;
        int otherObjectsForcedIndex = WorldManager.instance.mod(forcedConnectorIndex + 2, 4);//it will allways be a fence because of how the system works
        
        if (downconverted.id != id && forcedConnectors[forcedConnectorIndex] == false)
        {
        }
        else
        {
            downconverted.forcedConnectors[otherObjectsForcedIndex] = false;
            Debug.Log("Hard connecting");
            GameObject connectorPrefab = Instantiate(GameObject.Find("World").GetComponent<FlowerProperties>().getConnectorPrefab(this.id));
            connectorPrefab.transform.localPosition = gameObject.transform.position;
            connectorPrefab.transform.RotateAround(transform.position, Vector3.up, angle);
            if (Vector3.Dot(transform.right, transform.position - connectPlaceable.transform.position) > 0)//to the right
            {
                connectorPrefab.transform.position -= transform.right * 1.5f;
            }
            else
            {
                connectorPrefab.transform.position += transform.right * 1.5f;
            }
            //connectorPrefab.transform.localRotation = Quaternion.Euler(0, angle, 0);
            connectorPrefab.transform.parent = gameObject.transform;
            connectorList.Add(connectorPrefab);
            downconverted.softConnectToPlaceable(connectorPrefab);
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
        Debug.Log("Soft connecting");
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
        while (ii < connectorList.Count)
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

    public int getForcedIndex(int otherX, int otherY)
    {
        int toReturn;
        WorldManager world = WorldManager.instance;
        if (Mathf.Abs(otherX - flowerGridPos[0]) <= 1 && Mathf.Abs(otherY - flowerGridPos[1]) <= 1)
        {
            if (otherX == world.trueX(flowerGridPos[0] - 1) && otherY == world.trueY(flowerGridPos[1]))
            { toReturn = 1; }
            else if (otherX == world.trueX(flowerGridPos[0]) && otherY == world.trueY(flowerGridPos[1] - 1))
            { toReturn = 0; }
            else if (otherX == world.trueX(flowerGridPos[0]) && otherY == world.trueY(flowerGridPos[1]) + 1)
            { toReturn = 2; }
            else
            { toReturn = 3; }

        }
        else
        {
            if (otherX == world.trueX(flowerGridPos2[0] - 1) && otherY == world.trueY(flowerGridPos2[1]))
            { toReturn = 5; }
            else if (otherX == world.trueX(flowerGridPos2[0]) && otherY == world.trueY(flowerGridPos2[1] - 1))
            { toReturn = 4; }
            else if (otherX == world.trueX(flowerGridPos2[0]) && otherY == world.trueY(flowerGridPos2[1]) + 1)
            { toReturn = 6; }
            else
            { toReturn = 7; }
        }
        return toReturn;
    }

    public override void load()
    {
        if (PlayerPrefs.HasKey(WarpDriveController.instance.currentPlanetID + " " + flowerGridPos[0] + "x" + flowerGridPos[1] + "y_post"))
        {
            string key = PlayerPrefs.GetString(WarpDriveController.instance.currentPlanetID + " " + flowerGridPos[0] + "x" + flowerGridPos[1] + "y_post");
            for (int i = 0; i < 8; i++)
            {
                forcedConnectors[i] = key.ToCharArray()[i] == '1' ? true : false;
            }
        }
    }

    public override string save()
    {
        PlayerPrefs.DeleteKey(WarpDriveController.instance.currentPlanetID + " " + flowerGridPos[0] + "x" + flowerGridPos[1] + "y_post");
        PlayerPrefs.DeleteKey(WarpDriveController.instance.currentPlanetID + " " + flowerGridPos2[0] + "x" + flowerGridPos2[1] + "y_post");
        string toSet = "";
        for (int i = 0; i < 8; i++)
        {
            toSet += forcedConnectors[i] == true ? "1" : "0";
        }
        PlayerPrefs.SetString(WarpDriveController.instance.currentPlanetID + " " + flowerGridPos[0] + "x" + flowerGridPos[1] + "y_post", toSet);
        PlayerPrefs.SetString(WarpDriveController.instance.currentPlanetID + " " + flowerGridPos2[0] + "x" + flowerGridPos2[1] + "y_post", toSet);
        return base.save() + "|" + flowerGridPos[0] + "|" + flowerGridPos[1] + "|" + flowerGridPos2[0] + "|" + flowerGridPos2[1];
    }

    public override bool additionalLoad(string s)
    {
        //return true;
        string[] processed = s.Split('|');
        if (flowerGridPos[0] == int.Parse(processed[8])&& flowerGridPos[1] == int.Parse(processed[9]))
        {
            flowerGridPos2[0] = int.Parse(processed[10]);
            flowerGridPos2[1] = int.Parse(processed[11]);
            return true;
        }
        return false;
    }

    public override void init(int x, int y)
    {
        if (flowerGridPos[0] == -1)
        {
            flowerGridPos[0] = x;
            flowerGridPos[1] = y;
        }
        else if (flowerGridPos2[0] == -1)
        {
            flowerGridPos2[0] = x;
            flowerGridPos2[1] = y;
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
        transform.transform.localScale = new Vector3((scaleAmount * .7f) + .3f, scaleAmount, (scaleAmount * .7f) + .3f);
    }
}
