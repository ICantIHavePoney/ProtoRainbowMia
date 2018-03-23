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
		
	}


	void FixedUpdate()
	{
		if(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0 && !isWallRiding){
			transform.rotation = Quaternion.Euler(0, mainCam.localEulerAngles.y + Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg * currentState.GetLateralForce(), 0);
			transform.position += new Vector3(transform.forward.x, 0, transform.forward.z )* currentState.GetForwardForce() * Time.deltaTime;
		}
		if(Input.GetButtonDown("Jump")){
			rb.AddForce(Vector3.up * currentState.GetVerticalForce() * rb.mass, ForceMode.Impulse);
			currentState = stateMachine.SwitchState(PlayerStatus.InAir);
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if(other.transform.tag == "Floor" /*&& currentState.GetStateType() == StateType.InAir*/){
			Debug.Log("toto");
			currentState = stateMachine.SwitchState(PlayerStatus.Grounded);
		}
		else if(other.transform.tag == "Wall" && currentState.GetStateType() == StateType.InAir){
			if(Vector3.Dot(transform.forward, other.transform.forward) > 0.75f && Input.GetButton("Jump") && !isWallRiding){
				isWallRiding = true;
				currentState = stateMachine.SwitchState(PlayerStatus.OnWall);
				
				transform.rotation = other.transform.rotation;

				cf.force = new Vector3(cf.force.x, currentState.GetGravity(), cf.force.z);

				rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.25f, rb.velocity.z);

				rb.AddForce(Vector3.forward * currentState.GetForwardForce() * rb.mass, ForceMode.Impulse);
			}
		}

		horizontalDrag = Vector2.zero;
	}

}
