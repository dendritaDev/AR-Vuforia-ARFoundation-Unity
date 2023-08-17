using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
public class TouchController : MonoBehaviour
{

    public UnityEvent OnTouch;

    private void OnMouseDown()
    {
        OnTouch.Invoke();
    }
}
