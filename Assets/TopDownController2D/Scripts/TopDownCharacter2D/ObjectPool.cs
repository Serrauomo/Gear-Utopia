using System.Collections.Generic;
using UnityEngine;

namespace TopDownCharacter2D
{
    /// <summary>
    ///     Handles a pool of objects
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool sharedInstance;

        [SerializeField] private List<GameObject> pooledObjects;
        [SerializeField] private GameObject objectToPool;
        [SerializeField] private int amountToPool;

        private void Awake()
        {
            sharedInstance = this;
        }

        private void Start()
        {
            pooledObjects = new List<GameObject>();
            for (int i = 0; i < amountToPool; i++)
            {
                GameObject tmp = Instantiate(objectToPool);
                tmp.SetActive(false);
                pooledObjects.Add(tmp);
            }
        }

        /// <summary>
        ///     Returns an object from the pool
        /// </summary>
        /// <returns></returns>
        public GameObject GetPooledObject()
        {
            for (int i = 0; i < amountToPool; i++)
            {
                if (!pooledObjects[i].activeInHierarchy)
                {
                    return pooledObjects[i];
                }
            }

            return null;
        }
    }
}