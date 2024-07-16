using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreesSorter : MonoBehaviour
{
    [SerializeField] Transform treesCollection;


    private void Awake()
    {
        Transform[] trees = new Transform[treesCollection.childCount];
        for (int i = 0; i < treesCollection.childCount; i++)
        {
            trees[i] = treesCollection.GetChild(i);
        }
        Array.Sort(trees, YPositionComparison);
        int index = 0;
        foreach (Transform t in trees)
        {
            t.GetComponent<SpriteRenderer>().sortingOrder = index;
            index--;
        }
    }

    int YPositionComparison(Transform a, Transform b)
    {
        if (a == null) return (b == null) ? 0 : -1;
        if (b == null) return 1;

        float ya = a.position.y;
        float yb = b.position.y;
        return ya.CompareTo(yb);
    }
}
