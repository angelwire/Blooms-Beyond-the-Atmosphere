using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterPopupController : MonoBehaviour
{
    HomeBaseUIController controller;
    GameObject gridPanel;

    // Start is called before the first frame update
    void Start()
    {
        //On start tell the controller to hide the panels
        controller = GameObject.Find("HomeInventoryCanvas").GetComponent<HomeBaseUIController>();        
        gridPanel = GameObject.Find("gridPanel");
        controller.hidePanelsForSearch();
    }

    /// <summary>
    /// Tells the flower page content controller to close the filter popup
    /// </summary>
    public void closePopup()
    {
        //When closing tell the controller to show the panels
        controller.setPanelsActive(true);
        GameObject.Destroy(gameObject);
    }

    /// <summary>
    /// Collects the search flower and sends it to the fpcc and then tells it to close the popup
    /// </summary>
    public void search()
    {
        uint searchFlower = getSearchFlower();
        uint searchMask = getSearchMask();
        controller.setSearchParameters(searchFlower, searchMask);
        closePopup();
    }

    private uint getSearchFlower()
    {
        //Get the absolute value of every grid item
        //if the value is -1 then the mask will ignore it anyway so getting the abs is fine
        //All other values will work
        int pistilValue = Mathf.Abs(gridPanel.transform.GetChild(0).Find("gridItem").GetComponent<FilterItemController>().getValue());
        int pistilColor = Mathf.Abs(gridPanel.transform.GetChild(1).Find("gridItem").GetComponent<FilterItemController>().getValue());
        int petalValue = Mathf.Abs(gridPanel.transform.GetChild(2).Find("gridItem").GetComponent<FilterItemController>().getValue());
        int petalColor = Mathf.Abs(gridPanel.transform.GetChild(3).Find("gridItem").GetComponent<FilterItemController>().getValue());
        int leafValue = Mathf.Abs(gridPanel.transform.GetChild(4).Find("gridItem").GetComponent<FilterItemController>().getValue());
        int leafColor = Mathf.Abs(gridPanel.transform.GetChild(5).Find("gridItem").GetComponent<FilterItemController>().getValue());
        int stemValue = Mathf.Abs(gridPanel.transform.GetChild(6).Find("gridItem").GetComponent<FilterItemController>().getValue());
        int stemColor = Mathf.Abs(gridPanel.transform.GetChild(7).Find("gridItem").GetComponent<FilterItemController>().getValue());

        //Pe_St_Le_Pi
        //Petal stem leaf pistil
        uint result = 0;
        result += (uint)petalValue;
        result *= 16;
        result += (uint)petalColor;
        result *= 16;
        result += (uint)stemValue;
        result *= 16;
        result += (uint)stemColor;
        result *= 16;
        result += (uint)leafValue;
        result *= 16;
        result += (uint)leafColor;
        result *= 16;
        result += (uint)pistilValue;
        result *= 16;
        result += (uint)pistilColor;

        return result;
    }

    private uint getSearchMask()
    {
        uint result = 0;

        /*
         * If a value is equal to -1 then set it to 0 (which is 0000 in binary)
         * If a value is not equal to -1 then set it to 15 (which is 1111 in binary)
         */

        int pistilValue = (gridPanel.transform.GetChild(0).Find("gridItem").GetComponent<FilterItemController>().getValue() == -1) ? 0: 15;
        int pistilColor = (gridPanel.transform.GetChild(1).Find("gridItem").GetComponent<FilterItemController>().getValue() == -1) ? 0 : 15;
        int petalValue = (gridPanel.transform.GetChild(2).Find("gridItem").GetComponent<FilterItemController>().getValue() == -1) ? 0 : 15;
        int petalColor = (gridPanel.transform.GetChild(3).Find("gridItem").GetComponent<FilterItemController>().getValue() == -1) ? 0 : 15;
        int leafValue = (gridPanel.transform.GetChild(4).Find("gridItem").GetComponent<FilterItemController>().getValue() == -1) ? 0 : 15;
        int leafColor = (gridPanel.transform.GetChild(5).Find("gridItem").GetComponent<FilterItemController>().getValue() == -1) ? 0 : 15;
        int stemValue = (gridPanel.transform.GetChild(6).Find("gridItem").GetComponent<FilterItemController>().getValue() == -1) ? 0 : 15;
        int stemColor = (gridPanel.transform.GetChild(7).Find("gridItem").GetComponent<FilterItemController>().getValue() == -1) ? 0 : 15;

        //Pe_St_Le_Pi
        //Petal stem leaf pistil
        result += (uint)petalValue;
        result *= 16;
        result += (uint)petalColor;
        result *= 16;
        result += (uint)stemValue;
        result *= 16;
        result += (uint)stemColor;
        result *= 16;
        result += (uint)leafValue;
        result *= 16;
        result += (uint)leafColor;
        result *= 16;
        result += (uint)pistilValue;
        result *= 16;
        result += (uint)pistilColor;

        return result;
    }
}
