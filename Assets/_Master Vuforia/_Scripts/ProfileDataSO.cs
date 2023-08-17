using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Profile Data", menuName = "ScriptableObjects/Profile Data", order = 0)]
public class ProfileDataSO : ScriptableObject
{
    public enum URLType
    {
        Normal,
        Phone,
        Email
    }

    [Header("URL")]
    public string URL;
    public URLType urlType = URLType.Normal;
    public Sprite profileSprite;

    [Space]
    public bool useprofileText = false;
    public string profileText;

    [Header("Type - Email")]
    public string emailDirection;

    public string GetURL()
    {
        switch (urlType)
        {
            case URLType.Normal:
                return URL;

            case URLType.Phone:
                return string.Format("tel://{0}", URL);

            case URLType.Email:
                return string.Format("mailto:{0}", emailDirection);
        }

        return "";
    }



}
