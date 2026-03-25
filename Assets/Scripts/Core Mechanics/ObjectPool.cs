using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private PoolableObject Prefab;
    private List<PoolableObject> AvailableObjects;

    private ObjectPool(PoolableObject Prefab, int size)
    {
        this.Prefab = Prefab;
        AvailableObjects = new List<PoolableObject>(size);
    }

    //Creates an object pool with Prefab and contains size amount of objects
    public static ObjectPool CreateInstance(PoolableObject Prefab, int size)
    {
        ObjectPool pool = new ObjectPool(Prefab, size);

        GameObject poolObject = new GameObject(Prefab.name + " Pool");
        pool.CreateObjects(poolObject.transform, size);

        return pool;
    }

    //Creates objects for object pool
    private void CreateObjects(Transform parent, int size)
    {
        for(int i = 0; i < size; i++)
        {
            //object is a game object with Prefab, set at position (0,0,0), with rotation (0,0,0,0), as a child of parent
            PoolableObject poolableObject = GameObject.Instantiate(Prefab, Vector3.zero, Quaternion.identity, parent.transform);
            poolableObject.parent = this;
            poolableObject.gameObject.SetActive(false);
        }
    }

    //Takes objects available to be in pool and adds them into the object pool
    public void ReturnObjectToPool(PoolableObject poolableObject)
    {
        AvailableObjects.Add(poolableObject);
    }

    //Removes an object from object pool and activates it in the game
    public PoolableObject GetObject()
    {
        if (AvailableObjects.Count > 0)
        {
            PoolableObject instance = AvailableObjects[0];
            AvailableObjects.RemoveAt(0);

            instance.gameObject.SetActive(true);

            return instance;
        }
        else
        {
            Debug.LogError("Cannot get object from " + Prefab.name + " Pool. Please check configuration before the game explodes.");
            return null;
        }
    }
}
