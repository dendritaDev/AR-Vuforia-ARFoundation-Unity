/*============================================================================== 
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.  

Vuforia version: 10.2.5
Original script: PlaneManager.cs
Modified by: Mariano Sosa
Original resources: https://assetstore.unity.com/packages/templates/packs/vuforia-core-samples-99026
==============================================================================*/

using System.Timers;
using UnityEngine;
using Vuforia;

public class ARPlaneManager : MonoBehaviour
{
    [Header("Placement Augmentation")]
    [SerializeField] GameObject Product;

    const int RELOCALIZATION_TIMER = 10000;
    const string UNSUPPORTED_DEVICE_TITLE = "Unsupported Device";
    const string UNSUPPORTED_DEVICE_BODY = "This device does not support anchors and hit test functionality. " +
                                           "Please check the list of supported Ground Plane devices on our site: " +
                                           "\n\nhttps://library.vuforia.com/articles/Solution/ground-plane-supported-devices.html";

    PlaneFinderBehaviour mPlaneFinder;

    ContentPositioningBehaviour mContentPositioningBehaviour;
    ARTouchHandler mARTouchHandler;
    ARProductPlacement mARProductPlacement;
    AnchorBehaviour mPlacementAnchor;
    Timer mTimer;
    int mAutomaticHitTestFrameCount;
    bool mTimerFinished;
    TargetStatus mCachedStatus = TargetStatus.NotObserved;
    Vector3 mOriginalPlaneScale;
    Vector3 mOriginalMidAirScale;

    public bool GroundPlaneHitReceived { get; private set; }

    /// <summary>
    /// More Strict: Property returns true when Status is Tracked and StatusInfo is Normal.
    /// </summary>
    public bool TrackingStatusIsTrackedAndNormal => (mCachedStatus.Status == Status.TRACKED ||
                                                     mCachedStatus.Status == Status.EXTENDED_TRACKED) &&
                                                     mCachedStatus.StatusInfo == StatusInfo.NORMAL;

    /// <summary>
    /// Less Strict: Property returns true when Status is Tracked/Normal or Limited/Unknown.
    /// </summary>
    public bool TrackingStatusIsTrackedOrLimited => (mCachedStatus.Status == Status.TRACKED ||
                                                     mCachedStatus.Status == Status.EXTENDED_TRACKED) &&
                                                     mCachedStatus.StatusInfo == StatusInfo.NORMAL ||
                                                     mCachedStatus.Status == Status.LIMITED && mCachedStatus.StatusInfo == StatusInfo.UNKNOWN;

    /// <summary>
    /// The Surface Indicator should only be visible if the following conditions are true:
    /// 1. Tracking Status is Tracked or Limited (sufficient for Hit Test Anchors)
    /// 2. Ground Plane Hit was received for this frame
    /// 3. The Plane Mode is equal to GROUND or PLACEMENT(see #4)
    /// 4. If the Plane Mode is equal to PLACEMENT and *there's no active touches
    /// </summary>
    bool SurfaceIndicatorVisibilityConditionsMet => TrackingStatusIsTrackedOrLimited &&
                                                    GroundPlaneHitReceived &&
                                                    (Input.touchCount == 0);

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaInitialized;
        VuforiaBehaviour.Instance.DevicePoseBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;

        mPlaneFinder = FindObjectOfType<PlaneFinderBehaviour>();
        mContentPositioningBehaviour = mPlaneFinder.GetComponent<ContentPositioningBehaviour>();

        mPlaneFinder.HitTestMode = HitTestMode.AUTOMATIC;

        mARProductPlacement = FindObjectOfType<ARProductPlacement>();
        mARTouchHandler = FindObjectOfType<ARTouchHandler>();

        mPlacementAnchor = Product.GetComponentInParent<AnchorBehaviour>();

        GroundPlaneUtilities.EnableRendererColliderCanvas(Product, false);

        // Setup a timer to restart the DeviceTracker if tracking does not receive
        // status change from StatusInfo.RELOCALIZATION after 10 seconds.
        mTimer = new Timer(RELOCALIZATION_TIMER);
        mTimer.Elapsed += TimerFinished;
        mTimer.AutoReset = false;
    }

    void Update()
    {
        // The timer runs on a separate thread and we need to ResetTrackers on the main thread.
        if (mTimerFinished)
        {
            ResetDevicePoseBehaviour();
            ResetScene();
            mTimerFinished = false;
        }
    }

    void LateUpdate()
    {
        // The AutomaticHitTestFrameCount is assigned the Time.frameCount in the
        // HandleAutomaticHitTest() callback method. When the LateUpdate() method
        // is then called later in the same frame, it sets GroundPlaneHitReceived
        // to true if the frame number matches. For any code that needs to check
        // the current frame value of GroundPlaneHitReceived, it should do so
        // in a LateUpdate() method.
        GroundPlaneHitReceived = mAutomaticHitTestFrameCount == Time.frameCount;

        // Surface Indicator visibility conditions rely upon GroundPlaneHitReceived,
        // so we will move this method into LateUpdate() to ensure that it is called
        // after GroundPlaneHitReceived has been updated in Update().
        SetSurfaceIndicatorVisible(SurfaceIndicatorVisibilityConditionsMet);
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy() called.");
        if (VuforiaBehaviour.Instance != null)
            VuforiaBehaviour.Instance.DevicePoseBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    public void HandleAutomaticHitTest(HitTestResult result)
    {
        mAutomaticHitTestFrameCount = Time.frameCount;

        if (!mARProductPlacement.IsPlaced)
        {
            mARProductPlacement.DetachProductFromAnchor();
            Product.transform.position = result.Position;
        }
    }

    public void HandleInteractiveHitTest(HitTestResult result)
    {
        if (result == null)
        {
            Debug.LogError("Invalid hit test result!");
            return;
        }

        if (!mARTouchHandler.IsCanvasButtonPressed())
        {
            Debug.Log("HandleInteractiveHitTest() called.");

            // If the PlaneFinderBehaviour's Mode is Automatic, then the Interactive HitTestResult will be centered.
            // PlaneMode.Ground and PlaneMode.Placement both use PlaneFinder's ContentPositioningBehaviour
            mContentPositioningBehaviour.DuplicateStage = false;

            // Place object based on Ground Plane mode
            // With a tap a new anchor is created, so we first check that

            // Status=TRACKED/EXTENDED_TRACKED and StatusInfo=NORMAL before proceeding.
            if (TrackingStatusIsTrackedAndNormal)
            {
                // We assign our stage content, set an anchor and enable the content.
                mContentPositioningBehaviour.AnchorStage = mPlacementAnchor;
                mContentPositioningBehaviour.PositionContentAtPlaneAnchor(result);
                GroundPlaneUtilities.EnableRendererColliderCanvas(Product, true);

                // If the product has not been placed in the scene yet, we attach it to the anchor
                // while rotating it to face the camera. Then we activate the content, also
                // enabling rotation input detection.
                // Otherwise, we simply attach the content to the new anchor, preserving its rotation.
                // The placement methods will set the IsPlaced flag to true if the 
                // transform argument is valid and to false if it is null.
                if (!mARProductPlacement.IsPlaced)
                {
                    mARProductPlacement.PlaceProductAtAnchorFacingCamera(mPlacementAnchor.transform);
                    mARTouchHandler.EnableRotation = true;
                }
                else
                {
                    mARProductPlacement.PlaceProductAtAnchorAndSnapToMousePosition(mPlacementAnchor.transform);
                }
            }
        }
    }

    /// <summary>
    /// This method resets the augmentations and scene elements.
    /// It is called by the UI Reset Button and also by OnVuforiaPaused() callback.
    /// </summary>
    public void ResetScene()
    {
        Debug.Log("ResetScene() called.");

        // reset augmentations
        mARProductPlacement.Reset();
        GroundPlaneUtilities.EnableRendererColliderCanvas(Product, false);

        mARProductPlacement.DetachProductFromAnchor();
        mARTouchHandler.EnableRotation = false;
    }

    /// <summary>
    /// This method stops and restarts the DevicePoseBehaviour.
    /// It is called by the UI Reset Button and when RELOCALIZATION status has
    /// not changed for 10 seconds.
    /// </summary>
    public void ResetDevicePoseBehaviour()
    {
        Debug.Log("ResetDevicePoseBehaviour() called.");

        mPlacementAnchor.UnconfigureAnchor();
        VuforiaBehaviour.Instance.DevicePoseBehaviour.Reset();
    }

    /// <summary>
    /// This private method is called by the UI Button handler methods.
    /// </summary>
    /// <param name="mode">PlaneMode</param>
    void SetMode()
    {
        mPlaneFinder.enabled = true;
        mARTouchHandler.EnableRotation = Product.activeInHierarchy;
    }

    /// <summary>
    /// This method can be used to set the Ground Plane surface indicator visibility.
    /// This sample will display it when the Status=TRACKED and StatusInfo=Normal.
    /// </summary>
    /// <param name="isVisible">bool</param>
    void SetSurfaceIndicatorVisible(bool isVisible)
    {
        var renderers = mPlaneFinder.PlaneIndicator.GetComponentsInChildren<Renderer>(true);
        var canvases = mPlaneFinder.PlaneIndicator.GetComponentsInChildren<Canvas>(true);

        foreach (var canvas in canvases)
            canvas.enabled = isVisible;

        foreach (var renderer in renderers)
            renderer.enabled = isVisible;
    }

    /// <summary>
    /// This is a C# delegate method for the Timer:
    /// ElapsedEventHandler(object sender, ElapsedEventArgs e)
    /// </summary>
    /// <param name="source">System.Object</param>
    /// <param name="e">ElapsedEventArgs</param>
    void TimerFinished(System.Object source, ElapsedEventArgs e)
    {
        mTimerFinished = true;
    }

    void OnVuforiaInitialized(VuforiaInitError initError)
    {
        if (initError != VuforiaInitError.NONE)
            return;

        Debug.Log("OnVuforiaInitialized() called.");

        if (VuforiaBehaviour.Instance.World.AnchorsSupported)
        {
            if (!VuforiaBehaviour.Instance.DevicePoseBehaviour.enabled)
            {
                Debug.LogError("The Ground Plane feature requires the Device Tracking to be started. " +
                               "Please enable it in the Vuforia Configuration or start it at runtime through the scripting API.");
                return;
            }

            Debug.Log("DevicePoseBehaviour is Active");
        }
        else
        {
            //Create unsupported message using:
            //  UNSUPPORTED_DEVICE_TITLE
            //  UNSUPPORTED_DEVICE_BODY
        }
    }

    void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        mCachedStatus = targetStatus;

        Debug.Log("PlaneManager.OnTargetStatusChanged(" + mCachedStatus.Status + ", " + mCachedStatus.StatusInfo + ")");

        if (mCachedStatus.StatusInfo != StatusInfo.RELOCALIZING)
        {
            // If the timer is running and the status is no longer Relocalizing, then stop the timer
            if (mTimer.Enabled)
                mTimer.Stop();
        }
        else
        {
            // Start a 10 second timer to Reset Device Tracker
            if (!mTimer.Enabled)
                mTimer.Start();
        }
    }
   
}
