/*==============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/

using UnityEngine;
using Vuforia;

public static class GroundPlaneUtilities
{
    public static void RotateTowardsCamera(GameObject augmentation)
    {
        if (VuforiaBehaviour.Instance == null) 
            return;
        
        var lookAtPosition = VuforiaBehaviour.Instance.transform.position - augmentation.transform.position;
        lookAtPosition.y = 0;
        var rotation = Quaternion.LookRotation(lookAtPosition);
        augmentation.transform.rotation = rotation;
    }

    public static void EnableRendererColliderCanvas(GameObject gameObject, bool enable)
    {
        var rendererComponents = gameObject.GetComponentsInChildren<Renderer>(true);
        var colliderComponents = gameObject.GetComponentsInChildren<Collider>(true);
        var canvasComponents = gameObject.GetComponentsInChildren<Canvas>(true);
        // Enable rendering:
        foreach (var component in rendererComponents)
            component.enabled = enable;
        // Enable colliders:
        foreach (var component in colliderComponents)
            component.enabled = enable;
        // Enable canvas':
        foreach (var component in canvasComponents)
            component.enabled = enable;
    }
}
