using FSMs;
using UnityEngine;
using Steerings;
using Pathfinding;  // Asegúrate de incluir el namespace de A* Pathfinding
using System.Threading;

[CreateAssetMenu(fileName = "FSM_Guide", menuName = "Finite State Machines/FSM_Guide", order = 1)]
public class FSM_Guide : FiniteStateMachine
{
    private GUIDE_Blackboard blackboard;
    private PathFeeder pathFeeder;
    private GameObject wanderPoint;
    private GraphUpdateScene graphUpdateScene;  // Referencia a GraphUpdateScene
    private GameObject fleePoint; // Punto donde el Guide debe huir

    public override void OnEnter()
    {
        blackboard = GetComponent<GUIDE_Blackboard>();
        pathFeeder = GetComponent<PathFeeder>();
        graphUpdateScene = GetComponent<GraphUpdateScene>();  // Inicialización
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
                ApplyGraphUpdate();  // Aplicar actualización del gráfico cuando el Guide comienza a moverse
            },
            () => { },
            () => { }
        );

        State foundCultist = new State("Found_Cultist",
            () => { 
                pathFeeder.target = blackboard.cultist;
                ApplyGraphUpdate();  // Aplicar actualización al encontrar al Cultista
            },
            () => { },
            () => { }
        );

        State runAway = new State("Run_Away",
            () => {
                blackboard.timer = 0f;

                // Calcular la dirección de huida
                Vector3 fleeDirection = (gameObject.transform.position - blackboard.skeleton.transform.position).normalized;
                Vector3 fleePosition = gameObject.transform.position + fleeDirection * blackboard.runDistance;

                // Crear un nuevo punto de huida
                fleePoint = new GameObject("FleePoint");
                fleePoint.transform.position = fleePosition;

                pathFeeder.target = fleePoint;

                // Aplicar una actualización en el gráfico para crear un obstáculo dinámico en el camino de huida
                ApplyGraphUpdate();

                // Agregar obstáculos dinámicos a la ruta de huida para que el Guide sea influenciado por el GUO
                UpdateEscapeArea(fleePosition); // Actualiza la zona para crear obstáculos donde huye
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

    // Método para aplicar el cambio en el gráfico de navegación
    private void ApplyGraphUpdate()
    {
        if (graphUpdateScene != null)
        {
            graphUpdateScene.Apply();  // Aplica la actualización de la zona afectada
        }
    }

    // Método para agregar obstáculos dinámicos en la dirección de huida
    private void UpdateEscapeArea(Vector3 fleePosition)
    {
        if (graphUpdateScene != null)
        {
            // Definimos una zona alrededor del punto de huida para añadir obstáculos
            graphUpdateScene.points = new Vector3[]
            {
                fleePosition + new Vector3(2, 0, 2),  // Esquina superior derecha
                fleePosition + new Vector3(-2, 0, 2), // Esquina superior izquierda
                fleePosition + new Vector3(-2, 0, -2), // Esquina inferior izquierda
                fleePosition + new Vector3(2, 0, -2)  // Esquina inferior derecha
            };

            graphUpdateScene.modifyWalkability = true; // Modificar la caminabilidad
            graphUpdateScene.setWalkability = false;  // Marcar la zona como no caminable

            // Aplica la actualización para bloquear esa área
            graphUpdateScene.Apply();
        }
    }

    // Método para dibujar Gizmos y visualizar la zona de actualización
    private void OnDrawGizmos()
    {
        if (fleePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(fleePoint.transform.position, 1f);  // Dibujamos una esfera roja en el punto de huida
        }
    }
}
