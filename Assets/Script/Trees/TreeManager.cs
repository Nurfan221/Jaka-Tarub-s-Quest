using System.Collections.Generic;
using UnityEngine;

public class TreeManager : MonoBehaviour
{
    [System.Serializable]
    public class TreeData
    {
        public Vector3 position;
        public GameObject treePrefab;
        public float respawnTime = 4f;  // Dalam hari (waktu game)
        public float timer;  // Waktu saat pohon ditebang
        public bool isRegrowing;
    }

    public List<TreeData> trees = new List<TreeData>();  // Menyimpan data pohon

    private void Start()
    {
        RegisterAllTrees();
        UpdateTreePositions();
    }

    // Fungsi untuk mendeteksi semua pohon dengan TreeBehavior
    private void RegisterAllTrees()
    {
        // Cari semua objek dengan script TreeBehavior
        TreeBehavior[] allTrees = FindObjectsOfType<TreeBehavior>();

        foreach (TreeBehavior tree in allTrees)
        {
            // Tambahkan pohon ke dalam daftar jika belum ada
            if (!IsTreeRegistered(tree.gameObject))
            {
                TreeData newTree = new TreeData
                {
                    treePrefab = tree.gameObject,
                    position = tree.transform.position
                };
                trees.Add(newTree);
            }
        }
    }

    // Cek apakah pohon sudah terdaftar
    private bool IsTreeRegistered(GameObject treeObject)
    {
        foreach (TreeData tree in trees)
        {
            if (tree.treePrefab == treeObject)
            {
                return true;  // Pohon sudah terdaftar
            }
        }
        return false;  // Pohon belum terdaftar
    }

    private void UpdateTreePositions()
    {
        foreach (TreeData tree in trees)
        {
            if (tree.treePrefab != null)
            {
                tree.position = tree.treePrefab.transform.position;
            }
        }
    }
}
    