using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy_Cow : MonoBehaviour
{
    /// <summary>
    /// 1. Diam
    /// 2. akan kabur jika diserang
    /// 3. speednya tidak terlalu kencang
    /// 4. HP: Low
    /// </summary>

    Transform target;
    Enemy_Health eh;
    Rigidbody2D rb;

    [SerializeField] bool drawSpawnRadius;

    [SerializeField] float moveSpd;
    [SerializeField] float aggroDistance;
    [SerializeField] float idleDistance;
    Vector2 targetIdle;
    Vector2 runDir;

    [SerializeField] float waitTime = 4f;
    float waitTimer;

    bool caughtInCollision;
    private void Awake()
    {
        eh = GetComponent<Enemy_Health>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        target = eh.player;
    }

    // Update is called once per frame
    void Update()
    {
        waitTimer += Time.deltaTime;
        if (eh.justGotHit)
        {
            // Run Away
            runDir = transform.position - target.position;
            rb.position = Vector2.MoveTowards(rb.position, runDir.normalized * aggroDistance, moveSpd * Time.deltaTime);

            // If ran far enough, stop
            if (!IsNearPlayer())
                eh.justGotHit = false;
        }
        else if (waitTimer > waitTime)
        {
            // Target Idle is zero if it reach targetted idling pos
            if (targetIdle == Vector2.zero || caughtInCollision)
            {
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

    bool IsNearPlayer()
    {
        return Vector2.Distance(transform.position, target.position) < aggroDistance;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        caughtInCollision = true;
    }

    #region DEBUG
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (drawSpawnRadius)
            Gizmos.DrawWireSphere(transform.position, aggroDistance);
    }
#endif
    #endregion
}
