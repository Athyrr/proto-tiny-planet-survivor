using UnityEngine;

public class ProjectileTest : MonoBehaviour
{

    public float _speed = 250;

    [Space]

    public float _boulderRadius = 0f;
    private float _currentAngle = 0f;

    private PlanetComponent _planet;
    private Transform _player;

    private Vector3 _currentDirection;

    private void Start()
    {
        _planet = FindFirstObjectByType<PlanetComponent>();
        _player = FindFirstObjectByType<PlayerControllerComponent>().transform;


        transform.position = _planet.GetSnappedPosition(transform.position);
        _currentDirection = _planet.ProjectOnSurface(transform.forward.normalized, transform.position);
    }

    private void Update()
    {
        //OrbitalBoulder();
        //JinxRocket();
        Orbit();
    }

    public void JinxRocket()
    {
        Vector3 to = transform.position + _currentDirection;
        Vector3 nextPos = _planet.GetSurfaceStep(transform.position, to, _speed * Time.deltaTime);
        transform.position = nextPos;

        _currentDirection = _planet.ProjectOnSurface(_currentDirection, nextPos);
    }


    public void OrbitalBoulder()
    {
        Vector3 normal = _planet.GetNormalAtPosition(transform.position);

        Vector3 playerDir = _player.position - transform.position;
        Vector3 tangent = Vector3.Cross(normal, playerDir).normalized;

        Vector3 newPos = _planet.GetSurfaceStep(transform.position, transform.position + tangent, _speed * Time.deltaTime);
        transform.position = newPos;
    }

    public void Orbit()
    {
        _currentAngle += _speed * Time.deltaTime;
        Vector3 orbitPosition = CalculateOrbitPosition();
        transform.position = _planet.GetSnappedPosition(orbitPosition);
    }


    private Vector3 CalculateOrbitPosition()
    {
        Vector3 playerPosition = _planet.GetSnappedPosition(_player.position);
        Vector3 playerNormal = _planet.GetNormalAtPosition(playerPosition);

        Vector3 tangent1 = Vector3.Cross(playerNormal, Vector3.up);
        if (tangent1.magnitude < 0.1f) // Si playerNormal est parallèle à Vector3.up
            tangent1 = Vector3.Cross(playerNormal, Vector3.right);
        tangent1.Normalize();

        Vector3 tangent2 = Vector3.Cross(playerNormal, tangent1).normalized;

        // Position d'orbite dans le plan tangent
        float angleRad = _currentAngle * Mathf.Deg2Rad;
        Vector3 orbitOffset = (tangent1 * Mathf.Cos(angleRad) + tangent2 * Mathf.Sin(angleRad)) * _boulderRadius;

        return playerPosition + orbitOffset;
    }
}
