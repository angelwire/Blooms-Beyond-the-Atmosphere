//created: 10-4-19
//updated: 11-14-19
//version: .02
//Sets the flower's position depending on the curvature
//Author William Jones
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setCurvaturePosition : MonoBehaviour
{
    //The base position of the flower before curvature is added
    public float baseYPosition = 0;
    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        //When terrain is implemented, set the baseYPosition
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        Vector3 camPos = mainCamera.transform.position;
        //Vector3 camPos = Camera.main.transform.position;
        //Get the squared distance
        float xDifference = pos.x - camPos.x;
        float zDifference = pos.z - camPos.z;
        float distanceFromCamera = (xDifference * xDifference) + (zDifference * zDifference);
        //Move the flower
        transform.position = new Vector3(pos.x, baseYPosition - (distanceFromCamera * WorldManager.instance.getWorldCurvature()), pos.z);

        //Scale the flower so it slowly transitions into the view
        float maxFlowerDistance = 350;
        float minFlowerDistance = 300;
        if (distanceFromCamera > minFlowerDistance)
        {
            float scaleAmount = Mathf.Clamp(1 - ((distanceFromCamera - minFlowerDistance) / (maxFlowerDistance - minFlowerDistance)), .01f, 1);
            transform.transform.localScale = new Vector3((scaleAmount * .5f) + .5f, scaleAmount, (scaleAmount * .5f) + .5f);
        }
    }
}
