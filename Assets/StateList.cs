using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateList : MonoBehaviour {

    public static StateList instance;
    
    public State GroundedState;
    public State InAirState;
    public State wallRideState;
    
    // Use this for initialization
    void Start () {
        if (instance != this)
        {
            Destroy(instance);
        }
        instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
