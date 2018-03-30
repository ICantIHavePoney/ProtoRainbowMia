using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{


    public Transform Target;
    public float Distance;
    public float InitialAngle;
    public float RotationSpeed = 25f;
    public float SmoothRatio = 5f;
    public float MinAngle = 10f;
    public float MaxAngle = 45f;
    public CameraFocus CameraFocusObj;


    private float _Zoom = 0;
    private Transform _cam, _rotationArm;
    private Vector3 _initialOffset, _vel;
    private Animator _ZoomAnimator;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        _rotationArm = transform.GetChild(0);
        _cam = _rotationArm.GetChild(0);
        _ZoomAnimator = _rotationArm.GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {
        var rot = _rotationArm.localEulerAngles;
        rot.x = InitialAngle;
        _rotationArm.localEulerAngles = rot;
        _initialOffset = Target.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }


    void FixedUpdate()
    {
        //Rotation around
        float roty = Input.GetAxis("Mouse X") * 2f;
        float rotx = -Input.GetAxis("Mouse Y");
        transform.Rotate(Vector3.up, roty * Time.fixedDeltaTime * RotationSpeed);



        if (Mathf.Abs(rotx) > 0.5f)
        {
            rotx *= 2;
            var rot = _rotationArm.localEulerAngles;
            float rotation = (rot.x > 90) ? rot.x - 360 : rot.x;
            rotation += rotx * Time.fixedDeltaTime * RotationSpeed;


            rot.x = Mathf.Clamp(rotation, MinAngle, MaxAngle);
            _rotationArm.localEulerAngles = rot;
        }

        transform.position = Vector3.SmoothDamp(transform.position, Target.position, ref _vel, SmoothRatio);
        //transform.position = Target.position;
    }

}
