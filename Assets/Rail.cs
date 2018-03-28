using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Rail : MonoBehaviour {

	public Transform[] nodes;

	private Vector3 movingPoint;

	private Vector3 origin;

	private Vector3 endPoint;

	public int currentSeg;

	public float distance;


	private void Start()
	{
		distance = 0;
		nodes = GetComponentsInChildren<Transform>();
		for(int i = 1; i < nodes.Length - 1; i++){
			distance += Vector3.Distance(nodes[nodes.Length - 1].position, nodes[1].position);
		}
	}

	private void OnDrawGizmos()
	{
		for(int i = 1; i < nodes.Length - 1; i++){
			Handles.DrawDottedLine(nodes[i].position, nodes[i+1].position, 0.2f);
		}
	}

	public void SetDirection(Vector3 position){

		movingPoint = position;
		SetCurrentSeg();
	}
	
	private void SetCurrentSeg(){

		for(int i = 1; i < nodes.Length - 1; i++){
			if(Vector3.Distance(nodes[i].position, movingPoint) < Vector3.Distance(nodes[i].position, nodes[i+1].position)){

				currentSeg = i;
				break;				

			}
		}
	}




	public bool LerpPosition(out Vector3 movingPoint, float ratio){

		//Vector3 distance = nodes[currentSeg].position - movingPoint;

		Vector3 p1 = nodes[currentSeg].position;
		Vector3 p2 = nodes[currentSeg + 1].position;

		movingPoint = Vector3.Lerp(p1, p2, ratio);

		return false;

	}


	

}
