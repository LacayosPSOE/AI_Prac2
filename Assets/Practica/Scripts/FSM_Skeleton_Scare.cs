using FSMs;
using UnityEngine;
using Steerings;

[CreateAssetMenu(fileName = "FSM_Skeleton_Scare", menuName = "Finite State Machines/FSM_Skeleton_Scare", order = 1)]
public class FSM_Skeleton_Scare : FiniteStateMachine
{

    private SKELETON_Blackboard blackboard;
    private PathFeeder pathFeeder;

    public override void OnEnter()
    {
        blackboard = GetComponent<SKELETON_Blackboard>();
        pathFeeder = GetComponent<PathFeeder>();
        base.OnEnter(); 
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void OnConstruction()
    {

        State chasingPaladin = new State("Chasing_Paladin",
            () => { pathFeeder.target = blackboard.paladin; }, 
            () => { if (blackboard.timer > 0) { blackboard.timer -= Time.deltaTime; } else { } }, 
            () => { }   
        );

        State scareGuide = new State("Scare_Guide",
            () => { pathFeeder.target = blackboard.guide; }, 
            () => { blackboard.timer += Time.deltaTime; }, 
            () => { }  
        );


        Transition chaseGuide = new Transition("Chase_Guide",
            () => { return blackboard.timer <= 0 && SensingUtils.DistanceToTarget(gameObject, blackboard.guide) <= blackboard.scareRange;  }, 
            () => { }  
        );
                Transition quitChasingGuide = new Transition("Quit_Chasing_Guide",
            () => {return blackboard.timer >= 5;},
            () => { }  
        );
        AddStates(chasingPaladin, scareGuide);
        AddTransition(chasingPaladin, chaseGuide, scareGuide);
        AddTransition(scareGuide, quitChasingGuide, chasingPaladin);

         initialState = chasingPaladin;

    }
}
