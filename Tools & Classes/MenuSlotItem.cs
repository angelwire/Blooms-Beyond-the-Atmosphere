using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// William Jones
/// 1-25-20
/// This is a class to handle all the interactions for items in the home base menu.
/// The items can be picked up and set down in "Menu slots"
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MenuSlotItem : MonoBehaviour
{
    public uint id;
    public int count;
    public bool isFlower;

    GameObject rootObject;
    bool isHeld;

    public void Update()
    {
        if (isHeld == true)
        {
            moveWithMouse();
        }   

        if (rootObject != null)
        {
            rootObject.transform.Rotate(0, 40 * Time.deltaTime, 0);
        }
    }

    //Called when the user clicks on it
    public void grab()
    {
        isHeld = true;
        GetComponent<LayoutElement>().ignoreLayout = true;
        GetComponent<RectTransform>().sizeDelta = new Vector2(0,0);
        transform.SetParent(transform.root);
    }
    
    //Called when the user lets go of the mouse (or clicks again)
    public void release()
    {
        isHeld = false;
        if (GetComponent<LayoutElement>() != null)
        {
            GetComponent<LayoutElement>().ignoreLayout = false;
        }
    }

    //Moves with the mouse if it is being held
    private void moveWithMouse()
    {
        RectTransform rTransform = GetComponent<RectTransform>();
        rTransform.anchoredPosition = new Vector2(Input.mousePosition.x - Screen.width * .5f, Input.mousePosition.y - Screen.height * .5f);
    }

    public void setID(uint id, bool isFlower)
    {
        this.id = id;
        this.isFlower = isFlower;
        buildModel();
    }

    public void setCount(int count)
    {
        this.count = count;
        gameObject.transform.GetComponentInChildren<Text>().text = count + "";
    }

    private void buildModel()
    {
        if (this.isFlower)
        {
            buildFlower(id);
        }
        else
        {
            buildDecoration(id);
        }
    }

    private void buildDecoration(uint inIndex)
    {
        FlowerProperties flowerProperties = GameObject.Find("World").GetComponent<FlowerProperties>();
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
        rootObject = Instantiate(flowerProperties.getDecorationObject(inIndex), transform);
        //The root object is a prefab not meant for UI so it needs to be fixed up
        fixDecorationForUI(rootObject);
    }

    private void fixDecorationForUI(GameObject decoration)
    {
        bool isAnimation;

        FlowerProperties flowerProperties = GameObject.Find("World").GetComponent<FlowerProperties>();
        //Pick out it's scale
        float itemScale = decoration.GetComponentInChildren<Placeable>().uiScale;

        //Make it part of the ui
        rootObject.layer = LayerMask.NameToLayer("UI");

        //Replace the shader so that it doesn't have the curvature
        Material oldMaterial;
        if (decoration.GetComponent<MeshRenderer>() != null)
        {
            oldMaterial = decoration.GetComponent<MeshRenderer>().material;
            isAnimation = false;
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

        //Reset the transform
        decoration.transform.localPosition = Vector3.zero;
        decoration.transform.localRotation = Quaternion.identity;

        if (!isAnimation) //If it's not a flag
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
        else //if it is a flag or a windmill
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

            //Add another post
            if (connectorPrefab != null)
            {
                decoration.transform.localPosition = new Vector3(0, 0, .5f);
                GameObject otherPost = GameObject.Instantiate(decoration, decoration.transform);
                otherPost.transform.localPosition = new Vector3(0, 0, -1f);
                GameObject connectorInstance = GameObject.Instantiate(connectorPrefab, decoration.transform);
                connectorInstance.transform.localPosition = Vector3.zero;
                DestroyImmediate(connectorInstance.GetComponent<connectorController>());
                connectorInstance.GetComponent<MeshRenderer>().material.shader = flowerProperties.decorationUIShader;
                connectorInstance.layer = LayerMask.NameToLayer("UI");
            }

        }

        //Now add the root object to the ui
        rootObject.transform.parent = gameObject.transform.GetChild(0);
        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localScale = Vector3.one * (itemScale * 80);

        //Add exceptions for the grass
        if (decoration.GetComponent<Placeable>().id == 100)
        {
            //Set the grass ui shader
            oldMaterial.shader = flowerProperties.grassUIShader;
            //Scale it up by a bit
            decoration.transform.localScale = Vector3.one + Vector3.up * 10;
        }

        //Give it a random rotation
        rootObject.transform.Rotate(0, Random.Range(0, 6.28f), 0);

        if (!isAnimation)
        {
            rootObject.GetComponent<MeshRenderer>().material.renderQueue += 2;
        }
        else
        {
            rootObject.GetComponentInChildren<SkinnedMeshRenderer>().material.renderQueue += 2;
        }

        updateModelScale();
    }

    //Builds a flower with the given index
    private void buildFlower(uint inIndex)
    {
        FlowerProperties flowerProperties = GameObject.Find("World").GetComponent<FlowerProperties>();

        //Set the indexes
        int stemIndex = flowerProperties.getStemIndexFromIndex(inIndex);
        int petalIndex = flowerProperties.getPetalIndexFromIndex(inIndex);
        int pistilIndex = flowerProperties.getPistilIndexFromIndex(inIndex);
        int leafIndex = flowerProperties.getLeafIndexFromIndex(inIndex);

        //Set the colors
        int stemColor = flowerProperties.getStemColorIndexFromIndex(inIndex);
        int leafColor = flowerProperties.getLeafColorIndexFromIndex(inIndex);
        int petalColor = flowerProperties.getPetalColorIndexFromIndex(inIndex);
        int pistilColor = flowerProperties.getPistilColorIndexFromIndex(inIndex);

        //Destroy the flower before building a new one
        //Destroy the stem that destroys everything
        if (rootObject != null)
        {
            GameObject.Destroy(rootObject);
            rootObject = null;
        }

        //Instantiate the given stem object
        rootObject = Instantiate(flowerProperties.flowerStem[stemIndex], transform.GetChild(0));
        rootObject.layer = LayerMask.NameToLayer("UI");
        Material stemMat = flowerProperties.stemMaterial;
        rootObject.GetComponent<MeshRenderer>().material = stemMat;
        rootObject.GetComponent<MeshRenderer>().material.color = flowerProperties.flowerColors[stemColor];

        //Add leaves and petal to the stem
        bool petalHasBeenFound = false;
        GameObject petalObject=null;
        foreach (Transform child in rootObject.transform)
        {
            if (!petalHasBeenFound && child.gameObject.name.ToString().Equals("spetal"))
            {
                //It's the petal so it has been found
                petalHasBeenFound = true;
                //Now spawn it
                petalObject = Instantiate(flowerProperties.flowerPetal[petalIndex], child);
                petalObject.layer = LayerMask.NameToLayer("UI");
                //Color the material and apply it
                Material petalMat = flowerProperties.petalMaterial;
                petalObject.GetComponent<MeshRenderer>().material = petalMat;
                petalObject.GetComponent<MeshRenderer>().material.color = flowerProperties.flowerColors[petalColor]; ;
            }
            else
            {
                //It's a leaf so spawn it
                GameObject leaf = Instantiate(flowerProperties.flowerLeaf[leafIndex], child);
                leaf.layer = LayerMask.NameToLayer("UI");
                //Color the material and apply it
                Material leafMat = flowerProperties.leafMaterial;
                leaf.GetComponent<MeshRenderer>().material = leafMat;
                leaf.GetComponent<MeshRenderer>().material.color = flowerProperties.flowerColors[leafColor];
            }
        }

        if (petalObject != null)
        {
            //Add pistil to the petal
            GameObject pistilObject = Instantiate(flowerProperties.flowerPistil[pistilIndex], petalObject.transform.GetChild(0));
            pistilObject.layer = LayerMask.NameToLayer("UI");
            //Color the material and apply it
            Material pistilMat = flowerProperties.pistilMaterial;
            pistilObject.GetComponent<MeshRenderer>().material = pistilMat;
            pistilObject.GetComponent<MeshRenderer>().material.color = flowerProperties.flowerColors[pistilColor];
        }

        //Now add the root object to the ui
        rootObject.transform.parent = gameObject.transform.GetChild(0);
        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localScale = Vector3.one * 80;

        //Give it a random rotation
        rootObject.transform.Rotate(0, Random.Range(0, 6.28f), 0);
    }
    
    public void updateModelScale()
    {
        Rect frame = gameObject.GetComponent<RectTransform>().rect;
        updateModelScale(frame.width, frame.height);
    }

    public void updateModelScale(float width, float height)
    {
        float boundsSize;
        if (rootObject.GetComponent<MeshFilter>() != null) //If it has a mesh filter
        {
            boundsSize = VectorUtilities.VectorMax(rootObject.GetComponent<MeshFilter>().mesh.bounds.size) * 100;
        }
        else
        {
            boundsSize = VectorUtilities.VectorMax(rootObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.bounds.size) * 80;
        }

        float frameSize = Mathf.Min(height, width);
        float scale = frameSize / boundsSize;

        rootObject.transform.localScale = Vector3.one * (scale * 40);

        //Add exceptions for the grass
        if (this.id == 100)
        {
            //Scale it on the Y axis by a factor of 10
            Vector3 scaleAmount = new Vector3(rootObject.transform.localScale.x, rootObject.transform.localScale.y * 10, rootObject.transform.localScale.z);
            rootObject.transform.localScale = scaleAmount;
        }
    }
}
