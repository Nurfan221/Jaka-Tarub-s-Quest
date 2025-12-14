using UnityEngine;

public class ProjectileWeapons : MonoBehaviour
{
    [SerializeField] LayerMask ignoreThis;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            collision.GetComponent<Enemy_Health>().TakeDamage(10);
        }
        else if (collision.gameObject.layer == ignoreThis)
        {
            return;
        }
        Destroy(gameObject);
    }
}
