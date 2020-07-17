using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesActionController : MonoBehaviour
{
    HomeBaseUIController hbuic;
    // Start is called before the first frame update
    void Start()
    {
        hbuic = GameObject.Find("HomeInventoryCanvas").GetComponent<HomeBaseUIController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void purchaseUpgrade()
    {
        hbuic.purchaseUpgrade(HomeBaseUIController.UpgradeType.Jetpack);
    }
}
