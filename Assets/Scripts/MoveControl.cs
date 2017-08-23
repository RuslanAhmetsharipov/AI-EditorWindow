using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveControl : MonoBehaviour
{
    public Mode movementMode = Mode.RandomMovement;
    public NPCType npcType = NPCType.agressiveMonster;
    public List<Transform> wayPoints = new List<Transform>();

    public MovingType movingType;
    public Animator animator;
    public bool targetDefined = false;

    private float timeForIdle;
    private float maxDistanceForCast;
    private float percentageOfChangeDirection;
    private float distanceForAgression;
    private float closeAggressionDistance;
    private float movingSpeed;

    private float distanceToDestination = 0f;
    private Transform player;
    private bool changePath = false;
    private bool changeAnim = false;
    private States currentState;
    private AttackControl _ac;
    private int currentPoint = 0;
    private Vector3 destination;
    private NavMeshAgent agent;
    private float distanceToPlayer = 0f;
    private float rotationSpeed = 5f;
    private float range = 1f;
    private NavMeshObstacle obstacle;
    // Use this for initialization
    void Start()
    {
        if (movingType == null)
        {
            this.gameObject.SetActive(false);
        }
        _ac = GetComponent<AttackControl>();
        timeForIdle = movingType.timeForIdle;
        maxDistanceForCast = movingType.aggressionDistanceInFront;
        percentageOfChangeDirection = movingType.changeDirectionPercentage;
        distanceForAgression = movingType.aggressionDistanceInFront;
        closeAggressionDistance = movingType.closeAggressionDistance;
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        movingSpeed = movingType.speed;
        agent.speed = movingSpeed;
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("animator not assigned at " + transform.name);
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
            Debug.LogError("Player not assigned");
        if (npcType == NPCType.agressiveMonster || npcType == NPCType.non_agressiveMonster || npcType == NPCType.peacefulNPC)
        {
            SetPosition();
            agent.destination = destination;
            currentState = States.Walking;
            if (animator != null)
                SetAnimatorState("walk");
            if (movementMode == Mode.RandomMovement)
                StartCoroutine(ChangeDirection());
        }
        else
        {
            SetAnimatorState("idle");
            StartCoroutine(ChangeIdleAnim());
            targetDefined = false;
        }
    }

    void Update()
    {
        switch (npcType)
        {
            case NPCType.agressiveMonster:
                MoveAI(targetDefined);
                break;
            case NPCType.non_agressiveMonster:
                MoveAI(targetDefined, false);
                break;
            case NPCType.peacefulNPC:
                MoveAI(false, false);
                break;
            case NPCType.standOnePlaceNPC:
                //Add interaction system for your NPC here
                break;
            default:
                break;
        }
    }
    void MoveAI(bool enemyDefined, bool agressive = true, bool canMove = true)
    {
        switch (currentState)
        {
            case States.Stay:
                MethodForIdle(agressive, canMove);
                break;
            case States.Walking:
                MethodForWalking(enemyDefined, agressive);
                break;
            case States.Follow:
                MethodForFollowPlayer();
                break;
            case States.Attacking:
                MethodForFighting();
                break;
            default:
                break;
        }
    }
    Vector3 PickPosition()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        // Pick the first indice of a random triangle in the nav mesh
        int t = Random.Range(0, navMeshData.indices.Length - 3);

        // Select a random point on it
        Vector3 point = Vector3.Lerp(navMeshData.vertices[navMeshData.indices[t]], navMeshData.vertices[navMeshData.indices[t + 1]], Random.value);
        Vector3.Lerp(point, navMeshData.vertices[navMeshData.indices[t + 2]], Random.value);

        return point;
    }
    void CheckForPlayer()
    {
        float _distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 _direction = player.position - transform.position;
        if ((_distanceToPlayer < distanceForAgression && Vector3.Dot(transform.forward, _direction) > 0f) || _distanceToPlayer < closeAggressionDistance)
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, 0.1f, _direction, out hitInfo))
            {
                if (hitInfo.transform == player)
                {
                    StopAllCoroutines();
                    if (animator != null)
                        SetAnimatorState("walk");
                    targetDefined = true;
                }
            }
        }
    }

    void SetPosition()
    {
        switch (movementMode)
        {
            case Mode.RandomMovement:
                destination = PickPosition();
                break;
            case Mode.Waypoints:
                destination = wayPoints[currentPoint].position;
                distanceToDestination = Vector3.Distance(transform.position, destination);
                if (distanceToDestination < 1f)
                {
                    currentPoint++;
                    if (currentPoint >= wayPoints.Count)
                        currentPoint -= wayPoints.Count;
                    destination = wayPoints[currentPoint].position;
                }
                break;
        }
    }
    IEnumerator ChangePosition()
    {
        if (currentState == States.Walking || currentState == States.Stay)
        {
            yield return new WaitForSeconds(timeForIdle);

            if (animator != null)
                SetAnimatorState("walk");
            SetPosition();
            agent.destination = destination;
            currentState = States.Walking;

        }
        yield return null;
    }

    IEnumerator ChangeIdleAnim()
    {
        while (true)
        {
            float rand = Random.Range(0f, 100f);
            if (rand < percentageOfChangeDirection)
                changeAnim = true;
            if (changeAnim == true)
            {
                SetAnimatorState("idle");
                changeAnim = false;
            }
            yield return new WaitForSeconds(1f);
        }
    }
    IEnumerator ChangeDirection()
    {
        while (true)
        {
            if (currentState == States.Walking || currentState == States.Stay)
            {
                float rand = Random.Range(0f, 100f);
                if (rand < percentageOfChangeDirection)
                    changePath = true;
                if (changePath == true)
                {
                    SetPosition();
                    agent.destination = destination;
                    changePath = false;
                }
                yield return new WaitForSeconds(1f);

            }
        }
    }
    void MethodForIdle(bool agressive, bool canMove)
    {
        if (canMove)
        {
            StartCoroutine(ChangePosition());
            if (agressive)
            {
                CheckForPlayer();
            }
        }
    }
    void MethodForWalking(bool enemyDefined, bool agressive = true)
    {
        distanceToDestination = Vector3.Distance(transform.position, destination);
        if (distanceToDestination < 1f)
        {
            if (currentState == States.Walking || currentState == States.Stay)
                currentState = States.Stay;
        }
        if (agressive)
            CheckForPlayer();
        if (enemyDefined)
            currentState = States.Follow;
    }
    void MethodForFollowPlayer()
    {
        if (_ac.ChooseAttack() != -1)
            range = _ac.attackType[_ac.ChooseAttack()].range;
        if (Vector3.Distance(transform.position, player.position) <= range)
        {
            currentState = States.Attacking;
        }
        agent.destination = player.position;
    }
    void MethodForFighting()
    {
        MoveInBattle();
    }

    bool destinationReached = true;
    void MoveInBattle()
    {
        RaycastHit hitInfo;
        Vector3 directionNormalized = (player.position - transform.position).normalized;
        Physics.SphereCast(transform.position + new Vector3(0f, 0.5f, 0f), 0.3f, directionNormalized, out hitInfo, range);
        Debug.DrawRay(transform.position, directionNormalized);
        Quaternion _rot = Quaternion.LookRotation(directionNormalized);
        if (_ac.ChooseAttack() != -1)
            range = _ac.attackType[_ac.ChooseAttack()].range;
        bool isRanged = range > 2f;
        if (agent.enabled)
        {
            distanceToPlayer = agent.remainingDistance;
            distanceToDestination = Vector3.Distance(transform.position, destination);
            if (distanceToDestination <= 0.1f)
            {
                destinationReached = true;
            }
            if (distanceToPlayer <= range && destinationReached && hitInfo.transform == player)
            {

                agent.enabled = false;
                obstacle.enabled = true;
                if (animator != null)
                    SetAnimatorState("battle_idle");
            }
            else
            {
                NavMeshHit nvhit;
                if ((distanceToPlayer > range || hitInfo.transform != player) && NavMesh.SamplePosition(player.position, out nvhit, 1.5f, agent.areaMask))
                {
                    if (animator != null)
                        SetAnimatorState("walk");
                    agent.destination = nvhit.position;
                }
            }
        }
        else
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (hitInfo.transform != null && distanceToPlayer <= range)
            {
                if (hitInfo.transform != player)
                {
                    obstacle.enabled = false;
                    agent.enabled = true;
                    if (animator != null)
                        SetAnimatorState("walk");
                    agent.destination = player.position;
                }
                else
                {
                    if (hitInfo.transform != player && isRanged)
                    {
                        obstacle.enabled = false;
                        agent.enabled = true;
                        if (animator != null)
                            SetAnimatorState("walk");
                        RaycastHit hhit;
                        if (!Physics.SphereCast(transform.position, 0.5f, transform.right, out hhit, 0.3f) || hhit.transform.tag != "wall")
                        {
                            destination = transform.position + _rot * Vector3.right;
                            agent.destination = destination;
                            destinationReached = false;
                        }
                        else
                        {
                            if (!Physics.SphereCast(transform.position, 0.5f, -transform.right, out hhit, 0.3f) || hhit.transform.tag != "wall")
                            {
                                destination = transform.position - _rot * Vector3.right * 0.3f;
                                agent.destination = destination;
                                destinationReached = false;
                            }
                            else
                            {
                                if (!Physics.SphereCast(transform.position, 0.5f, transform.forward, out hhit, 0.3f) || hhit.transform.tag != "wall")
                                {
                                    destination = transform.position - _rot * Vector3.forward * 0.3f;
                                    agent.destination = destination;
                                    destinationReached = false;
                                }
                                else
                                {
                                    agent.enabled = false;
                                    obstacle.enabled = true;
                                    if (Quaternion.Angle(transform.rotation, _rot) < 1f)
                                    {
                                        if (animator != null)
                                            SetAnimatorState("battle_idle");
                                    }
                                    else
                                    {
                                        transform.rotation = Quaternion.Slerp(transform.rotation, _rot, Time.fixedDeltaTime * rotationSpeed);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (hitInfo.transform == player.transform)
                        {
                            agent.enabled = false;
                            obstacle.enabled = true;
                            if (animator != null)
                                SetAnimatorState("battle_idle");
                            if (Quaternion.Angle(transform.rotation, _rot) < 4f)
                            {
                                _ac.Attack();
                            }
                            else
                                transform.rotation = Quaternion.Slerp(transform.rotation, _rot, Time.fixedDeltaTime * rotationSpeed);
                        }

                    }
                }
            }
            else
            {
                if (distanceToPlayer > range)
                {
                    obstacle.enabled = false;
                    agent.enabled = true;
                    agent.destination = player.position;
                    if (animator != null)
                        SetAnimatorState("walk");
                }
            }
        }
    }
    private enum States
    {
        Walking,
        Stay,
        Follow,
        Attacking
    }
    public void SetAnimatorState(string state)
    {
        switch (state)
        {
            case "battle_idle":
                animator.SetBool("Can_attack", true);
                animator.SetBool("walking", false);
                animator.SetBool("IsAttackReady", false);
                break;
            case "attack":
                animator.SetBool("Can_attack", true);
                animator.SetBool("walking", false);
                animator.SetInteger("Rand_Attack", Random.Range(0, 10));
                animator.SetBool("IsAttackReady", true);
                break;
            case "idle":
                animator.SetBool("Can_attack", false);
                animator.SetBool("walking", false);
                animator.SetInteger("Rand_Idle", Random.Range(1, 7));
                break;
            case "walk":
                animator.SetBool("Can_attack", false);
                animator.SetBool("walking", true);
                break;
        }
    }
}
public enum Mode
{
    RandomMovement,
    Waypoints,
}
public enum NPCType
{
    agressiveMonster,
    non_agressiveMonster,
    peacefulNPC,
    standOnePlaceNPC
}