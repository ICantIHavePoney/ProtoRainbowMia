using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{


    public Transform TargetPosition, TargetFocus;
    public float SmoothSpeed = 0.3f;
    public float SphereCastRadius = 1.0f;
    public LayerMask CollisionsLayer;
    public bool IsClamped
    {
        get
        {
            return _clamped;
        }
    }

    private bool _clamped = false;

    private Vector3 _vel;
    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        //transform.position = Vector3.SmoothDamp(transform.position, TargetPosition.position, ref _vel, SmoothSpeed, 30f, Time.fixedDeltaTime);
        transform.position = ClampPosition();
        transform.LookAt(TargetFocus);
    }

    private Vector3 ClampPosition()
    {
        RaycastHit hit;
        Debug.DrawRay(TargetFocus.position, TargetPosition.position - TargetFocus.position, Color.green, Time.fixedDeltaTime);
        if (Physics.SphereCast(TargetFocus.position, SphereCastRadius, TargetPosition.position - TargetFocus.position, out hit, Vector3.Distance(TargetPosition.position, TargetFocus.position), CollisionsLayer))
        {
            _clamped = true;
            return hit.point + hit.normal * SphereCastRadius;
        }
        _clamped = false;
        return TargetPosition.position;
    }
}
