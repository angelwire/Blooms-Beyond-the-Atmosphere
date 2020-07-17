using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class textInputController : MonoBehaviour
{
    public GameObject caller;
    [SerializeField] GameObject answerBox;
    [SerializeField] GameObject questionLabel;

    public void Start()
    {
        answerBox.GetComponent<InputField>().Select();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cancelDialog();
        }
    }

    public void cancelDialog()
    {
        if (caller.GetComponent<UIControllerScript>() != null)
        {
            caller.GetComponent<UIControllerScript>().dialogReturn("");
        }
        if (caller.GetComponent<SpaceStationPanelController>() != null)
        {
            caller.GetComponent<SpaceStationPanelController>().dialogReturn(""); 
        }
        GameObject.Destroy(gameObject);
    }

    public void confirmDialog()
    {
        if (caller.GetComponent<UIControllerScript>() != null)
        {
            caller.GetComponent<UIControllerScript>().dialogReturn(answerBox.GetComponent<InputField>().text);
        }
        if (caller.GetComponent<SpaceStationPanelController>() != null)
        {
            caller.GetComponent<SpaceStationPanelController>().dialogReturn(answerBox.GetComponent<InputField>().text);
        }
            GameObject.Destroy(gameObject);
    }

    public void setQuestionText(string inputString)
    {
        questionLabel.GetComponent<Text>().text = inputString;
    }
}
