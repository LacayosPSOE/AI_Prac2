using FSMs;
using UnityEngine;
using Steerings;
using Pathfinding;  
using System.Threading;

[CreateAssetMenu(fileName = "FSM_Guide", menuName = "Finite State Machines/FSM_Guide", order = 1)]
public class FSM_Guide : FiniteStateMachine
{
    private GUIDE_Blackboard blackboard;
    private PathFeeder pathFeeder;
    private GameObject wanderPoint;
    private GraphUpdateScene graphUpdateScene;  

    public override void OnEnter()
    {
        blackboard = GetComponent<GUIDE_Blackboard>();
        pathFeeder = GetComponent<PathFeeder>();
        graphUpdateScene = GetComponent<GraphUpdateScene>(); 
        base.OnEnter();
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void OnConstruction()
    {
        State wandering = new State("Wandering",
            () => {
                wanderPoint = SensingUtils.FindRandomInstanceWithinRadius(gameObject, "POINT", 1000f);
                pathFeeder.target = wanderPoint;
                ApplyGraphUpdate();
            },
            () => { },
            () => { }
        );

        State foundCultist = new State("Found_Cultist",
            () => { 
                pathFeeder.target = blackboard.cultist;
                ApplyGraphUpdate();  
            },
            () => { },
            () => { }
        );

        State runAway = new State("Run_Away",
            () => {
                blackboard.timer = 0f;
                Vector3 fleeDirection = (gameObject.transform.position - blackboard.skeleton.transform.position).normalized;
                Vector3 fleePosition = gameObject.transform.position + fleeDirection * blackboard.runDistance;

                GameObject fleePoint = new GameObject("FleePoint");
                fleePoint.transform.position = fleePosition;

                pathFeeder.target = fleePoint;

                ApplyGraphUpdate();
            },
            () => {
                blackboard.timer += Time.deltaTime;
            },
            () => {
                blackboard.timer = 0f;
            }
        );

        Transition cultistWithinRange = new Transition("Cultist_Within_Range",
            () => { return SensingUtils.DistanceToTarget(gameObject, blackboard.cultist) <= blackboard.cultistRange; },
            () => { }
        );

        Transition SkeletonTooClose = new Transition("Skeleton_Too_Close",
            () => { return SensingUtils.DistanceToTarget(gameObject, blackboard.skeleton) <= blackboard.runRange; },
            () => { }
        );

        Transition checkPointReached = new Transition("CheckPoint_Reached",
            () => { return SensingUtils.DistanceToTarget(gameObject, wanderPoint) <= blackboard.reachedRange || SensingUtils.DistanceToTarget(gameObject, blackboard.cultist) <= blackboard.reachedRange; },
            () => { }
        );

        Transition tired = new Transition("Tired",
            () => { return blackboard.timer >= 6f; },
            () => { }
        );

        AddStates(wandering, foundCultist, runAway);
        AddTransition(wandering, checkPointReached, wandering);
        AddTransition(wandering, cultistWithinRange, foundCultist);
        AddTransition(wandering, SkeletonTooClose, runAway);
        AddTransition(foundCultist, SkeletonTooClose, runAway);
        AddTransition(runAway, tired, wandering);

        initialState = wandering;
    }

    private void ApplyGraphUpdate()
    {
        if (graphUpdateScene != null)
        {
            graphUpdateScene.Apply();  
        }
    }
}