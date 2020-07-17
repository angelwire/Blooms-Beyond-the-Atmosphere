using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationTabSetter : MonoBehaviour
{
    private GameObject hbuic;
    // Start is called before the first frame update
    void Start()
    {
        //Get reference to HomeBaseUIController
        hbuic = GameObject.Find("HomeInventoryCanvas");
    }

    public void setTab(int tab)
    {
        if (hbuic != null)
        {
            hbuic.GetComponent<HomeBaseUIController>().setDecorationTabID(tab);
        }
    }
}
