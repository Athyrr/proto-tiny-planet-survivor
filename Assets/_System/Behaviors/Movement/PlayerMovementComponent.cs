using UnityEngine;

public class PlayerMovementComponent : BaseMovementComponent
{
    #region Fields

    private Vector3 _previousDirection = Vector3.zero;

    private bool _wasMoving = false;

    #endregion


    #region Lifecycle

    protected override bool Init()
    {
        base.Init();

        SetPlanetCenter(FindFirstObjectByType<PlanetComponent>()?.transform);

        _speed = _data.Speed;
        _maxSpeed = _data.MaxSpeed;

        return true;
    }

    #endregion


    #region Public API

    public override bool Move(Vector3 direction, float delta)
    {
        //Debug.Log($"Move called with direction: {direction}, delta: {delta}");

        // Acceleration
        if (direction.magnitude > 0.1f)
        {
            Vector3 targetVelocity = direction.normalized * _speed;
            _velocity = Vector3.MoveTowards(_velocity, targetVelocity, _data.Acceleration * delta);

            if (!_wasMoving)
                base.OnMoveStart?.Invoke(this, direction, _velocity.magnitude);
        }
        // Deceleration
        else
        {
            _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, _data.Deceleration * delta);

            if (_velocity.magnitude <= 0.01f && _wasMoving)
                base.OnMoveEnd?.Invoke(this, direction, _velocity.magnitude);
            
            //Debug.Log("Velocity: " + _velocity);
        }

        // Position
        ApplyMovementToPlanet(delta, out Vector3 finalPosition);

        // Rotation
        AlignWithPlanet();

        _previousDirection = direction;
        _wasMoving = _velocity.magnitude > 0.01f;

        if (_velocity.magnitude > 0.01f)
            base.OnMoveUpdate?.Invoke(this, _velocity.normalized, _velocity.magnitude);

        return _velocity.magnitude > 0.01f;
    }

    #endregion


    #region Debug

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + GetPlanetNormal(transform.position) * 5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + _velocity.normalized * _velocity.magnitude);
    }

    #endregion
}