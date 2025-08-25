using UnityEngine;

public class EnemyMovementComponent : BaseMovementComponent
{
    #region Fields

    [SerializeField]
    private Transform _playerTransform = null;

    #endregion


    #region Lifecycle

    protected override bool Init()
    {
        base.Init();

        _speed = _data.Speed;
        _maxSpeed = _data.MaxSpeed;

        return true;
    }

    #endregion


    #region Public API

    public override bool Move(Vector3 direction, float delta)
    {
        _velocity = Vector3.MoveTowards(_velocity, direction * _speed, _speed * delta);
        transform.position += _velocity * delta;

        ApplyMovementToPlanet(delta, out _);
        AlignWithPlanet();

        RotateTowardsTarget(_playerTransform, delta);

        return true;
    }

    #endregion


    #region Private API

    /// <summary>
    /// Rotate towards the target.
    /// </summary>
    /// <param name="deltaTime"></param>
    private void RotateTowardsTarget(Transform target, float deltaTime)
    {
        if (target == null)
            return;

        Vector3 planetNormal = GetPlanetNormal(transform.position);
        Vector3 targetDir = target.transform.position - transform.position;

        Vector3 projectedTargetDirection = Vector3.ProjectOnPlane(targetDir, planetNormal);
        projectedTargetDirection.Normalize();

        Vector3 newForward = Vector3.Slerp(transform.forward, projectedTargetDirection, _data.RotationSpeed * deltaTime);
        Quaternion targetRotation = Quaternion.LookRotation(newForward, planetNormal);

        transform.rotation = targetRotation;
    }

    #endregion
}
