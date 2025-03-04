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

        /* STAGE 2: create the transitions with their logic(s)
         * ---------------------------------------------------

        Transition varName = new Transition("TransitionName",
            () => { }, // write the condition checkeing code in {}
            () => { }  // write the on trigger code in {} if any. Remove line if no on trigger action needed
        );

        */
        Transition chaseGuide = new Transition("Chase_Guide",
            () => { return blackboard.timer <= 0 && SensingUtils.DistanceToTarget(gameObject, blackboard.guide) <= blackboard.scareRange;  }, // write the condition checkeing code in {}
            () => { }  // write the on trigger code in {} if any. Remove line if no on trigger action needed
        );
                Transition quitChasingGuide = new Transition("Quit_Chasing_Guide",
            () => {return blackboard.timer >= 5;}, // write the condition checkeing code in {}
            () => { }  // write the on trigger code in {} if any. Remove line if no on trigger action needed
        );

        /* STAGE 3: add states and transitions to the FSM 
         * ----------------------------------------------

        AddStates(...);

        AddTransition(sourceState, transition, destinationState);

         */ 
        AddStates(chasingPaladin, scareGuide);
        AddTransition(chasingPaladin, chaseGuide, scareGuide);
        AddTransition(scareGuide, quitChasingGuide, chasingPaladin);

        /* STAGE 4: set the initial state
         
        initialState = ... 

         */
         initialState = chasingPaladin;

    }
}
