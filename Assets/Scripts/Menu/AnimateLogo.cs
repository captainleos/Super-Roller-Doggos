using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateLogo : MonoBehaviour
{
    [Header("X Variables")]
    [SerializeField] private float currentX = 0f;
    [SerializeField] private float speedX = 0.2f;
    [SerializeField] private float minX = 680f;
    [SerializeField] private float maxX = 900f;

    [Header("Y Variables")]
    [SerializeField] private float currentY = 0f;
    [SerializeField] private float speedY = 0.2f;
    [SerializeField] private float minY = 380f;
    [SerializeField] private float maxY = 460f;

    private RectTransform _rectTransform;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        _rectTransform.sizeDelta = new Vector2(Mathf.SmoothStep(minX, maxX, currentX), Mathf.SmoothStep(minY, maxY, currentY));
        MorphX();
        MorphY();
    }

    void MorphX()
    {
        currentX += speedX * Time.deltaTime;

        if (currentX > 1.0f)
        {
            float temp = maxX;
            maxX = minX;
            minX = temp;
            currentX = 0.0f;
        }
    }

    void MorphY()
    {
        currentY += speedY * Time.deltaTime;

        if (currentY > 1.0f)
        {
            float temp = maxY;
            maxY = minY;
            minY = temp;
            currentY = 0.0f;
        }
    }
}
