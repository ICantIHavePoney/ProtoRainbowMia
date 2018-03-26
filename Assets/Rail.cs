using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Rail : MonoBehaviour {

	private Transform[] nodes;

	private void Start()
	{
		nodes = GetComponentsInChildren<Transform>();
	}


	/*private void OnDrawGizmos()
	{
		for(int i = 0; i < nodes.Length -1;i++){
			Handles.DrawDottedLine(nodes[i].position, nodes[i + 1].position, 0.2f );
		}
	}*/
}
