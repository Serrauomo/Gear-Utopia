using System.Collections.Generic;
using UnityEngine;

namespace TopDownCharacter2D
{
    [System.Serializable]
    public class PoolConfig
    {
        public string poolName;
        public GameObject prefab;
        public int poolSize;
        [HideInInspector] public List<GameObject> pooledObjects;
    }

    /// <summary>
    ///     Handles multiple pools of different objects
    /// </summary>
    public class MultiObjectPool : MonoBehaviour
    {
        public static MultiObjectPool sharedInstance;

        [SerializeField] private List<PoolConfig> pools;

        private void Awake()
        {
            sharedInstance = this;
        }

        private void Start()
        {
            foreach (PoolConfig pool in pools)
            {
                pool.pooledObjects = new List<GameObject>();
                for (int i = 0; i < pool.poolSize; i++)
                {
                    GameObject tmp = Instantiate(pool.prefab);
                    tmp.SetActive(false);
                    pool.pooledObjects.Add(tmp);
                }
            }
        }

        /// <summary>
        ///     Returns an object from a specific pool
        /// </summary>
        /// <param name="poolName">Name of the pool</param>
        /// <returns></returns>
        public GameObject GetPooledObject(string poolName)
        {
            PoolConfig targetPool = pools.Find(p => p.poolName == poolName);
            if (targetPool == null) return null;

            for (int i = 0; i < targetPool.pooledObjects.Count; i++)
            {
                if (!targetPool.pooledObjects[i].activeInHierarchy)
                {
                    return targetPool.pooledObjects[i];
                }
            }

            return null;
        }

        /// <summary>
        ///     Returns an object from the first pool (backward compatibility)
        /// </summary>
        public GameObject GetPooledObject()
        {
            if (pools.Count > 0)
                return GetPooledObject(pools[0].poolName);
            return null;
        }
    }
}