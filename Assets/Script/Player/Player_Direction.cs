using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Direction : MonoBehaviour
{
    public static Player_Direction Instance;
    public Transform Target;

    [SerializeField] Transform arrow;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Target != null)
        {
            arrow.gameObject.SetActive(true);
            Vector2 rotation = Target.position - transform.position;
            float rot = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
            arrow.eulerAngles = new(0, 0, rot);
        }
        else
        {
            arrow.gameObject.SetActive(false);
        }
    }
}
