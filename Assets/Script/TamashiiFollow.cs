using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TamashiiFollow : MonoBehaviour
{
    public static TamashiiFollow Instance;

    [HideInInspector] public AIPath path;
    AIDestinationSetter aids;

    Transform player;
    public SpriteRenderer sr;
    [SerializeField] bool isFollowing;
    public bool IsFollowing { get { return isFollowing; } }

    private void Awake()
    {
        if(Instance==null)
            Instance = this;
        aids = GetComponent<AIDestinationSetter>();
        path = GetComponent<AIPath>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //aids.target = player;
    }

    // Update is called once per frame
    void Update()
    {
        path.canMove = isFollowing;

        if (path.desiredVelocity.x > 0)
        {
            sr.flipX = false;
        }
        else if (path.desiredVelocity.x < 0)
        {
            sr.flipX = true;
        }
    }

    public void SetFollowing(bool isFollowing)
    {
        this.isFollowing = isFollowing;
    }
}
