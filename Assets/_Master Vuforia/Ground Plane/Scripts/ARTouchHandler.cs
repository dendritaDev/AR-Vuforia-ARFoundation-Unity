/*==============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.

Vuforia version: 10.2.5
Original scripts:   TouchHandler.cs
                    GroundPlaneUI.cs
Modified by: Mariano Sosa
Original resources: https://assetstore.unity.com/packages/templates/packs/vuforia-core-samples-99026
==============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ARProductPlacement))]
public class ARTouchHandler : MonoBehaviour
{
    public Transform Product;
    [HideInInspector]
    public bool EnableRotation;
    public bool EnablePinchScaling;

    public static bool sIsSingleFingerStationary => IsSingleFingerDown() && Input.GetTouch(0).phase == TouchPhase.Stationary;
    public static bool sIsSingleFingerDragging => IsSingleFingerDown() && Input.GetTouch(0).phase == TouchPhase.Moved;
    static int sLastTouchCount;

    const float SCALE_RANGE_MIN = 0.1f;
    const float SCALE_RANGE_MAX = 2.0f;

    Touch[] mTouches;
    bool mIsFirstFrameWithTwoTouches;
    float mCachedTouchAngle;
    float mCachedTouchDistance;
    float mCachedAugmentationScale;
    Vector3 mCachedAugmentationRotation;

    GraphicRaycaster mGraphicRayCaster;
    PointerEventData mPointerEventData;
    EventSystem mEventSystem;

    void Start()
    {
        mGraphicRayCaster = FindObjectOfType<GraphicRaycaster>();
        mEventSystem = FindObjectOfType<EventSystem>();

        mCachedAugmentationScale = Product.localScale.x;
        mCachedAugmentationRotation = Product.localEulerAngles;
    }

    void Update()
    {
        if (Input.touchCount == 2)
        {
            GetTouchAngleAndDistance(Input.GetTouch(0), Input.GetTouch(1),
                out var currentTouchAngle, out var currentTouchDistance);

            if (mIsFirstFrameWithTwoTouches)
            {
                mCachedTouchDistance = currentTouchDistance;
                mCachedTouchAngle = currentTouchAngle;
                mIsFirstFrameWithTwoTouches = false;
            }

            var angleDelta = currentTouchAngle - mCachedTouchAngle;
            var scaleMultiplier = currentTouchDistance / mCachedTouchDistance;
            var scaleAmount = mCachedAugmentationScale * scaleMultiplier;
            var scaleAmountClamped = Mathf.Clamp(scaleAmount, SCALE_RANGE_MIN, SCALE_RANGE_MAX);

            if (EnableRotation)
                Product.localEulerAngles = mCachedAugmentationRotation - new Vector3(0, angleDelta * 3f, 0);

            // Optional Pinch Scaling can be enabled via Inspector for this Script Component
            if (EnableRotation && EnablePinchScaling)
                Product.localScale = new Vector3(scaleAmountClamped, scaleAmountClamped, scaleAmountClamped);
        }
        else if (Input.touchCount < 2)
        {
            mCachedAugmentationScale = Product.localScale.x;
            mCachedAugmentationRotation = Product.localEulerAngles;
            mIsFirstFrameWithTwoTouches = true;
        }
        // enable runtime testing of pinch scaling
        else if (Input.touchCount == 6)
            EnablePinchScaling = true;
        // disable runtime testing of pinch scaling
        else if (Input.touchCount == 5)
            EnablePinchScaling = false;
    }

    void GetTouchAngleAndDistance(Touch firstTouch, Touch secondTouch, out float touchAngle, out float touchDistance)
    {
        touchDistance = Vector2.Distance(firstTouch.position, secondTouch.position);
        var diffY = firstTouch.position.y - secondTouch.position.y;
        var diffX = firstTouch.position.x - secondTouch.position.x;
        touchAngle = Mathf.Atan2(diffY, diffX) * Mathf.Rad2Deg;
    }

    static bool IsSingleFingerDown()
    {
        if (Input.touchCount == 0 || Input.touchCount >= 2)
            sLastTouchCount = Input.touchCount;

        return Input.touchCount == 1 && Input.GetTouch(0).fingerId == 0 && sLastTouchCount == 0;
    }

    public bool IsCanvasButtonPressed()
    {
        if (!mGraphicRayCaster || !mEventSystem) return false;

        mPointerEventData = new PointerEventData(mEventSystem) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        mGraphicRayCaster.Raycast(mPointerEventData, results);

        return results.Count > 0;
    }
}