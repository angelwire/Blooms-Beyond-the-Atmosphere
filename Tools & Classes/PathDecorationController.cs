using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author Vance Howald
public class PathDecorationController : Placeable
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
        newFlower.GetComponent<PathDecorationController>().enabled = true;
        //newFlower.GetComponent<decorationController>().enabled = true;
        newFlower.transform.LookAt(transform.position);
        newFlower.transform.eulerAngles = new Vector3(0, 0, 0);
//        newFlower.transform.Rotate(new Vector3(0, 90, 0));
        newFlower.SetActive(true);
        newFlower.transform.localScale = Vector3.one;

        PathDecorationController currentFlowerObj = newFlower.GetComponent<PathDecorationController>();
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
        //transform.transform.localScale = new Vector3((scaleAmount * .7f) + .3f, scaleAmount, (scaleAmount * .7f) + .3f);
    }
}
