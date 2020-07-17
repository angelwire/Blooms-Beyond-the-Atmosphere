using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpaceStationPointerController : MonoBehaviour
{
    [SerializeField] GameObject pointerImage;
    List<Planet> visitablePlanets;
    List<GameObject> imageInstances;
    Vector3 currentPosition;
    Camera mainCamera;
    Canvas mainCanvas;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        mainCanvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
        currentPosition = WarpDriveController.instance.GetCurrentPlanet().getGalacticPosition();
        setupInstances();
    }

    private void OnEnable()
    {
        setupInstances();
    }

    public void setupInstances()
    {
        Debug.Log("setupInstances");

        if (visitablePlanets == null)
        {
            visitablePlanets = new List<Planet>();
        }
        if (imageInstances == null)
        {
            imageInstances = new List<GameObject>();
        }

        for (int i = 0; i < imageInstances.Count; i++)
        {
            Destroy(imageInstances[i]);
        }

        visitablePlanets.Clear();
        imageInstances.Clear();

        for (int i = 0; i < 16; i++)
        {
            visitablePlanets.Add(Galaxy.instance.planetList[i]);
            GameObject imageInstance = Instantiate(pointerImage, transform);
            imageInstances.Add(imageInstance);
            if (Galaxy.instance.planetList[i].planetIsLocked())
            {
                imageInstance.SetActive(false);
            }

            if (Galaxy.instance.planetList[i].planetIsVisited())
            {
                imageInstance.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        mainCamera = Camera.main;
        for (int i = 0; i < visitablePlanets.Count; i++)
        {
            Vector3 planetRelativePosition = visitablePlanets[i].getGalacticPosition() + mainCamera.transform.position - currentPosition;
            Vector3 planetScreenPoint = mainCamera.WorldToScreenPoint(planetRelativePosition);
            Vector2 flatScreenPoint = new Vector2(planetScreenPoint.x, planetScreenPoint.y);


            if (planetScreenPoint.z < 0 || i == WarpDriveController.instance.currentPlanetID)
            {
                imageInstances[i].GetComponent<Image>().enabled = false;
                imageInstances[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
            }
            else
            {
                imageInstances[i].GetComponent<Image>().enabled = true;
                imageInstances[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
            }

            Vector2 screenMiddle = new Vector2(Screen.width * .5f, Screen.height * .5f);
            Vector2 pointToPosition = (flatScreenPoint - screenMiddle) / mainCanvas.scaleFactor;

            pointToPosition.x = Mathf.Clamp(pointToPosition.x * mainCanvas.scaleFactor, -Screen.width * .5f, Screen.width * .5f) / mainCanvas.scaleFactor;
            pointToPosition.y = Mathf.Clamp(pointToPosition.y * mainCanvas.scaleFactor, -Screen.height * .5f, Screen.height * .5f) / mainCanvas.scaleFactor;

            imageInstances[i].GetComponent<RectTransform>().anchoredPosition = pointToPosition;
        }
    }
}
