using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPageContentController : MonoBehaviour
{
    HomeBaseUIController controller;
    [SerializeField] private GameObject filterPopupPrefab;
    private GameObject filterPopupInstance;
    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.Find("HomeInventoryCanvas").GetComponent<HomeBaseUIController>();
    }

    public void nextPage()
    {
        controller.FlowerPageNext();
    }
    public void prevPage()
    {
        controller.FlowerPagePrevious();
    }
   
    public void openFilter()
    {
        controller.dropItem();
        //Instantiate a popupFilterPrefab
        filterPopupInstance = Instantiate(filterPopupPrefab, controller.transform);
        //On Start the prefab will perform the necessary setup
    }
}
