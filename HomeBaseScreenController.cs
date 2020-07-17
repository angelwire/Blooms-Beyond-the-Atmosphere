using UnityEngine;
using UnityEngine.SceneManagement;
//Author William Jones
public class HomeBaseScreenController : MonoBehaviour
{
    [SerializeField] float rotationSpeed;
    GameObject promptObject;
    bool isHovered = false;
    Camera mainCamera;

    void Start()
    {
        promptObject = transform.GetChild(0).gameObject;
        promptObject.SetActive(false);
        mainCamera = Camera.main;
    }

        // Update is called once per frame
        void Update()
    {
        gameObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (isHovered)
        {
            promptObject.transform.rotation = Quaternion.identity * mainCamera.transform.rotation;
        }

        //If the player is looking at the bench
        if (isHovered)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if(SceneManager.GetActiveScene()!=SceneManager.GetSceneByName("onlinePlanet"))
                    WorldManager.instance.ActivateHomeBaseMenu();
            }
        }

    }

    //Sets the hover bool
    public void setHover(bool hover)
    {
        if (hover != isHovered)
        {
            isHovered = hover;
            //Enable the text prompt
            promptObject.SetActive(isHovered);
            //Set the layer to the effects layer if it's hovered
            if (hover)
            {
                gameObject.layer = LayerMask.NameToLayer("OutlineEffects");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Hoverable");
            }
        }
    }


}
