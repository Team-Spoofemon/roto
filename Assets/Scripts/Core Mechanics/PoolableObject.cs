using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    public ObjectPool parent;

    //Returns object to the pool when object is disabled
    public virtual void OnDisable()
    {
        parent.ReturnObjectToPool(this);
    }
}
