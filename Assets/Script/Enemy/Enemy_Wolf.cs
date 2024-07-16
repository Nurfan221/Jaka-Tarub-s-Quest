using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Wolf : MonoBehaviour
{
    /// <summary>
    /// 1. Spawn ketika raid
    /// 2. mengejar player
    /// </summary>

    Transform target;
    Enemy_Health eh;
    Rigidbody2D rb;
    AIDestinationSetter aids;
    AIPath path;
    SpriteRenderer sr;

    [SerializeField] bool drawSpawnRadius;

    [SerializeField] float moveSpd;
    [SerializeField] float runSpd;
    [SerializeField] float aggroDistance;
    [SerializeField] float attackDistance;
    [SerializeField] float idleDistance;
    Vector2 targetIdle;

    [SerializeField] GameObject attackHitBox;
    [SerializeField] float attackDur = .5f;
    [SerializeField] float attackDelay = 2f;
    float attackTimer, attackingTimer;

    [SerializeField] float waitTime = 2f;
    float waitTimer;

    bool caughtInCollision;


    private void Awake()
    {
        eh = GetComponent<Enemy_Health>();
        rb = GetComponent<Rigidbody2D>();
        aids = GetComponent<AIDestinationSetter>();
        path = GetComponent<AIPath>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        target = eh.player;
        aids.target = target;
    }

    // Update is called once per frame
    void Update()
    {
        if (path.desiredVelocity.x > 0)
        {
            sr.flipX = false;
        }
        else if (path.desiredVelocity.x < 0)
        {
            sr.flipX = true;
        }

        waitTimer += Time.deltaTime;
        if (waitTimer > waitTime)
        {
            if (IsInReach()) // Attack if in reach
            {
                //Try to attack player
                attackTimer += Time.deltaTime;
                if (attackTimer > attackDelay)
                {
                    attackHitBox.SetActive(true);
                    attackingTimer += Time.deltaTime;
                    if (attackingTimer > attackDur)
                    {
                        attackHitBox.SetActive(false);
                        attackingTimer = 0;
                        attackTimer = 0;
                        waitTimer = 0;
                    }
                }
            }
            else if (IsNearPlayer()) // Aggro if near
            {
                //rb.position = Vector2.MoveTowards(rb.position, target.position, runSpd * Time.deltaTime);
            }
            else // Idle if neither
            {
                // Target Idle is zero if it reach targetted idling pos
                if (targetIdle == Vector2.zero || caughtInCollision)
                {
                    caughtInCollision = false;
                    waitTimer = 0;
                    targetIdle.x = Random.Range(-idleDistance, idleDistance);
                    targetIdle.y = Random.Range(-idleDistance, idleDistance);
                    targetIdle += (Vector2)transform.position;
                }
                else
                {
                    rb.position = Vector2.MoveTowards(rb.position, targetIdle, moveSpd * Time.deltaTime);
                    if (Vector2.Distance(rb.position, targetIdle) < .5f)
                        targetIdle = Vector2.zero;
                }
            }
        }
    }

    bool IsNearPlayer()
    {
        return Vector2.Distance(transform.position, target.position) < aggroDistance;
    }

    bool IsInReach()
    {
        return Vector2.Distance(transform.position, target.position) < attackDistance;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        caughtInCollision = true;
    }

    #region DEBUG
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (drawSpawnRadius)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aggroDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackDistance);
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position, idleDistance);
        }
    }
#endif
    #endregion
}
