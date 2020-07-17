//updated: 11-21-19
//version: .03
//Author William Jones
/*
 * Controls the UI flowers
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFlowerScript : MonoBehaviour
{
    //Whether or not the uiFlower is a flower or not
    bool isFlower;

    public float rootScale = 1;

    //Values between 0-15 for each element
    int petalIndex = 0;
    int stemIndex = 0;
    int pistilIndex = 0;
    int leafIndex = 0;

    //Values between 0-15 for each element
    int petalColorIndex = 0;
    int stemColorIndex = 0;
    int pistilColorIndex = 0;
    int leafColorIndex = 0;

    //How fast to rotate
    public float rotationSpeed = 2;

    //How much to pop up on the scroll
    float popupMax = 40;
    float popupTo = 0;
    float popupSpeed = 8f;
    float speedOffset = 4f;

    //Whether or not the player is currently using Control to view the ui flower
    public bool isViewing = false;
    public bool isDoneViewing = false;
    public Vector3 goToLocation;
    public Quaternion goToRotation;
    public Vector3 goToScale;
    public Vector3 returnScale;
    public float viewingSpeed = 7;

    private FlowerProperties flowerProperties;

    // Start is called before the first frame update
    void Start()
    {
        //Scale it by 100 to get the right size
        transform.localScale = new Vector3(100 * transform.localScale.x, 100 * transform.localScale.y, 100 * transform.localScale.z);
        rootObject = transform.GetChild(0).gameObject;

        //Get the scale it's supposed to return to when done viewing
        returnScale = transform.localScale;

        //Get the properties object
        flowerProperties = GameObject.Find("World").GetComponent<FlowerProperties>();

        hideFlower();
    }

    //Updates
    private void Update()
    {
        //The "target" fps to make it spin as fast as it did before adding "time.deltaTime"
        float fpsTarget = 60;

        //If the flower is NOT being viewed
        if (!isViewing)
        {
            //Spin
            transform.Rotate(0, rotationSpeed * Time.deltaTime * fpsTarget, 0);

            float deltaY = popupSpeed;
            if (transform.localPosition.y > popupTo)
            {
                deltaY = (-popupSpeed + speedOffset) * Time.deltaTime * fpsTarget;
            }

            //Popup when the user scrolls to it
            if (Mathf.Abs(transform.localPosition.y - popupTo) > Mathf.Abs(deltaY))
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + deltaY, transform.localPosition.z);
            }
            else
            {
                transform.localPosition = new Vector3(transform.localPosition.x, popupTo, transform.localPosition.z);
            }
        }
        else if (rootObject != null)//If the flower is being viewed and there is a placeable to view
        {
            //Set the offset to center the flower's rotation axis
            float yOffset;
            if (!isDoneViewing)
            {
                if (transform.GetChild(0).gameObject.GetComponent<MeshFilter>() != null)
                {
                    yOffset = (transform.GetChild(0).gameObject.GetComponent<MeshFilter>().mesh.bounds.size.y * .5f) * rootScale;
                }
                else
                {
                    yOffset = (transform.GetChild(0).gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.bounds.size.y * .5f) * rootScale;
                }
            }
            else
            {
                yOffset = 0;
            }
            //If there's a stem to move then move it
            if (transform.childCount > 0)
            {
                transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, new Vector3(0, -yOffset, 0), Time.deltaTime * viewingSpeed);
            }

            //Go to where the UIController told it to go
            transform.localPosition = Vector3.Lerp(transform.localPosition, goToLocation, Time.deltaTime * viewingSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, goToScale, Time.deltaTime * viewingSpeed);

            //If it's done viewing then lerp to the goto location
            if (isDoneViewing)
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, goToRotation, Time.deltaTime * viewingSpeed);
            }

            //If it's at the position
            if (isDoneViewing && Vector3.Distance(transform.localPosition, goToLocation) < 1f)
            {
                isViewing = false;
                transform.localPosition = goToLocation;
                transform.localScale = goToScale;
                transform.localRotation = goToRotation;
                if (transform.childCount > 0)
                {
                    transform.GetChild(0).localPosition = Vector3.zero;
                }
            }
        }
    }

    //The objects for each flower elements
    GameObject petalObject, rootObject, pistilObject, decorationObject;
    List<GameObject> leafObjects = new List<GameObject>();

    //Sets the color of the flower depending on the given indexes
    public void colorFlower(int inStem, int inLeaf, int inPetal, int inPistil)
    {
        isFlower = true;
        stemColorIndex = inStem;
        leafColorIndex = inLeaf;
        petalColorIndex = inPetal;
        pistilColorIndex = inPistil;
        rootObject.GetComponent<MeshRenderer>().material.color = flowerProperties.flowerColors[inStem];
        pistilObject.GetComponent<MeshRenderer>().material.color = flowerProperties.flowerColors[inPistil];
        petalObject.GetComponent<MeshRenderer>().material.color = flowerProperties.flowerColors[inPetal];
        foreach (GameObject leaf in leafObjects)
        {
            leaf.GetComponent<MeshRenderer>().material.color = flowerProperties.flowerColors[inLeaf];
        }
    }

    //Builds a flower with the given index for each element
    public void buildFlower(int inStem, int inLeaf, int inPetal, int inPistil)
    {
        if (flowerProperties == null)
        {
            flowerProperties = GameObject.Find("World").GetComponent<FlowerProperties>();
        }

        isFlower = true;
        //Set the indexes
        stemIndex = inStem;
        petalIndex = inPetal;
        pistilIndex = inPistil;
        leafIndex = inLeaf;

        //Destroy the flower before building a new one
        //Destroy the stem that destroys everything
        if (rootObject != null)
        {
            GameObject.Destroy(rootObject);
            rootObject = null;
        }
        //Clear the leaves
        leafObjects.Clear();

        //Instantiate the given stem object
        rootObject = Instantiate(getStemObjectFromIndex(inStem), transform);
        rootObject.layer = LayerMask.NameToLayer("UI");
        Material stemMat = flowerProperties.stemMaterial;
        rootObject.GetComponent<MeshRenderer>().material = stemMat;

        //Add leaves and petal to the stem
        bool petalHasBeenFound = false;
        foreach (Transform child in rootObject.transform)
        {
            if (!petalHasBeenFound && child.gameObject.name.ToString().Equals("spetal"))
            {
                //It's the petal so it has been found
                petalHasBeenFound = true;
                //Now spawn it
                petalObject = Instantiate(getPetalObjectFromIndex(inPetal), child);
                petalObject.layer = LayerMask.NameToLayer("UI");
                Material petalMat = flowerProperties.petalMaterial;
                petalObject.GetComponent<MeshRenderer>().material = petalMat;
            }
            else
            {
                //It's a leaf so spawn it
                GameObject leaf = Instantiate(getLeafObjectFromIndex(inLeaf), child);
                leaf.layer = LayerMask.NameToLayer("UI");
                Material leafMat = flowerProperties.leafMaterial;
                leaf.GetComponent<MeshRenderer>().material = leafMat;
                leafObjects.Add(leaf);
            }
        }

        //Add pistil to the petal
        pistilObject = Instantiate(getPistilObjectFromIndex(inPistil), petalObject.transform.GetChild(0));
        pistilObject.layer = LayerMask.NameToLayer("UI");
        Material pistilMat = flowerProperties.pistilMaterial;
        pistilObject.GetComponent<MeshRenderer>().material = pistilMat;
    }

    /// <summary>
    /// Builds the decoration for the UI with the given id (in long form)
    /// </summary>
    /// <param name="inID"></param>
    /// <returns></returns>
    public GameObject buildDecoration(long inID)
    {
        /*
         * Decoration ID codes:
         * (all decorations should be negative and should never have an ID of 0)
         * (flower ids take up the entire positive range of longs (including 0))
         * 0 ... 9: Japanese style path, bench, fence, archway, and then some extra IDs for padding in case more need added
         * 10...19: Urban style path, bench, fence, archway...
         * 20...29: Suburban style- path, bench, fence, archway...
         * 30...99: Padding for other styles that might be added in the future
         * 100+... misc IDs
         */
        isFlower = false;

        //Destroy the flower or decoration before building a new one
        //Destroy the root destroys everything
        if (rootObject != null)
        {
            GameObject.Destroy(rootObject);
            rootObject = null;
        }


        rootObject = Instantiate(getDecorationObjectFromIndex((uint)inID), transform);
        //The root object is a prefab not meant for UI so it needs to be fixed up
        fixDecorationForUI(rootObject);
        return rootObject;
    }

    /// <summary>
    /// Takes the decoration gameobject and fixes it to be used in the UI
    /// </summary>
    /// <param name="decoration"></param>
    private void fixDecorationForUI(GameObject decoration)
    {
        bool isAnimation = false;

        //Pick out it's scale
        float itemScale = decoration.GetComponent<Placeable>().uiScale;
        //Make it part of the ui
        rootObject.layer = LayerMask.NameToLayer("UI");

        //Replace the shader so that it doesn't have the curvature
        Material oldMaterial;
        if (decoration.GetComponent<MeshRenderer>() != null)
        {
            oldMaterial = decoration.GetComponent<MeshRenderer>().material;
        }
        else
        {
            oldMaterial = decoration.GetComponentInChildren<SkinnedMeshRenderer>().material;

            //Check for flag or windmill
            if (rootObject.transform.Find("flagMesh") != null)
            {
                GameObject flagObject = rootObject.transform.Find("flagMesh").gameObject;
                flagObject.layer = LayerMask.NameToLayer("UI");
                flagObject.transform.localPosition = Vector3.zero;
                flagObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                GameObject windmillObject = rootObject.transform.Find("windmillBase").gameObject;
                windmillObject.layer = LayerMask.NameToLayer("UI");
                windmillObject.transform.localPosition = Vector3.zero;
                windmillObject.transform.localRotation = Quaternion.identity;
            }

            isAnimation = true;
        }


        oldMaterial.shader = flowerProperties.decorationUIShader;
        if (decoration.GetComponent<Placeable>().id == 100)
        {
            oldMaterial.shader = flowerProperties.grassUIShader;
        }

        //Reset the transform
        decoration.transform.localPosition = Vector3.zero;
        decoration.transform.localRotation = Quaternion.identity;

        if (!isAnimation) //If it's not a flag or windmill
        {
            //Remove all children from the gameobject (for the bench to remove the prompt)
            while (decoration.transform.childCount > 0)
            {
                GameObject.DestroyImmediate(decoration.transform.GetChild(0).gameObject);
            }

            //Only need the MeshFilter, MeshRenderer, and Transform so remove everything else
            foreach (Component cc in decoration.GetComponents<Component>())
            {
                if (cc.GetType() != typeof(MeshFilter) && cc.GetType() != typeof(MeshRenderer) && cc.GetType() != typeof(Transform))
                {
                    Destroy(cc);
                }
            }
        }
        else //If it is a flag
        {
            foreach (FlagDecorationController cc in decoration.GetComponents<FlagDecorationController>())
            {
                Destroy(cc);
            }
            foreach (windmillController cc in decoration.GetComponents<windmillController>())
            {
                Destroy(cc);
            }
        }

        //If it's a fence post then add another fence post and connect them
        if (decoration.name.Contains("post"))
        {
            //Get the fence connector
            string connectorStyle = "";
            if (decoration.name.Contains("Japanese")) { connectorStyle = "Japanese"; }
            if (decoration.name.Contains("Urban")) { connectorStyle = "Urban"; }
            if (decoration.name.Contains("Suburban")) { connectorStyle = "Suburban"; }
            GameObject connectorPrefab = Resources.Load("Prefabs/Decorations/" + "connector" + connectorStyle + "Prefab") as GameObject;
            if (connectorPrefab != null)
            {
                decoration.transform.localPosition = new Vector3(0, 0, .5f * itemScale);
                GameObject otherPost = GameObject.Instantiate(decoration, decoration.transform);
                otherPost.transform.localPosition = new Vector3(0, 0, -1f);
                GameObject connectorInstance = GameObject.Instantiate(connectorPrefab, decoration.transform);
                DestroyImmediate(connectorInstance.GetComponent<connectorController>());
                connectorInstance.transform.localPosition = Vector3.zero;
                connectorInstance.GetComponent<MeshRenderer>().material.shader = flowerProperties.decorationUIShader;
                connectorInstance.layer = LayerMask.NameToLayer("UI");
            }

        }
    }

    public GameObject getRootObject()
    {
        return rootObject;
    }

    //Getters:
    public GameObject getStemObjectFromIndex(int inStem)
    {
        //print(inStem);
        return flowerProperties.flowerStem[inStem];
    }
    public GameObject getPetalObjectFromIndex(int inPetal)
    {
        //print(inPetal);
        return flowerProperties.flowerPetal[inPetal];
    }
    public GameObject getLeafObjectFromIndex(int inLeaf)
    {
        //print(inLeaf);
        return flowerProperties.flowerLeaf[inLeaf];
    }
    public GameObject getPistilObjectFromIndex(int inPistil)
    {
        //print(inPistil);
        return flowerProperties.flowerPistil[inPistil];
    }
    public GameObject getDecorationObjectFromIndex(uint index)
    {
        if (flowerProperties == null)
        {
            flowerProperties = GameObject.Find("World").GetComponent<FlowerProperties>();
        }
        return flowerProperties.getDecorationObject(index);
    }

    //Tell the ui flower to pop up or pop down (when the user scrolls through inventory slots)
    public void popup() { popupTo = popupMax; }
    public void popdown() { popupTo = 0; }

    /// <summary>
    /// Returns the long form index of the flower
    /// </summary>
    /// <returns>Flower long form index</returns>
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

    /// <summary>
    /// Destroys the stem if there's one to destroy
    /// </summary>
    public void hideFlower()
    {
        if (rootObject != null)
        {
            GameObject.Destroy(rootObject);
        }
        else
        {
            if (transform.childCount > 0)
            {
                GameObject.Destroy(transform.GetChild(0).gameObject);
            }
        }
    }

    public void fixBrokenFlower()
    {
        if(rootObject == null)
        {
            if (transform.childCount > 0)
            {
                rootObject = transform.GetChild(0).gameObject;
            }
        }
    }
}
