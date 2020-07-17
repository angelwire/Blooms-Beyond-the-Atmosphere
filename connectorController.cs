using UnityEngine;

public class connectorController : MonoBehaviour
{
    Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(10, 10, 10));
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        Vector3 camPos = mainCamera.transform.position;
        //Get the squared distance
        float xDifference = pos.x - camPos.x;
        float zDifference = pos.z - camPos.z;
        float distanceFromCamera = (xDifference * xDifference) + (zDifference * zDifference);

        //Scale the item so it slowly transitions into the view
        float maxFlowerDistance = 400;
        float minFlowerDistance = 100;
        float scaleAmount = Mathf.Clamp(1 - ((distanceFromCamera - minFlowerDistance) / (maxFlowerDistance - minFlowerDistance)), .01f, 1);
        transform.transform.localScale = new Vector3(1f, scaleAmount, 1f);
    }

}
