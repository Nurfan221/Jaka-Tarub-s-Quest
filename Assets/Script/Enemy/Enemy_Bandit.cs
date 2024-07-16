using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Enemy_Bandit : MonoBehaviour
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

    [SerializeField] bool drawAggroDistance;

    [SerializeField] float moveSpd;
    [SerializeField] float aggroDistance;
    Vector2 runDir;

    [SerializeField] GameObject attackHitBox;
    [SerializeField] float attackDur = .5f;
    [SerializeField] float attackDelay = 2f;
    float attackTimer, attackingTimer;

    [SerializeField] float waitTime = 2f;
    float waitTimer;


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
            if (!IsNearPlayer())
            {
                //rb.position = Vector2.MoveTowards(rb.position, target.position, moveSpd * Time.deltaTime);
            }
            else
            {
                //Try to attack player
                if (Time.time > attackTimer)
                {
                    attackHitBox.SetActive(true);
                    attackingTimer += Time.deltaTime;
                    if (attackingTimer > attackDur)
                    {
                        attackTimer = Time.time + attackDelay;
                        attackHitBox.SetActive(false);
                        attackingTimer = 0;
                        attackTimer = 0;
                        waitTimer = 0;
                    }
                }
            }
        }
    }

    bool IsNearPlayer()
    {
        return Vector2.Distance(transform.position, target.position) < aggroDistance;
    }

    #region DEBUG
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (drawAggroDistance)
            Gizmos.DrawWireSphere(transform.position, aggroDistance);
    }
#endif
    #endregion
}
