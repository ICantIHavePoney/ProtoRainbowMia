using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {


	private Rigidbody rb;

	[SerializeField]
	private StateMachine stateMachine;

	private RaycastHit hit;

	[SerializeField]
	private State currentState;

	[SerializeField]
	private BoxCollider floorDetectionBox;

	private Transform mainCam;

	private bool isWallRiding;

	private ConstantForce cf;

	private bool isMoving;

	float startHeight = 0;

	float endHeight;

	public AnimationCurve curve;

	private Vector2 horizontalDrag;

	// Use this for initialization
	void Start () {
		stateMachine = new StateMachine();
		currentState = stateMachine.currentStateProperties;
		cf = GetComponent<ConstantForce>();
		mainCam = Camera.main.transform;
		rb = GetComponent<Rigidbody>();
		cf.force = new Vector3(0, rb.mass * currentState.GetGravity(), 0);
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0 && !isWallRiding){
			isMoving = true;
		}
		else if((Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) || isWallRiding){
			isMoving = false;
		}

		if (Input.GetButtonDown("Jump")){
				endHeight = transform.position.y;
		}

				
		if((int)(transform.position.y/endHeight * 100) >= 80){

			Debug.Log("toto");

			rb.AddForce(Vector3.down * currentState.GetVerticalForce());
		}

		if((int)(transform.position.y/endHeight * 100) < 20 ){
			rb.AddForce(Vector3.up);
		}

		
		/*if((int)(transform.position.y/endHeight * 100) >= 100){

			Debug.Log("toto");

			rb.velocity = Vector3.down * .5f;
		}*/

		Debug.Log((int)(transform.position.y/endHeight * 100));


        if (currentState.GetStateType() == StateType.InAir && rb.velocity.y <0)
        {
            rb.AddForce(Vector3.down * rb.mass *.1f, ForceMode.Acceleration);
        }

    }


	void FixedUpdate()
	{

        if(isMoving && !isWallRiding){
            transform.rotation = Quaternion.Euler(0, mainCam.localEulerAngles.y + Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg * currentState.GetLateralForce(), 0);
            rb.MovePosition(transform.position + new Vector3(transform.forward.x, 0, transform.forward.z) * currentState.GetForwardForce() * Time.deltaTime);
        }
        if (Input.GetButtonDown("Jump"))
        {
            if (!isWallRiding)
            {

                rb.AddForce(Vector3.up * currentState.GetVerticalForce() * (rb.mass * 0.5f), ForceMode.Impulse);
                currentState = stateMachine.SwitchState(PlayerStatus.InAir);
            }
            else
            {
                rb.velocity = Vector3.zero;
                isWallRiding = false;
                rb.AddForce(Vector3.up * currentState.GetVerticalForce() * rb.mass, ForceMode.Impulse);
                currentState = stateMachine.SwitchState(PlayerStatus.OffWall);
                cf.force = new Vector3(cf.force.x, currentState.GetGravity() * rb.mass, cf.force.z);
            }
        }
	}


	private void OnCollisionEnter(Collision other)
	{
		if(other.transform.tag == "Floor" /*&& currentState.GetStateType() == StateType.InAir*/){

			currentState = stateMachine.SwitchState(PlayerStatus.Grounded);
            rb.velocity = Vector3.zero;
            if (isWallRiding)
            {

                cf.force = new Vector3(cf.force.x, currentState.GetGravity() * rb.mass, cf.force.z);
                isWallRiding = false;
            }
		}
		else if(other.transform.tag == "Wall" && currentState.GetStateType() == StateType.InAir){
            float angle = Vector3.Dot(transform.forward, other.transform.forward);

            if (Mathf.Abs(angle) > 0.5f && Input.GetButton("Jump") && !isWallRiding){
				isWallRiding = true;
				currentState = stateMachine.SwitchState(PlayerStatus.OnWall);

				if(Mathf.Sign(angle) > 0){
                	transform.rotation = other.transform.rotation;
				}
				else{
					transform.rotation = Quaternion.Inverse(other.transform.rotation);
				}

				cf.force = new Vector3(cf.force.x, currentState.GetGravity() * rb.mass, cf.force.z);

				rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.25f, rb.velocity.z);

				rb.AddForce(transform.forward * currentState.GetForwardForce() * rb.mass, ForceMode.Impulse);
			}
		}
		else if(other.transform.tag == "Slide" && currentState.GetStateType() == StateType.InAir){
			float angle = Vector3.Dot(transform.forward, other.transform.forward);

			isWallRiding = true;


			transform.rotation = other.transform.rotation;

			transform.position = new Vector3(other.transform.transform.position.x, transform.position.y, transform.position.z);

			rb.AddForce(transform.forward * 10);
		
		}

		horizontalDrag = Vector2.zero;
	}

}
