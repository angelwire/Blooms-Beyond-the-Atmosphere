using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class onlineSearchController : MonoBehaviour
{
    public GameObject caller;
    [SerializeField] GameObject answerBox;
    [SerializeField] GameObject questionLabel;
    [SerializeField] GameObject listButton;
    [SerializeField] GameObject listView;

    string[] planetNames;

    string[] allPlayerPlanets = new string[] { "-1" };

    float addButtonHeight;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cancelDialog();
        }
    }

    public void OnEnable()
    {
        allPlayerPlanets = new string[] { "-1" };
    }

    public void Start()
    {
        answerBox.GetComponent<InputField>().Select();
        addButtonHeight = listButton.GetComponent<RectTransform>().sizeDelta.y;
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

    public void buttonClicked(string inText)
    {
        answerBox.GetComponent<InputField>().text = inText;
    }

    public void searchClicked()
    {
        if (allPlayerPlanets.Length == 1 && allPlayerPlanets[0] == "-1")
        {
            GameObject searhingPanel = caller.GetComponent<SpaceStationPanelController>().showLoadingInTransform(listView.transform);
            searhingPanel.GetComponent<loadingPanelController>().setText("Searching...");

            GameJolt.API.DataStore.GetKeys(true, "playerplanet_*", keys => {

                caller.GetComponent<SpaceStationPanelController>().hideLoading();
                if (keys != null)
                {
                    allPlayerPlanets = new string[keys.Length];
                    for (int i = 0; i < keys.Length; i++)
                    {
                        allPlayerPlanets[i] = keys[i];
                    }

                    populateList(allPlayerPlanets);
                }
                else
                {
                    allPlayerPlanets = new string[0];
                    allPlayerPlanets[0] = "";
                    populateList(allPlayerPlanets);
                }
            });
        }
    }

    public void populateList(string[] nameList)
    {
        planetNames = new string[nameList.Length];
        for(int i =0; i<nameList.Length; i+=1)
        {
            planetNames[i] = nameList[i];
        }

        float currentPositition = 0;
        ScrollRect sr = listView.GetComponent<ScrollRect>();
        for (int i = 0; i < planetNames.Length; i += 1)
        {
            addButtonToList(planetNames[i], currentPositition, sr.content.transform);
            currentPositition -= addButtonHeight;
        }
        sr.content.sizeDelta = new Vector2(sr.content.sizeDelta.x, -currentPositition);
    }

    public void addButtonToList(string inText, float position, Transform parent)
    {
        GameObject addButton = Instantiate(listButton, parent.transform);
        addButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, position);
        addButton.GetComponentInChildren<Text>().text = inText.Split('_')[1];
    }
}
