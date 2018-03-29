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
    [SerializeField]
    private bool canControl;

    public State(float g, float f, float l, float v, StateType t, bool cC){
        gravity = g;
        forwardForce = f;
        lateralForce = l;
        verticalForce = v;
        type = t;
        canControl = cC;
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

    public bool GetCanControl()
    {
        return canControl;
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

        State groundedState = new State(-9.8f, 20,1,20,StateType.grounded, true);
        State inAirState = new State(-9.8f, 20, 0.5f, 0, StateType.inAir, true); 
        State wallRideState = new State(-2.5f, 20, 0, 15, StateType.wallRide, false);
        State boostState = new State(-9.8f, 40, 1, 10, StateType.boost, true);
        transitions = new Dictionary<Transition, State>{

            {new Transition(groundedState, PlayerStatus.InAir), inAirState},
            {new Transition(groundedState, PlayerStatus.Sprint), boostState},

            {new Transition(boostState, PlayerStatus.Grounded), groundedState},

            {new Transition(inAirState, PlayerStatus.Sprint), boostState},
            {new Transition(inAirState, PlayerStatus.Grounded), groundedState},
            {new Transition(inAirState, PlayerStatus.OnWall), wallRideState},
            
            {new Transition(wallRideState, PlayerStatus.OffWall), inAirState},
            {new Transition(wallRideState, PlayerStatus.Grounded),  groundedState}
        };

        currentState = inAirState;
    }

    public State SwitchState(PlayerStatus status){
        Transition newTransition = new Transition(currentState, status);  

        State nextState;
        
        Debug.Log(status);

        if(!transitions.TryGetValue(new Transition(currentState, status), out nextState)){
            Debug.LogWarning("Transition non trouvée");
            return currentState;
        }
        currentState = nextState;
        return nextState;        
    }

}


public enum StateType{
    inAir,
    grounded,
    wallRide,
    boost
}

public enum PlayerStatus{
	InAir,
    Grounded,
    Slide,
    SlideOff,
    OnWall,
    OffWall,
    Sprint
}
