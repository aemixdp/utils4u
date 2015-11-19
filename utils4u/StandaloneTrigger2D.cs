using UnityEngine;

public abstract class StandaloneTrigger2D<ColliderType> : MonoBehaviour
    where ColliderType : Collider2D
{
    private ColliderType _collider;
    public ColliderType Collider { get { return _collider; } }

    public delegate void TriggerHandler(Collider2D other);
    public event TriggerHandler TriggerEnter2D;

    void OnTriggerEnter2D(Collider2D other)
    {
        TriggerEnter2D(other);
    }

    protected static TriggerType Add<TriggerType>(GameObject parent)
        where TriggerType : StandaloneTrigger2D<ColliderType>
    {
        var dummy = new GameObject();
        dummy.transform.parent = parent.transform;
        var trigger = dummy.AddComponent<TriggerType>();
        trigger._collider = dummy.AddComponent<ColliderType>();
        trigger._collider.isTrigger = true;
        return trigger;
    }
}

public class StandaloneCircleTrigger2D : StandaloneTrigger2D<CircleCollider2D>
{
    public static StandaloneCircleTrigger2D Add(GameObject parent)
    {
        return Add<StandaloneCircleTrigger2D>(parent);
    }
}
