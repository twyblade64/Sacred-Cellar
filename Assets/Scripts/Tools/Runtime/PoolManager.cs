using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton managed class
/// You can create pools of objects that have the IPoolable interface implemented in their classes and reuse them
/// </summary>
public class PoolManager : MonoBehaviour {
    // Static instance reference
    private static PoolManager instance;

    // Dictionary of pools
    private Dictionary<GameObject, IndexedPoolList> poolDictionary;

    // Structure for pool creation at PoolManager.Awake()
    [System.Serializable]
    public struct StartPoolObject {
        public GameObject prefab;
        public int number;
        public int limit;
    }

    // Start pool creation array
    public StartPoolObject[] startPool;


    /// <summary>
    /// Init references and create starting pools
    /// </summary>
    public void Awake() {
        instance = this;

        poolDictionary = new Dictionary<GameObject, IndexedPoolList>();

        for (int i = 0; i < startPool.Length; i++)
            poolDictionary.Add(startPool[i].prefab, new IndexedPoolList(startPool[i].prefab, startPool[i].number, startPool[i].limit));
    }

    /// <summary>
    /// Create a GameObject pool
    /// </summary>
    /// <param name="prefab">A GameObject Prefab that has a IPoolable component</param>
    /// <param name="size">Starting size of the pool</param>
    /// <param name="limit">Limit size for the pool, -1 for unlimited size.</param>
    public void CreatePool(GameObject prefab, int size = 0, int limit = -1) {
        if (prefab.GetComponent<IPoolable>() != null) {
            if (!poolDictionary.ContainsKey(prefab)) {
                IndexedPoolList ipl = new IndexedPoolList(prefab, size, limit);
                poolDictionary.Add(prefab, ipl);
            }
        } else {
            Debug.LogError("Pool cannot be created from " + prefab.name);
        }
    }

    /// <summary>
    /// Remove a GameObjects's object pool, destroying GameObjects in associated to it
    /// </summary>
    /// <param name="prefab">The pool's base GameObject</param>
    public void RemovePool(GameObject prefab) {
        IndexedPoolList ipl;
        if (poolDictionary.TryGetValue(prefab, out ipl)) {
            ipl.Destroy();
            poolDictionary.Remove(prefab);
        }
    }

    /// <summary>
    /// Get an instance of an object through an existing pool
    /// </summary>
    /// <param name="prefab">The GameObject you want the instance</param>
    /// <returns>A no-initiated GameObject instance. Call Init().</returns>
    public GameObject GetPoolInstance(GameObject prefab) {
        IndexedPoolList ipl;

        if (poolDictionary.TryGetValue(prefab, out ipl)) {
            return ipl.GetAvailable().PoolGameObject;
        }
        Debug.Log("Pool not found: " + prefab);
        return Instantiate(prefab);
    }

    public static PoolManager poolManager
    {
        get
        {
            return instance;
        }
    }

    /// <summary>
    /// Object pools are managed through a List container. Each IPoolable object has an Alive property used to know if they are busy or not.
    /// This class iterates through them as a circular list in order to find the next non-busy instance.
    /// </summary>
    class IndexedPoolList {
        public GameObject prefab;
        public List<IPoolable> objectList;
        public int index;
        public int limit;

        /// <summary>
        /// List-managed pool container.
        /// </summary>
        /// <param name="prefab">The Prefab asociated to the IndexedPoolList</param>
        /// <param name="size">The starting size of the list</param>
        /// <param name="limit">The limit for instance creation. Negative values set no limit.</param>
        public IndexedPoolList(GameObject prefab, int size, int limit = -1) {
            this.prefab = prefab;
            this.limit = limit;

            if (limit >= 0)

                size = Mathf.Max(Mathf.Min(size, limit), 0);
            objectList = new List<IPoolable>(size);
            
            for (int i = 0; i < size; ++i) {
                GameObject obj = Instantiate(prefab);
                obj.name = "Pool " + obj.name + " " + i;
                IPoolable poolObj = obj.GetComponent<IPoolable>();
                objectList.Add(poolObj);
                poolObj.InPool = true;
                poolObj.Clear();
            }
        }

        /// <summary>
        /// Get an Available instance of the GameObjects Pool.
        /// </summary>
        /// <returns>The IPoolable reference associated with the GameObject instance.</returns>
        public IPoolable GetAvailable() {
            
            int i = index;
            do {
                if (!objectList[i].Alive) {
                    index = (i + 1) % objectList.Count;
                    return objectList[i];
                }
                i = (i + 1) % objectList.Count;
            } while (i != index);

            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            IPoolable poolObj = obj.GetComponent<IPoolable>();

            if (limit == -1 || objectList.Count < limit) {
                obj.name = "Pool " + obj.name + " " + objectList.Count;

                objectList.Add(poolObj);
                poolObj.InPool = true;
            }
            return poolObj;
        }

        /// <summary>
        /// Remove all GameObject instance references
        /// </summary>
        public void Destroy() {
            foreach (IPoolable obj in objectList)
                GameObject.Destroy(obj.PoolGameObject);
        }
    }
}
