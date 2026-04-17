using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    public ObjectPool parent;

    public virtual void OnDisable()
    {
        if (parent != null)
            parent.ReturnObjectToPool(this);
    }
}