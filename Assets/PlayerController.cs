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
	private State previousState;

	[SerializeField]
	private BoxCollider floorDetectionBox;

	private Transform mainCam;

	private bool isWallRiding;

	private bool isSprinting;

	private ConstantForce cf;

	private Rail currentRail;

	private bool isMoving;

	private bool canJump;

	private bool isRiding;

	float startHeight = 0;

	public float angle = 0;

	public float ratio = 0;

	float endHeight;

	bool isJumping;

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

		if (Input.GetButtonDown("Jump") && !isJumping && canJump){
				endHeight = transform.position.y + 10;
				isJumping = true;
				canJump = false;
		}


		if(Input.GetButtonDown("Fire3")){
			previousState = currentState;
			currentState = stateMachine.SwitchState(PlayerStatus.Sprint);
		}


		if(Input.GetButtonUp("Fire3")){
			if(previousState.GetStateType() == StateType.grounded){
				currentState = stateMachine.SwitchState(PlayerStatus.Grounded);
			}
		}
		
				
		if((int)(transform.position.y/endHeight * 100) >= 30){


			rb.AddForce(Vector3.down * currentState.GetVerticalForce());
		}

		if((int)(transform.position.y/endHeight * 100) < 20 ){
			rb.AddForce(Vector3.up);
		}

        if (currentState.GetStateType() == StateType.inAir && rb.velocity.y <0)
        {
            rb.AddForce(Vector3.down * rb.mass *.1f, ForceMode.Acceleration);
        }

    }


	void FixedUpdate()
	{

		if(isRiding){
			Debug.Log("toto");
			if(angle < 0){
				ratio -= Time.deltaTime * currentState.GetForwardForce() / currentRail.distance;
			}
			else{
				ratio += Time.deltaTime * currentState.GetForwardForce() / currentRail.distance;
			}
			cf.force = Vector3.zero;
			if(ratio > 1){
				ratio = 0;
				currentRail.currentSeg ++;
			}

			else if(ratio < 0){
				ratio = 1;
				currentRail.currentSeg --;
			}

			rb.MovePosition(currentRail.LerpPosition(ratio));
		}

        if(isMoving && !isWallRiding && !isRiding){
            transform.rotation = Quaternion.Euler(0, mainCam.localEulerAngles.y + Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg * currentState.GetLateralForce(), 0);
            rb.MovePosition(transform.position + new Vector3(transform.forward.x, 0, transform.forward.z) * currentState.GetForwardForce() * Time.deltaTime);
        }
        if (isJumping)
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
			isJumping = false;
        }
	}


	private void OnCollisionEnter(Collision other)
	{
		if(other.transform.tag == "Floor" ){
			
			if(!Input.GetButton("Fire3")){
				currentState = stateMachine.SwitchState(PlayerStatus.Grounded);
			}
            rb.velocity = Vector3.zero;
            if (isWallRiding)
            {

                cf.force = new Vector3(cf.force.x, currentState.GetGravity() * rb.mass, cf.force.z);
                isWallRiding = false;
            }
			canJump = true;
		}
		else if(other.transform.tag == "Wall" && currentState.GetStateType() == StateType.inAir){
            angle = Vector3.Dot(transform.forward, other.transform.forward);

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
		else if(other.transform.tag == "Slide" && currentState.GetStateType() == StateType.inAir && !isRiding){
		
			angle = Vector3.Dot(transform.forward, other.transform.forward);

			currentRail = other.transform.GetComponent<Rail>();

			isRiding = true;

			Vector3 distance = transform.position - other.transform.position;

			if(angle > 0){
				transform.rotation = other.transform.rotation;
				currentRail.SetDirection(transform.position);
			}
			else{
				transform.rotation = Quaternion.Inverse(other.transform.rotation);
				currentRail.SetDirection(transform.position);
			}


			transform.position = other.transform.position + distance;
		}
	}


	private void OnTriggerEnter(Collider other)
	{

	}


	private void OnCollisionExit(Collision other)
	{
		if(other.transform.tag == "Wall" && isWallRiding){
			isWallRiding = false;
			currentState = stateMachine.SwitchState(PlayerStatus.OffWall);
 			cf.force = new Vector3(cf.force.x, currentState.GetGravity() * rb.mass, cf.force.z);

		}	
	}

}
