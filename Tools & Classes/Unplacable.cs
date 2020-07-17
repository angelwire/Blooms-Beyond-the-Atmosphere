using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Author Vance Howald
public class Unplacable : Placeable
{
    public override bool canBePlaced(RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }

    public override void clearPos()
    {
        throw new System.NotImplementedException();
    }

    public override GameObject cloneObj(GameObject model, Placeable data, RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }

    public override Vector3 posClamp(RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }

    public override void setHover(bool hover)
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
