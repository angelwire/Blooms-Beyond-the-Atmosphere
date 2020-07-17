using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FadeControllerScript : MonoBehaviour
{
    //The speed of the fade
    [SerializeField] float fadeSpeed;
    //The transparence that should be faded to
    private float alphaTo;
    //The image that should fade
    Image image;

    void Awake()
    {
        image = GetComponent<Image>();
        float alpha = 1;
        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        alphaTo = 0f;
    }

    void Update()
    {
        float alpha = Mathf.Lerp(image.color.a, alphaTo, fadeSpeed * Time.deltaTime);
        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
    }

    public void fadeOut()
    {
        alphaTo = 1f;
    }

    public bool fadeComplete()
    {
        return Mathf.Abs(image.color.a - alphaTo) < .01;
    }

    public void instantFadeOut()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1.0f);
        GetComponent<RectTransform>().ForceUpdateRectTransforms();
    }
}