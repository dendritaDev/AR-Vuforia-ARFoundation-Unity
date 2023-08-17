using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileButton : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private ProfileDataSO profileData;

    [Header("Referencias")]
    [SerializeField] private Image profileImg;

    void Start()
    {
        //consumir info
        profileImg.sprite = profileData.profileSprite;

    }

    public void Execute()
    {
        Application.OpenURL(profileData.GetURL());
    }
}

