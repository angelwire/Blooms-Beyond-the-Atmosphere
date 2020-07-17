using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//updated: 10-31-19
//version: .01
//A controller for the clouds
//Author: William Jones
public class CloudControl : MonoBehaviour
{
    float position;
    public float animationSpeed;
    public Mesh[] cloudMeshes;

    private float baseYPosition;
    Camera mainCamera;

    //Reset the cloud and then give it a random texture position
    private void Start()
    {
        resetCloud();
        position = (Random.Range(0, 20) * .05f)-.5f;
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //Move the texture position depending on the animation speed
        position += animationSpeed * Time.deltaTime;
        Material mat = GetComponent<MeshRenderer>().material;
        mat.SetTextureOffset("_MainTex", new Vector2(0, position));
        if (position >= .5f)
        {
            //Reset the cloud if the textrue position is more than half
            //(the cloud is invisible at this point)
            resetCloud();
        }
    }

    //Resets the cloud position and chooses a random mesh
    void resetCloud()
    {
        //Reset the texture position
        position = -.5f;

        //Choose random cloud mesh
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = cloudMeshes[Random.Range(0, cloudMeshes.Length)];

        //Move to a new random position
        SkyController skyScript = transform.GetComponentInParent<SkyController>();
        float skySize = skyScript.skySize;
        float skyDepth = skyScript.skyDepth;
        float cloudHeight = skyScript.cloudHeight;

        transform.localPosition = new Vector3(Random.Range(-skySize, skySize),
            cloudHeight + Random.Range(-skyDepth, skyDepth),
            Random.Range(-skySize, skySize));

        baseYPosition = transform.position.y;
        setCurvaturePosition();
    }
    
    //Do some curvature stuff so clouds will be in the horizon
    private void setCurvaturePosition()
    {
        mainCamera = Camera.main;
        float xDifference = transform.position.x - mainCamera.transform.position.x;
        float zDifference = transform.position.z - mainCamera.transform.position.z;
        float distanceFromCamera = (xDifference * xDifference) + (zDifference * zDifference);
        transform.transform.position = new Vector3(transform.position.x, baseYPosition - (distanceFromCamera * WorldManager.instance.getWorldCurvature()), transform.position.z);
    }
}
