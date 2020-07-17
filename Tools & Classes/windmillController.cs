using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author Vance Howald
public class windmillController : Placeable
{
    Camera mainCamera;

    SkinnedMeshRenderer wheelMeshRenderer;
    GameObject wheelMeshObject;

    int[] flowerGridPos2 = { 0, 0 };//this needs to be fixed but rn is needed for the object to disapear

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
                x2++;
            }
            else
            {
                x2--;
            }
        }
        else
        {
            if (distanceFromCenter.z > 0)
            {
                y2++;
            }
            else
            {
                y2--;
            }
        }
        x2 = WorldManager.instance.trueX(x2);
        y2 = WorldManager.instance.trueX(y2);
        return (WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]] == null || WorldManager.instance.placableGrid[flowerGridLoc[0], flowerGridLoc[1]].isPlaceable()) && (WorldManager.instance.placableGrid[x2, y2] == null || WorldManager.instance.placableGrid[x2, y2].isPlaceable());
    }

    public override GameObject cloneObj(GameObject model, Placeable data, RaycastHit hit)
    {
        GameObject newFlower = GameObject.Instantiate(model, posClamp(hit), Quaternion.identity, hit.transform);//make a copy
        //newFlower.name = x + " " + y;//placement stuff
        newFlower.transform.LookAt(transform.position);
        int yRot = (int)((mainCamera.transform.parent.eulerAngles.y + 225) / 90);
        newFlower.transform.eulerAngles = new Vector3(0, yRot * 90, 0);
        //        newFlower.transform.Rotate(new Vector3(0, 90, 0));
        newFlower.SetActive(true);
        newFlower.transform.localScale = Vector3.one;

        windmillController currentFlowerObj = newFlower.GetComponent<windmillController>();

        int[] flowerGridLoc = WorldManager.instance.worldPointToFlowerGrid(hit.point);//find what grid the point is in
        float chunkOverFlower = (float)WorldManager.instance.chunkWidth / WorldManager.instance.FlowerWidthPerChunk;
        Vector3 flowerCellCenterPosition = VectorUtilities.VectorAdd((VectorUtilities.VectorFloor(hit.point * chunkOverFlower) / chunkOverFlower), (chunkOverFlower * .5f));
        int x2 = flowerGridLoc[0];
        int y2 = flowerGridLoc[1];
        WorldManager.instance.placableGrid[x2, y2] = currentFlowerObj;//move the flower data over
        Vector3 distanceFromCenter = hit.point - flowerCellCenterPosition;
        if (yRot % 2 == 0)
        {
            if (distanceFromCenter.x > 0)
            {
                x2++;
            }
            else
            {
                x2--;
            }
        }
        else
        {
            if (distanceFromCenter.z > 0)
            {
                y2++;
            }
            else
            {
                y2--;
            }
        }
        x2 = WorldManager.instance.trueX(x2);
        y2 = WorldManager.instance.trueX(y2);
        WorldManager.instance.removeFromFlowerList(x2, y2);
        WorldManager.instance.placableGrid[x2, y2] = currentFlowerObj;//move the flower data over
        currentFlowerObj.flowerGridPos2 = new int[] { x2, y2 };
        currentFlowerObj.flowerGridPos = flowerGridLoc;
        return newFlower;
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

    // Start is called before the first frame update
    void Start()
    {
        wheelMeshObject = transform.Find("windmillBase").gameObject;
        wheelMeshRenderer = wheelMeshObject.GetComponent<SkinnedMeshRenderer>();

        //Set the bounds so it won't get clipped prematurely
        wheelMeshRenderer.localBounds = new Bounds(Vector3.zero, Vector3.one * 100);

        mainCamera = Camera.main;
    }


    public void updateFlagMeshObject()
    {
        wheelMeshObject = transform.Find("windmillBase").gameObject;
        wheelMeshRenderer = wheelMeshObject.GetComponent<SkinnedMeshRenderer>();

        //Set the bounds so it won't get clipped prematurely
        //flagMeshRenderer.sharedMesh.bounds = new Bounds(Vector3.zero, flagMeshRenderer.sharedMesh.bounds.size * 100);
        wheelMeshRenderer.localBounds = new Bounds(Vector3.zero, Vector3.one * 100);
    }

    //Sets the hover bool
    public override void setHover(bool hover)
    {
        if (wheelMeshObject == null)
        {
            updateFlagMeshObject();
        }

        if (hover != isHovered)
        {
            isHovered = hover;
            //Set the layer to the effects layer if it's hovered
            if (hover)
            {
                gameObject.layer = LayerMask.NameToLayer("OutlineEffects");
                wheelMeshObject.layer = LayerMask.NameToLayer("OutlineEffects");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Placeable");
                wheelMeshObject.layer = LayerMask.NameToLayer("Placeable");
            }
        }
    }

    public override void clearPos()
    {
        WorldManager.instance.placableGrid[flowerGridPos[0], flowerGridPos[1]] = null;
        WorldManager.instance.updateFlowerPos(flowerGridPos[0], flowerGridPos[1]);
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
        float maxFlowerDistance = 400;
        float minFlowerDistance = 100;
        float scaleAmount = Mathf.Clamp(1 - ((distanceFromCamera - minFlowerDistance) / (maxFlowerDistance - minFlowerDistance)), .01f, 1);
        transform.transform.localScale = new Vector3((scaleAmount * .7f) + .3f, scaleAmount, (scaleAmount * .7f) + .3f);
    }

    public override string save()
    {
        return base.save() + "|" + flowerGridPos[0] + "|" + flowerGridPos[1] + "|" + flowerGridPos2[0] + "|" + flowerGridPos2[1];
    }

    public override bool additionalLoad(string s)
    {
        //return true;
        string[] processed = s.Split('|');
        if (flowerGridPos[0] == int.Parse(processed[8]) && flowerGridPos[1] == int.Parse(processed[9]))
        {
            flowerGridPos2[0] = int.Parse(processed[10]);
            flowerGridPos2[1] = int.Parse(processed[11]);
            return true;
        }
        return false;
    }
}
