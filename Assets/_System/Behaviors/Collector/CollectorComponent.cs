using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an ability to collect an <see cref="ICollectible"/>.
/// </summary>
/// <remarks>
/// The component use a sphere collider in trigger mode to detect collectibles.
/// If you need to add a new type of collector, ensure that you added a new sphere collider and assign it to this component in the inspector. If you need a custom editor, ask for Adam.
/// </remarks>
[RequireComponent(typeof(SphereCollider))]
public abstract class CollectorComponent<T> : MonoBehaviour, ICollector, IUpgradableBehavior where T : ICollectible
{
    #region Sub-class

    /// <summary>
    /// Represents a pickup animation currently playing for a <see cref="ICollectible"/>
    /// </summary>
    private class CollectibleAnimation
    {
        public T Collectible = default;

        public float Timer = 0f;

        public Vector3 StartPosition = Vector3.zero;

        public CollectibleAnimation(T collectible, float timer)
        {
            Collectible = collectible;
            Timer = timer;
            StartPosition = Collectible.gameObject.transform.position;
        }
    }

    #endregion


    #region Delegates

    public delegate void OnCollectDelegate(T collectible);
    public event OnCollectDelegate OnCollectItem = null;

    public delegate void OnItemCollectedDelegate(T collectible);
    public event OnItemCollectedDelegate OnItemCollected = null;

    #endregion


    #region Fields

    [Header("Refs")]

    [SerializeField]
    private SphereCollider _triggerCollider = null;

    [Header("Base Settings")]

    [Min(0)]
    [Tooltip("Range within the entity collects an item.")]
    public float BaseCollectRange = 1f;

    [Tooltip("Collectible layer.")]
    public LayerMask CollectibleLayer = ~0;

    [Header("Collect Animation")]

    [Min(0)]
    [Tooltip("The duration (in seconds) of the \"magnet\" animation for a collected object. Note that this also define the time before the object is effectively collected.")]
    public float CollectAnimationDuration = 0.25f;

    [Tooltip("Defines how the collected objects are animated: X axis is % of the animation time, Y axis is the lerp value to apply from the start position of the collected object to this entity.")]
    public AnimationCurve CollectAnimationCurve = AnimationCurve.Constant(0, 0, 0);

    [Header("Debug")]

    [SerializeField]
    private bool _drawDebug = false;

    [SerializeField]
    private Color _debugColor = Color.red;


    ///<inheritdoc cref="LevelComponent"/>
    protected LevelComponent _playerLevel = null;

    /// <summary>
    /// Collectibles currently animated for collecting.
    /// </summary>
    private List<CollectibleAnimation> _collectibleAnimations = new();

    private Coroutine _collectibleAnimationsCoroutine = null;

    private float _collectRange = 0f;


    #endregion


    #region Lifecycle

    private void Awake()
    {
        _triggerCollider.isTrigger = true;
        _triggerCollider.radius = BaseCollectRange;

        _collectRange = BaseCollectRange;
    }

    private void Start()
    {
        TryGetComponent<LevelComponent>(out _playerLevel);
    }

    protected virtual void Update()
    {
        // Start the collect animations coroutine if needed
        if (_collectibleAnimationsCoroutine == null && _collectibleAnimations.Count > 0)
            _collectibleAnimationsCoroutine = StartCoroutine(AnimateCollectCouroutine());
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<T>(out T collectible))
        {
            if (CanCollect(collectible))
                Collect(collectible);
        }
    }

    #endregion


    #region Public API

    public SphereCollider TriggerCollider => _triggerCollider;
    public float CollectRange => _collectRange;

    public bool SetCollectRange(float range)
    {
        if (range == _collectRange)
            return false;

        if (_triggerCollider == null)
            return false;

        _collectRange = Mathf.Max(0, range);
        _triggerCollider.radius = _collectRange;
        return true;
    }

    public bool CanCollect(ICollectible collectible)
    {
        if (collectible == null)
            return false;

        foreach (var item in _collectibleAnimations)
        {
            if ((ICollectible)item.Collectible == collectible)
                return false;
        }

        return collectible.CanCollect(this);
    }

    /// <summary>
    /// Animate a collectible beeing collected.
    /// </summary>
    /// <param name="collectible"></param>
    /// <returns></returns>
    public bool Collect(T collectible)
    {
        if (!CanCollect(collectible))
            return false;

        // Start the collect animation
        collectible.Collect(this); //@todo Handle collectible.Collect by listening the delegate and remove here.
       
        OnCollectItem?.Invoke(collectible);

        _collectibleAnimations.Add(new CollectibleAnimation(collectible, 0));
        return true;
    }

    #endregion


    #region Protected API

    /// <summary>
    ///DEPRECATED
    ///We use collider instead.
    /// </summary>
    protected void DetectCollectibles()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _collectRange, CollectibleLayer);

        if (colliders.Length <= 0)
            return;

        // For each collider in range
        foreach (Collider col in colliders)
        {
            if (!col.TryGetComponent<T>(out T collectible))
                continue;

            Collect(collectible);
        }
    }

    /// <summary>
    /// Applies player and collectible effects when collectibles ends its anim.
    /// </summary>
    /// <returns></returns>
    protected virtual bool ProcessCollectEffects(T collectible)
    {
        if (collectible == null)
            return false;

        //@todo Play player collect feedbacks when it collects.

        OnItemCollected?.Invoke(collectible);

        return collectible.ApplyEffects(this, _playerLevel);
    }

    // Play the animation of each collectibles detected.
    protected virtual IEnumerator AnimateCollectCouroutine()
    {
        while (_collectibleAnimations.Count > 0)
        {
            for (int i = _collectibleAnimations.Count - 1; i >= 0; i--)
            {
                CollectibleAnimation animation = _collectibleAnimations[i];

                if (animation == null || animation.Collectible.gameObject == null)
                {
                    _collectibleAnimations.RemoveAt(i);
                    continue;
                }

                animation.Timer += Time.deltaTime;

                float ratio = CollectAnimationDuration > 0 ? animation.Timer / CollectAnimationDuration : 1;

                animation.Collectible.gameObject.transform.position = Vector3.LerpUnclamped(animation.StartPosition, transform.position, CollectAnimationCurve.Evaluate(ratio));

                if (ratio >= 1)
                {
                    ProcessCollectEffects(animation.Collectible);

                    _collectibleAnimations.RemoveAt(i);
                    Destroy(animation.Collectible.gameObject);
                }
            }
            yield return null;
        }


        StopCoroutine(_collectibleAnimationsCoroutine);
        _collectibleAnimationsCoroutine = null;
    }

    #endregion


    #region Debug   

    private void OnGUI()
    {
        if (_drawDebug)
            DrawCollectiblesStats();
    }

    private void OnDrawGizmosSelected()
    {
        if (!_drawDebug)
            return;

        Gizmos.color = _debugColor;
        Gizmos.DrawWireSphere(_triggerCollider.transform.position, CollectRange);
    }

    private void DrawCollectiblesStats()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Color.white;

        var animationCount = _collectibleAnimations.Count;
        var range = _triggerCollider.radius;

        string statsText = $" {typeof(T)} Collector Performance:\n" +
                          $"Range: {range}\n" +
                          $"Collectibles animation: {animationCount}";

        GUI.Label(new Rect(10, 10, 300, 200), statsText, style);
    }

    #endregion

}
