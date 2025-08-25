using UnityEngine;


[CreateAssetMenu(fileName = "NewMovementData", menuName = Constants.CreateDataAssetMenu + "/Movement Data")]
public class MovementData : ScriptableObject
{
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

    public bool SlowImmune = false;
}
