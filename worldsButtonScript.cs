using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class worldsButtonScript : MonoBehaviour
{
    public void buttonClick(bool isSearchButton)
    {
        if (isSearchButton)
        {
            GameObject.Destroy(gameObject);

            //ask for list of worlds

            GameObject.FindGameObjectWithTag("findPlanetPrefabTag").GetComponent<onlineSearchController>().searchClicked();
        }
        else
        {
            GameObject.FindGameObjectWithTag("findPlanetPrefabTag").GetComponent<onlineSearchController>().buttonClicked(GetComponentInChildren<Text>().text);
        }
    }
}
