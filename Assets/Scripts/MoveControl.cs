using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveControl : MonoBehaviour
{
    public Mode movementMode = Mode.RandomMovement;
    [SerializeField]
    private List<Transform> wayPoints = new List<Transform>();

    public MovingType movingType;

    private float timeForIdle;
    private float maxDistanceForCast;
    private float percentageOfChangeDirection;
    private float distanceForAgression;
    private float closeAggressionDistance;
    private float movingSpeed;

    private float distanceToDestination = 0f;
    private Transform player;
    private bool changePath = false;
    [SerializeField]
    private States currentState;
    private AttackControl _ac;
    private int currentPoint = 0;
    private Vector3 destination;
    private NavMeshAgent agent;
    private float distanceToPlayer = 0f;
    private float rotationSpeed = 5f;
    private float range = 1f;
    private NavMeshObstacle obstacle;
    private Animator animator;
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
        //agent.stoppingDistance = 0.9f * range;
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("animator not assigned at " + transform.name);
            Debug.Break();
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
            Debug.LogError("hde igrok");
        SetPosition();
        agent.destination = destination;
        currentState = States.Walking;
        SetAnimatorState("walk");
        if (movementMode == Mode.RandomPointInFOW)
            StartCoroutine(ChangeDirection());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (currentState)
        {
            case States.Stay:
                MethodForIdle();
                break;
            case States.Walking:
                MethodForWalking();
                break;
            case States.Follow:
                MethodForFollowPlayer();
                break;
            case States.Attacking:
                MethodForFighting();
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
                    SetAnimatorState("walk");
                    currentState = States.Follow;
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
            case Mode.RandomPointInFOW:
                destination = Vector3.zero;
                while (destination == Vector3.zero)
                {
                    Vector3 direction = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * transform.forward;
                    RaycastHit hitInfo;
                    if (Physics.SphereCast(transform.position, 0.5f, direction, out hitInfo, maxDistanceForCast))
                    {
                        NavMeshHit nmHit;
                        if (NavMesh.SamplePosition(hitInfo.point, out nmHit, 1.5f, NavMesh.AllAreas))
                        {
                            destination = nmHit.position;
                        }
                        else
                        {
                            destination = Vector3.zero;
                        }
                    }
                    else
                    {
                        NavMeshHit nmHit;
                        if (NavMesh.SamplePosition(transform.position + direction * maxDistanceForCast, out nmHit, 1.5f, NavMesh.AllAreas))
                        {
                            destination = nmHit.position;
                        }
                        else
                        {
                            destination = Vector3.zero;
                        }
                    }
                }
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

    void MethodForIdle()
    {
        CheckForPlayer();
    }
    void MethodForWalking()
    {
        distanceToDestination = Vector3.Distance(transform.position, destination);
        if (distanceToDestination < 1f)
        {
            StartCoroutine(ChangePosition());
        }
        CheckForPlayer();
    }
    void MethodForFollowPlayer()
    {
        if (_ac.ChooseAttack() != -1)
            range = _ac.attackType[_ac.ChooseAttack()].range;
        if (Vector3.Distance(transform.position, player.position) <= range + 5f)
        {
            currentState = States.Attacking;
        }
        agent.destination = player.position;
    }
    void MethodForFighting()
    {
        MoveInBattle();

    }

    IEnumerator ChangePosition()
    {
        if (currentState == States.Walking || currentState == States.Stay)
        {
            currentState = States.Stay;
            SetAnimatorState("idle");
            yield return new WaitForSeconds(timeForIdle);
            SetAnimatorState("walk");
            SetPosition();
            agent.destination = destination;
            currentState = States.Walking;
        }
        yield return null;
    }
    IEnumerator ChangeDirection()
    {
        while (true)
        {
            if (currentState == States.Walking || currentState == States.Stay)
            {
                int rand = Random.Range(0, 200);
                if (rand < percentageOfChangeDirection)
                    changePath = true;
                if (changePath == true)
                {
                    SetPosition();
                    agent.destination = destination;
                    changePath = false;
                }
                yield return new WaitForSeconds(0.5f);

            }
        }
    }
    bool destinationReached = true;
    void MoveInBattle()
    {
        RaycastHit hitInfo;
        Vector3 directionNormalized = (player.position - transform.position).normalized;
        Physics.SphereCast(transform.position + new Vector3(0f, 0.5f, 0f), 0.3f, directionNormalized, out hitInfo, range);
        Debug.DrawRay(transform.position, directionNormalized);
        if (hitInfo.transform != null)
            Debug.Log((player.position - transform.position).magnitude.ToString() + ' ' + hitInfo.transform.ToString() + ' ' + range);
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
                SetAnimatorState("battle_idle");
            }
            else
            {
                NavMeshHit nvhit;
                if ((distanceToPlayer > range || hitInfo.transform != player) && NavMesh.SamplePosition(player.position, out nvhit, 1.5f, agent.areaMask))
                {
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
                    SetAnimatorState("walk");
                    agent.destination = player.position;
                }
                else
                {
                    if (hitInfo.transform != player && isRanged)
                    {
                        obstacle.enabled = false;
                        agent.enabled = true;
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
                                    if (Quaternion.Angle(transform.rotation, _rot) < 4f)
                                        SetAnimatorState("battle_idle");
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
        RandomPointInFOW
    }
