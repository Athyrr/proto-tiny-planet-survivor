using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class ExpGemComponent : MonoBehaviour, ICollectible
{

    #region Fields

    [SerializeField]
    private Rigidbody _rigidbody = null;

    [SerializeField]
    private SphereCollider _collider = null;

    #endregion


    #region Lifecycle

    private void Awake()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();

        if (_collider == null)
            _collider = GetComponent<SphereCollider>();

        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;

        _collider.isTrigger = true;
    }

    #endregion


    #region Public API

    ///<inheritdoc cref="ICollectible.CanCollect(ICollector)"/>
    public bool CanCollect(ICollector collector)
    {
        return true;
    }

    ///<inheritdoc cref="ICollectible.Collect(ICollector)"/>
    public bool Collect(ICollector collector)
    {
        return true;
    }

    ///<inheritdoc cref="ICollectible.ApplyEffects(ICollector)"/>
    public bool ApplyEffects(ICollector collector)
    {
        return true;
    }

    #endregion

}
