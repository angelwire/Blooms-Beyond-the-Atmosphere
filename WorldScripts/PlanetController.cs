using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    Material myMaterial;
    Material myEffectMaterial;
    Planet myPlanet;
    [SerializeField] int planetID;

    // Start is called before the first frame update
    void Awake()
    {
        myMaterial = GetComponent<MeshRenderer>().material;
        myEffectMaterial = transform.GetChild(0).GetComponent<MeshRenderer>().material;
    }

    public void setPlanetDarkness(float inDark)
    {
        Color newColor = myPlanet.getColor();
        Color newEffectColor = myEffectMaterial.GetColor("_Color");

        if (myPlanet != null)
        {
            newColor.a = 1.0f - inDark; 
            myMaterial.color = newColor;
            newEffectColor.a = 1.0f - inDark;
            myEffectMaterial.color = newEffectColor;
        }
    }

    public void setupPlanet(Planet inPlanet)
    {
        myPlanet = inPlanet;
        planetID = inPlanet.getID();
        myMaterial.color = myPlanet.getColor();
        myEffectMaterial.color = Color.Lerp(myPlanet.getColor(),Color.black,.5f);
    }

    public int getPlanetID()
    {
        if (myPlanet != null)
        {
            return myPlanet.getID();
        }
        else
        {
            return -1;
        }
    }

    public Planet getPlanet()
    {
        return myPlanet;
    }
}
