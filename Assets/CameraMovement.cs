using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {


	[SerializeField]
	private Transform character; 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		transform.LookAt(character);

		transform.RotateAround(character.transform.position, Vector3.up, 20 * Input.GetAxis("CameraHorizontal") * Time.deltaTime);

	}
}
