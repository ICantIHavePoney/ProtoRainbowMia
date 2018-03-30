using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {


	private Rigidbody rb;

	[SerializeField]
	private StateMachine stateMachine;

	[SerializeField]
	private State currentState;

	[SerializeField]
	private State previousState;

	[SerializeField]
	private Animator animator;

	private Transform mainCam;

	private bool isWallRiding;

	private bool isSprinting;

	private ConstantForce cf;

	private Rail currentRail;

	public bool isMoving;

	private bool canJump;

	private bool isRiding;

	private bool jumpEnded;

	float startHeight = 0;

	public float minHeightOffset;

	public float maxHeightOffset;

	public float angle = 0;

	public float ratio = 0;

	public float minHeight;

	public float endHeight;

	float currentSpeed;

	bool isJumping;

	Vector3 newPos;

	Vector3 newRot;

	// Use this for initialization
	void Start () {
		endHeight = 100000000000000f;
		stateMachine = new StateMachine();
		currentState = stateMachine.currentStateProperties;
		cf = GetComponent<ConstantForce>();
		mainCam = Camera.main.transform;
		rb = GetComponent<Rigidbody>();
		cf.force = new Vector3(0, rb.mass * currentState.GetGravity(), 0);
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0){
			isMoving = true;
			ratio += Time.deltaTime  * 0.4f;
			if(ratio >= 1){
				ratio = 1;
			}
					animator.SetBool("isMoving", true);
			currentSpeed = Mathf.Lerp(0, currentState.GetForwardForce(), ratio);
			newPos = transform.position + new Vector3(transform.forward.x, 0, transform.forward.z) * currentSpeed * Time.deltaTime;
			newRot = new Vector3(0, mainCam.localEulerAngles.y + Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg * currentState.GetLateralForce(), 0);
		}
		else if((Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) && isMoving){
			
			
			animator.SetBool("isMoving", false);
			ratio -= Time.deltaTime  * 0.005f;
		
			if( ratio < 0.985f){
				isMoving = false;
				ratio = 0;
			}
			currentSpeed = Mathf.Lerp(1, currentSpeed, ratio);
			newPos = transform.position + new Vector3(transform.forward.x, 0, transform.forward.z) * currentSpeed * Time.deltaTime;

		}

		animator.SetFloat("runVelocity", ratio);

		if (Input.GetButtonDown("Jump") && canJump){
			isJumping = true;
			canJump = false;
			animator.SetTrigger("Jump");
			animator.SetBool("IsSmallJump", false);
			animator.ResetTrigger("Landed");
			minHeight = transform.position.y + minHeightOffset;
			endHeight = transform.position.y + maxHeightOffset;

		}

		if(Input.GetButtonUp("Jump")){
			if(transform.position.y < minHeight){
				endHeight = minHeight;
				animator.SetBool("IsSmallJump", true);
			}
		}

		if(Input.GetButtonDown("Fire3")){
			previousState = currentState;
			currentState = stateMachine.SwitchState(PlayerStatus.Sprint);
			animator.SetBool("isBoosting", true);
		}


		if(Input.GetButtonUp("Fire3")){
			if(previousState.GetStateType() == StateType.grounded){
				currentState = stateMachine.SwitchState(PlayerStatus.Grounded);
				animator.SetBool("isBoosting", false);
			}
		}	
    }

	void FixedUpdate()
	{

		if(isRiding){


            Vector3 newPos = Vector3.zero;
            bool lastSegment = currentRail.Slide(out newPos, ratio, angle);
            if (angle < 0)
            {
                ratio -= Time.deltaTime * currentState.GetForwardForce() / currentRail.currentSegment.distance;
            }
            else
            {
                ratio += Time.deltaTime * currentState.GetForwardForce() / currentRail.currentSegment.distance;
            }

            if(ratio > 1  && angle >= 0)
            {
                if (!lastSegment)
                {
                    ratio = 0;
                    currentRail.currentSegmentIndex++;
                    currentRail.SetRailSegment();
                    transform.rotation = currentRail.currentSegment.firstEndPoint.rotation;
                }
                else
                {
                    isRiding = false;
					rb.AddForce(transform.forward * currentState.GetForwardForce() * rb.mass, ForceMode.Impulse);
					isJumping = true;
                }
            }
            if(ratio < 0  && angle < 0)
            {
                if (!lastSegment)
                {
                    ratio = 1;
                    currentRail.currentSegmentIndex--;
                    currentRail.SetRailSegment();
                    transform.rotation = Quaternion.Inverse(currentRail.currentSegment.firstEndPoint.rotation);
                }
                else
                {
                    isRiding = false;
					rb.AddForce(transform.forward * currentState.GetForwardForce() * rb.mass, ForceMode.Impulse);
					isJumping = true;
				}
            }
            transform.position = newPos;

		}

        if(isMoving && currentState.GetCanControl()){

            transform.rotation = Quaternion.Euler(newRot);
            rb.MovePosition(newPos);
		}


        if (isJumping)
        {
			rb.AddForce(transform.up * currentState.GetVerticalForce() * (rb.mass * 0.30f), ForceMode.Impulse);			
			cf.force = Vector3.zero;
			currentState = stateMachine.SwitchState(PlayerStatus.InAir);
			isJumping = false;

        }

		if(transform.position.y / endHeight * 100f >= 80){
			if(cf.force == Vector3.zero){
				cf.force = Vector3.up * currentState.GetGravity() * rb.mass;
			}
			rb.AddForce(Vector3.down, ForceMode.Acceleration);
		}
		if(transform.position.y / endHeight * 100f >= 80 && endHeight != minHeight){
			animator.SetTrigger("Fall");
		}
	}


	private void OnCollisionEnter(Collision other)
	{
			animator.ResetTrigger("Jump");
			animator.ResetTrigger("Fall");
			animator.SetTrigger("Landed");
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

			currentRail = other.transform.GetComponentInParent<Rail>();

			Vector3 distance = transform.position - other.transform.position;

            transform.position = other.transform.position + distance;

			currentState = stateMachine.SwitchState(PlayerStatus.Slide);

            currentRail.SetRailSegment(transform);

            cf.force = Vector3.zero;

            if (angle >=  0){
				ratio = 0;
                currentRail.movable.position = transform.position;
                currentRail.currentSegment.firstEndPoint = currentRail.movable;

				transform.rotation = other.transform.rotation;
			}
			else{
                ratio = 1;
                currentRail.movable.position = transform.position;
                currentRail.currentSegment.secondEndPoint = currentRail.movable;
                transform.rotation = Quaternion.Inverse(other.transform.rotation);
			}

            currentRail.currentSegment.distance = Vector3.Distance(currentRail.currentSegment.firstEndPoint.position, currentRail.currentSegment.secondEndPoint.position);

            isRiding = true;
        }
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
