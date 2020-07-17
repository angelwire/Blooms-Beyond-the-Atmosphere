using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilterItemController : MonoBehaviour
{
    int currentItem = 0;
    //Petal_Stem_Leaf_Pistil
    [SerializeField] int Pe_St_Le_Pi = 0; //Use 0-3 for flower part types and 4-7 for color
    GameObject rootObject = null;
    GameObject rootImage = null;
    Mesh questionMarkMesh;
    FlowerProperties fp;
    float rotationSpeed = 40;

    // Start is called before the first frame update
    void Start()
    {
        rootObject = transform.Find("meshItem").gameObject;
        rootImage = transform.Find("colorImage").gameObject;
        questionMarkMesh = rootObject.GetComponent<MeshFilter>().sharedMesh;
        fp = GameObject.Find("World").GetComponent<FlowerProperties>();
        setCurrentItem(-1);
        if (Pe_St_Le_Pi < 4)
        {
            //disable image
            rootImage.SetActive(false);
        }
        else
        {
            fitModelToBounds();
        }
    }

    // Update is called once per frame
    void Update()
    {
        rootObject.transform.RotateAround(transform.position, transform.up, rotationSpeed * Time.deltaTime);  
    }

    public void nextItem()
    {
        currentItem += 1;
        if (currentItem > 15)
        {
            currentItem = -1;
        }

        //Loop through all the items to find the next one that has been discovered
        if (Pe_St_Le_Pi < 4)
        {
            while (currentItem != -1 && !HomeInventoryData.instance.knownParts[Pe_St_Le_Pi, currentItem])
            {
                currentItem += 1;
                if (currentItem > 15)
                {
                    currentItem = -1;
                }
            }
        }
        else
        {
            while (currentItem != -1 && !HomeInventoryData.instance.knownColors[Pe_St_Le_Pi-4, currentItem])
            {
                currentItem += 1;
                if (currentItem > 15)
                {
                    currentItem = -1;
                }
            }
        }

        setCurrentItem(currentItem);
    }

    public void previousItem()
    {
        currentItem -= 1;
        if (currentItem < -1)
        {
            currentItem = 15;
        }

        //Loop through all the items to find the next one that has been discovered
        if (Pe_St_Le_Pi < 4)
        {
            while (currentItem != -1 && !HomeInventoryData.instance.knownParts[Pe_St_Le_Pi, currentItem])
            {
                currentItem -= 1;
                if (currentItem < -1)
                {
                    currentItem = 15;
                }
            }
        }
        else
        {
            while (currentItem != -1 && !HomeInventoryData.instance.knownColors[Pe_St_Le_Pi - 4, currentItem])
            {
                currentItem -= 1;
                if (currentItem < -1)
                {
                    currentItem = 15;
                }
            }
        }

        setCurrentItem(currentItem);
    }

    public void setCurrentItem(int id)
    {
        currentItem = id;

        //If the item is set to a flower part model
        if (Pe_St_Le_Pi < 4)
        {
            if (currentItem == -1)
            {
                rootObject.GetComponent<MeshFilter>().mesh = questionMarkMesh;
            }
            else
            {
                Mesh changeTo;
                switch (Pe_St_Le_Pi)
                {
                    case 0: changeTo = fp.flowerPetal[currentItem].GetComponent<MeshFilter>().sharedMesh; break;
                    case 1: changeTo = fp.flowerStem[currentItem].GetComponent<MeshFilter>().sharedMesh; break;
                    case 2: changeTo = fp.flowerLeaf[currentItem].GetComponent<MeshFilter>().sharedMesh; break;
                    case 3: changeTo = fp.flowerPistil[currentItem].GetComponent<MeshFilter>().sharedMesh; break;
                    default: changeTo = questionMarkMesh; break;
                }
                rootObject.GetComponent<MeshFilter>().mesh = changeTo;
            }

            fitModelToBounds();
        }
        else //If the item is set to a color
        {
            if (currentItem == -1)
            {
                rootObject.SetActive(true);
                rootImage.SetActive(false);
            }
            else
            {
                rootObject.SetActive(false);
                rootImage.SetActive(true);
                Color setColor = fp.flowerColors[currentItem];
                setColor.a = 1;
                rootImage.GetComponent<Image>().color = setColor;
            }
        }

    }

    public void fitModelToBounds()
    {
        centerMeshByBounds(rootObject.GetComponent<MeshFilter>().mesh);
        updateModelScale(100, 100);
    }

    public void updateModelScale(float width, float height)
    {
        float boundsSize = VectorUtilities.VectorMax(rootObject.GetComponent<MeshFilter>().mesh.bounds.size) * 100;
        float frameSize = Mathf.Min(height, width);
        float scale = frameSize / boundsSize;

        rootObject.transform.localScale = Vector3.one * (scale * 40);
    }

    public int getValue()
    {
        return currentItem;
    }

    //Center's a mesh around the average vertex position
    private void centerMeshByAverage(Mesh m)
    {
        //Create a mutable copy of the mesh vertices
        Vector3[] newVertices = m.vertices;
        //Initialize the average
        Vector3 average = new Vector3(0, 0, 0);
        //Loop through the original vertices
        foreach (Vector3 v in m.vertices)
        {
            //Increment the average
            average += v;
        }
        //Divide the total sum by the count
        average /= m.vertices.Length;

        //Average found so loop through the mutable values
        for (int i= 0; i < newVertices.Length; i++)
        {
            //Decrement the values based on the average
            newVertices[i] -= average;
        }
        //Set the vertices
        m.SetVertices(new List<Vector3>(newVertices));
    }

    //Center's a mesh around the bound center
    private void centerMeshByBounds(Mesh m)
    {
        //Create a mutable copy of the mesh vertices
        Vector3[] newVertices = m.vertices;

        //Average found so loop through the mutable values
        for (int i = 0; i < newVertices.Length; i++)
        {
            //Decrement the values based on the average
            newVertices[i] -= m.bounds.center;
        }
        //Set the vertices
        m.SetVertices(new List<Vector3>(newVertices));
    }
}
