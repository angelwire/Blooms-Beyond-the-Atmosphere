using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class loadingPanelController : MonoBehaviour
{
    GameObject spinner;
    float rotationSpeed = -50f;
    [SerializeField] Text textObject;

    // Start is called before the first frame update
    void Start()
    {
        spinner = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        spinner.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    public void setText(string inText)
    {
        textObject.text = inText;
    }
}
