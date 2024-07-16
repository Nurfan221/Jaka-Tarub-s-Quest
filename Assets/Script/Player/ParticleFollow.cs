using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFollow : MonoBehaviour
{
    public static ParticleFollow Instance;

    [SerializeField] Transform particleTransform;
    ParticleSystem particle;
    [SerializeField] List<Transform> positions;

    bool pathing = false;

    float totalDistance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        particle = particleTransform.GetComponent<ParticleSystem>();
        for (int i = 0; i < positions.Count - 1; i++)
        {
            totalDistance = Vector2.Distance(positions[i].position, positions[i + 1].position);
        }
    }

    public void StartPath(float range = 1)
    {
        if (!pathing)
        {
            particleTransform.position = positions[0].position;
            transform.GetChild(0).localScale = new(range, 1, 1);
            particle.Play();
            StartCoroutine(Pathing());
        }
    }

    IEnumerator Pathing()
    {
        pathing = true;
        foreach (Transform target in positions)
        {
            while (Vector2.Distance(particleTransform.position, target.position) > .1f)
            {
                GoToPos(target);
                yield return null;
            }
        }
        pathing = false;
    }

    void GoToPos(Transform target)
    {
        particleTransform.position = Vector2.MoveTowards(particleTransform.position, target.position, totalDistance / .4f);
    }
}
