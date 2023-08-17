using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleDetection : MonoBehaviour
{
    [Header("Settings: ")]
    [SerializeField] private Image detectionImg;
    [Space]
    [SerializeField] private Color colorOn;
    [SerializeField] private Color colorOff;

    private void Start()
    {
        EnableDetection(false);
    }
    public void EnableDetection(bool enable)
    {
        detectionImg.color = enable ? colorOn : colorOff;
    }
}

