using UnityEngine;


[CreateAssetMenu(fileName = "NewMovementData", menuName = Constants.CreateDataAssetMenu + "/Movement Data")]
public class MovementData : ScriptableObject
{
    [Header("Speed")]

    [Min(0)]
    public float Speed = 0f;

    [Range(0, 150)]
    public float MaxSpeed = 0f;

    [Min(0)]
    public float Acceleration = 10f;

    [Min(0)]
    public float Deceleration = 10f;

    [Min(0)]
    public float RotationSpeed = 0f;

    [Header("Avoidance - @todo make avoidance settings only for NMEs (create EnemyMovementData)")]

    [Tooltip("Layer of objects to avoid overlapping. Could be enemies and obstacles.")]
    public LayerMask AvoidanceLayer = ~0;

    [Tooltip("Radius of objects detection to calculate avoidance.\n" +
        "Make sure that this setting is greater than 'AvoidanceMaxDistance'")]
    [Min(1)]
    public float AvoidanceDetectionRadius = 0f;

    [Tooltip("The distance below which avoidance is calculated.\n" +
        "@todo use animation curve instead.")]
    [Min(1)]
    public float AvoidanceMaxDistance = 0f;

    // @todo use animation curve instead of hard coded
    [Tooltip("Represents the force of the avoidance over distance with an object. \n" +
        "X axis: The distance. 0 is the object position. 1 is the min distance of avoidance. \n" +
        "Y axis: The avoidance force.")]
    public AnimationCurve AvoidanceForceCurve = AnimationCurve.EaseInOut(0, 0, 0, 0);

    [Header("Misc")]

    public bool SlowImmune = false;
}
