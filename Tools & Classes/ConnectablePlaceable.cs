using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author William Jones
public abstract class ConnectablePlaceable : Placeable
{
    public List<GameObject> connectorList = new List<GameObject>();
    public bool[] forcedConnectors = { false, false, false, false, false, false, false, false };

    /// <summary>
    /// Checks the 4 adjacent cells for fence posts and adds a connector
    /// </summary>
    public abstract void updateConnectors(bool updateForce=true);

    /// <summary>
    /// Builds a prefab to connect the current placeable to the given placeable
    /// </summary>
    /// <param name="connectPlaceable"></param>
    /// <param name="angle"></param>
    public abstract void hardConnectToPlaceable(Placeable connectPlaceable, float angle, int forcedConnectorIndex);

    /// <summary>
    /// Connects the given post to the current post
    /// (only by adding a reference to the connector, does not instantiate a new object)
    /// </summary>
    /// <param name="connector"></param>
    public abstract void softConnectToPlaceable(GameObject connector);

    /// <summary>
    /// Clears all connectors connected to this post
    /// </summary>
    public abstract void clearConnectors();

    /// <summary>
    /// Removes all null references from the list
    /// </summary>
    public abstract void cleanConnectorList();

    /// <summary>
    /// For loding overide connectors
    /// </summary>
    public abstract void load();
}
