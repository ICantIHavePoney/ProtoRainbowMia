using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class State{
    
    
    [SerializeField]
    private float gravity;
    [SerializeField]
    private float forwardForce;
    [SerializeField]
    private float lateralForce;
    [SerializeField]
    private float verticalForce;
    [SerializeField]
    private StateType type;

    public State(float g, float f, float l, float v, StateType t){
        gravity = g;
        forwardForce = f;
        lateralForce = l;
        verticalForce = v;
        type = t;
    }   

    public float GetGravity(){
        return gravity;
    }

    public float GetForwardForce(){
        return forwardForce;
    }

    public float GetLateralForce(){
        return lateralForce;
    }

    public float GetVerticalForce(){
        return verticalForce;
    }

    public StateType GetStateType(){

        return type;
    }

    public override int GetHashCode()
    {
        return (int)
        (17 + 31 * gravity.GetHashCode() + 31 * forwardForce.GetHashCode() +  
        31 * lateralForce.GetHashCode() + 31 * verticalForce.GetHashCode());
    }

    public override bool Equals(object obj){
        State other = obj as State;
        return other != null && this.type == other.type;
    }
}

class Transition{
    public readonly State RelatedState;
    readonly PlayerStatus status;

    public Transition(State currentState, PlayerStatus command)
    {
        RelatedState = currentState;
        status = command;
    }

    public override int GetHashCode()
    {
        return 17 + 31 * RelatedState.GetHashCode() + 31 * status.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        Transition other = obj as Transition;
        return other != null && this.RelatedState.Equals(other.RelatedState) && this.status == other.status;
    }
}

[Serializable]
public class StateMachine{

    [SerializeField]
    private Dictionary<Transition, State> transitions;

    public State currentStateProperties {get{
        return currentState;
    } 
    private set{}}

    [SerializeField]
    private State currentState;

    public StateMachine(){

        State GroundedState = new State(-9.8f, 20,1,20,StateType.Grounded);
        State InAirState = new State(-9.8f, 20, 0.5f, 0, StateType.InAir); 
        State wallRideState = new State(-2.5f, 20, 0, 15, StateType.wallRide);
        transitions = new Dictionary<Transition, State>{
            {new Transition(GroundedState, PlayerStatus.InAir), InAirState},
            {new Transition(InAirState, PlayerStatus.Grounded), GroundedState},
            {new Transition(InAirState, PlayerStatus.OnWall), wallRideState},
            {new Transition(wallRideState, PlayerStatus.OffWall), InAirState},
            {new Transition(wallRideState, PlayerStatus.Grounded),  GroundedState }
        };

        currentState = InAirState;
    }

    public State SwitchState(PlayerStatus status){
        Transition newTransition = new Transition(currentState, status);  

        State nextState;
        
        Debug.Log(status);

        if(!transitions.TryGetValue(new Transition(currentState, status), out nextState)){
            return currentState;
        }
        currentState = nextState;
        return nextState;        
    }

}


public enum StateType{
    InAir,
    Grounded,
    wallRide,
    wallRideEnded
}

public enum PlayerStatus{
	InAir,
    Grounded,
    Slide,
    SlideOff,
    OnWall,
    OffWall	
}
