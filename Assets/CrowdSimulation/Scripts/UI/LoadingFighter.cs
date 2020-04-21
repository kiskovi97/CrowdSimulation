using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingFighter : MonoBehaviour
{
    public Image loadingCircleImg;
    public Image[] colorables;
    public Image cloth;
    public TextMeshProUGUI text;

    public void SetImage(Sprite sprite)
    {
        cloth.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        foreach (var img in colorables)
        {
            img.color = color;
        }
    }

    public void SetLoading(float loading, int currentEntity, int maxEntity)
    {
        loadingCircleImg.fillAmount = loading;
        text.text = $"{currentEntity} / {maxEntity}";
    }
}
