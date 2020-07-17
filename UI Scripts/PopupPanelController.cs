using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupPanelController : MonoBehaviour
{
    [SerializeField] GameObject textObject;
    [SerializeField] float popinSpeed;
    [SerializeField] float lifetime;
    float timeAlive = 0f;
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, popinSpeed * Time.deltaTime);
        timeAlive += Time.deltaTime;
        if (timeAlive > lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void setText(string inText)
    {
        textObject.GetComponent<Text>().text = inText;
    }
}
