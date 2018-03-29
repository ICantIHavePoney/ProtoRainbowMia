using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public struct RailSegment
{
    public Transform firstEndPoint;

    public Transform secondEndPoint;

    public float distance;

    public bool isFinal;

    public RailSegment(Transform fEP, Transform sEP, float d, bool iF)
    {
        firstEndPoint = fEP;

        secondEndPoint = sEP;

        distance = d;

        isFinal = iF;
    }
}


[ExecuteInEditMode]
public class Rail : MonoBehaviour {

	public Transform[] nodes;

    public Transform nodesParent;

    public List<RailSegment> segments;

    public int currentSegmentIndex;

    public Transform movable;

    public RailSegment currentSegment;

	private void Start()
	{
		nodes = nodesParent.GetComponentsInChildren<Transform>();

        segments = new List<RailSegment>();

        for (int i = 2; i < nodes.Length - 1; i++)
        {     
            segments.Add(new RailSegment(nodes[i], nodes[i + 1], Vector3.Distance(nodes[i].position, nodes[i + 1].position), i == 2 || i == nodes.Length -2));
        }

        movable = nodes[1];

	}

	private void OnDrawGizmos()
	{
		for(int i = 2; i < nodes.Length - 1; i++){
			Handles.DrawDottedLine(nodes[i].position, nodes[i+1].position, 0.2f);
		}
	}

    public void SetRailSegment(Transform position = null)
    {
        if (position)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                if(segments[i].distance > Vector3.Distance(segments[i].firstEndPoint.position, position.position))
                {
                    currentSegmentIndex = i;
                    currentSegment = segments[i];
                    break;
                }
            }
        }
        else
        {
            currentSegment = segments[currentSegmentIndex];
        }
    }




    public bool Slide(out Vector3 movingPoint, float ratio, float direction)
    {
        movingPoint = Vector3.Lerp(currentSegment.firstEndPoint.position, currentSegment.secondEndPoint.position , ratio);

        if ((currentSegmentIndex == 0  && direction < 0)|| (currentSegmentIndex == segments.Count - 1 && direction >= 0))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
	


	

}
