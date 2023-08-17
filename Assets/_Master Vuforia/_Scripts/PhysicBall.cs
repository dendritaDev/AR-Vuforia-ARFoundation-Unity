using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicBall : MonoBehaviour
{
    private Rigidbody _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void EnableRigidBody(bool enable)
    {
        _rigidbody.constraints = enable ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;

    }

}
