/*============================================================================== 
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.  

Vuforia version: 10.2.5
Original script: ProductPlacement.cs
Modified by: Mariano Sosa
Original resources: https://assetstore.unity.com/packages/templates/packs/vuforia-core-samples-99026
==============================================================================*/

using UnityEngine;
using Vuforia;
using Vuforia.UnityRuntimeCompiled;

[RequireComponent(typeof(ARTouchHandler))]
public class ARProductPlacement : MonoBehaviour
{
    public bool IsPlaced { get; private set; }
    public bool IsAnchorTracking { get; private set; }

    public Vector3 ProductScale
    {
        get
        {
            var augmentationScale = VuforiaRuntimeUtilities.IsPlayMode() ? 0.1f : ProductSize;
            return new Vector3(augmentationScale, augmentationScale, augmentationScale);
        }
    }

    [Header("Augmentation Object")]
    [SerializeField] GameObject Product = null;

    [Header("Control Indicators")]
    [SerializeField] GameObject TranslationIndicator = null;
    [SerializeField] GameObject RotationIndicator = null;

    [Header("Augmentation Size")]
    [Range(0.1f, 2.0f)]
    [SerializeField] float ProductSize = 0.65f;

    const string GROUND_PLANE_NAME = "Emulator Ground Plane";
    const string FLOOR_NAME = "Floor";

    MeshRenderer mProductRenderer;
    MeshFilter mProductFilter;
    ARPlaneManager mARPlaneManager;
    ARTouchHandler mARTouchHandler;
    Camera mMainCamera;
    string mFloorName;
    Vector3 mOriginalChairScale;

    bool IsProductVisible => mARPlaneManager.TrackingStatusIsTrackedOrLimited && mARPlaneManager.GroundPlaneHitReceived;

    //ProductSO Implementation
    [Header("Product Data")]
    [SerializeField] private bool UseProductLimit = true;
    [SerializeField] private ProductSO[] ProductDataArray;

    int ProductIndex;
    bool IsChangingProduct;


    void Start()
    {
        mARPlaneManager = FindObjectOfType<ARPlaneManager>();
        mARTouchHandler = FindObjectOfType<ARTouchHandler>();
        mMainCamera = VuforiaBehaviour.Instance.GetComponent<Camera>();
        mProductRenderer = Product.GetComponent<MeshRenderer>();
        mProductFilter= Product.GetComponent<MeshFilter>();
        SetupFloor();

        mOriginalChairScale = Product.transform.localScale;
        Reset();
    }

    void Update()
    {
        if (!IsChangingProduct) mProductRenderer.materials = ProductDataArray[ProductIndex].GetMaterials(IsPlaced);

        if (!IsPlaced)
            GroundPlaneUtilities.RotateTowardsCamera(Product);

        if (IsPlaced)
        {
            RotationIndicator.SetActive(Input.touchCount == 2);

            TranslationIndicator.SetActive((ARTouchHandler.sIsSingleFingerDragging || ARTouchHandler.sIsSingleFingerStationary)
                                            && !mARTouchHandler.IsCanvasButtonPressed());

            SnapProductToMousePosition();
        }
        else
        {
            RotationIndicator.SetActive(false);
            TranslationIndicator.SetActive(false);
        }
    }

    void SnapProductToMousePosition()
    {
        if (ARTouchHandler.sIsSingleFingerDragging || VuforiaRuntimeUtilities.IsPlayMode() && Input.GetMouseButton(0))
        {
            if (!UnityRuntimeCompiledFacade.Instance.IsUnityUICurrentlySelected() && !mARTouchHandler.IsCanvasButtonPressed())
            {
                var cameraToPlaneRay = mMainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(cameraToPlaneRay, out var cameraToPlaneHit) &&
                    cameraToPlaneHit.collider.gameObject.name == mFloorName)
                    Product.transform.position = cameraToPlaneHit.point;
            }
        }
    }

    void LateUpdate()
    {
        if (!IsPlaced)
            mProductRenderer.enabled = IsProductVisible;
    }

    public void Reset()
    {
        if (Product != null)
        {
            Product.transform.position = Vector3.zero;
            Product.transform.localEulerAngles = Vector3.zero;
            Product.transform.localScale = Vector3.Scale(mOriginalChairScale, ProductScale);
        }

        IsPlaced = false;
    }

    // Called by Anchor_Placement's DefaultObserverEventHandler.OnTargetFound()
    public void OnAnchorFound()
    {
        IsAnchorTracking = true;
    }

    // Called by Anchor_Placement's DefaultObserverEventHandler.OnTargetLost()
    public void OnAnchorLost()
    {
        IsAnchorTracking = false;
    }

    public void PlaceProductAtAnchorAndSnapToMousePosition(Transform anchor)
    {
        PlaceProductAtAnchor(anchor);
        SnapProductToMousePosition();
    }

    void PlaceProductAtAnchor(Transform anchor)
    {
        Product.transform.SetParent(anchor, true);
        Product.transform.localPosition = Vector3.zero;

        IsPlaced = true;
    }

    public void PlaceProductAtAnchorFacingCamera(Transform anchor)
    {
        PlaceProductAtAnchor(anchor);
        GroundPlaneUtilities.RotateTowardsCamera(Product);
    }

    public void DetachProductFromAnchor()
    {
        Product.transform.SetParent(null);
        Reset();
    }

    void SetupFloor()
    {
        if (VuforiaRuntimeUtilities.IsPlayMode())
            mFloorName = GROUND_PLANE_NAME;
        else
        {
            mFloorName = FLOOR_NAME;
            var floor = new GameObject(mFloorName, typeof(BoxCollider));
            floor.transform.SetParent(Product.transform.parent);
            floor.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            floor.transform.localScale = Vector3.one;
            floor.GetComponent<BoxCollider>().size = new Vector3(100f, 0, 100f);
        }
    }

    #region ARProductSO Implementation

    [ContextMenu("Change Product Next")]
    public void ChangeProductNext()
    {
        ChangeProduct(true);
    }
    
    [ContextMenu("Change Product Previous")]
    public void ChangeProductPrevious()
    {
        ChangeProduct(false);
    }

    private void ChangeProduct (bool IsNext) 
    {
        IsChangingProduct = true;

        ProductIndex = IsNext ? ProductIndex + 1 : ProductIndex - 1;

        if (ProductIndex > ProductDataArray.Length - 1)
        {
            ProductIndex = UseProductLimit ? ProductDataArray.Length - 1 : 0;
        }
        else if (ProductIndex < 0)
        {
            ProductIndex = UseProductLimit ? 0 : ProductDataArray.Length - 1;
        }

        mProductFilter.mesh = ProductDataArray[ProductIndex].GetMesh();

        IsChangingProduct = false;
    }
    #endregion


}
