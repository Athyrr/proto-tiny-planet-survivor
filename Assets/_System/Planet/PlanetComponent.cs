using UnityEngine;

public class PlanetComponent : MonoBehaviour
{
    //private PlanetData _planetData = null;

    [SerializeField]
    private LayerMask _surfaceLayer = ~0;

    private float _radius = 0f;


    // Normal calculation
    private Vector3 _cachedPlanetNormal;
    private Vector3 _lastCachedPosition;
    private float _lastNormalUpdate;

    private void Awake()
    {
        _radius = transform.localScale.x * 0.5f;
    }

    public float Radius => _radius;

    public Vector3 GetNormalAtPosition(Vector3 position)
    {
        bool timeDirty = Time.time - _lastNormalUpdate > 0.05f; // 5 frames threshold
        bool positionDirty = (position - _lastCachedPosition).sqrMagnitude > 0.2f * 0.2f; // 0.2m deplacement threshold

        if (timeDirty || positionDirty)
        {
            _cachedPlanetNormal = (position - transform.position).normalized;
            _lastNormalUpdate = Time.time;
            _lastCachedPosition = position;
        }

        return _cachedPlanetNormal;
    }

    /// <summary>
    /// Projects a vector onto the surface of the planet at a given position
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="atPostition"></param>
    /// <returns></returns>
    public Vector3 ProjectOnSurface(Vector3 vector, Vector3 atPostition)
    {
        Vector3 normal = GetNormalAtPosition(atPostition);
        return Vector3.ProjectOnPlane(vector, normal);
    }

    /// <summary>
    /// Snaps a position to the surface of the planet
    /// </summary>
    /// <param name="position"></param>
    /// <returns>Returns the snapped position.</returns>
    public Vector3 GetSnappedPosition(Vector3 position)
    {
        return GetSnappedPosition(position, 0f);
    }

    /// <summary>
    /// Snaps a position to the surface of the planet with an offset along the normal
    /// </summary>
    /// <param name="position"></param>
    /// <param name="offset"></param>
    /// <returns>Returns the snapped position with the defined offset.</returns>
    public Vector3 GetSnappedPosition(Vector3 position, float offset)
    {
        Vector3 direction = (position - transform.position).normalized;

        //Vector3 snapped = transform.position + (direction * _radius);
        Vector3 snapped = transform.position + (direction * transform.localScale.x * 0.5f);
        Vector3 normal = GetNormalAtPosition(snapped);
        return snapped + normal * offset;  // @todo add terrain height map
    }

    /// <summary>
    /// Get position on the surface of the planet after moving a certain distance from a point to another.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to">Point that defines a direction. Not a target point</param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public Vector3 GetSurfaceStep(Vector3 from, Vector3 to, float distance)
    {
        Vector3 tangentialDirection = ProjectOnSurface((to - from).normalized, from);
        Vector3 newPosition = from + tangentialDirection * distance;
        return GetSnappedPosition(newPosition);
    }

    /// <summary>
    /// Get the distance along the surface of the planet between two points.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public float GetSurfaceDistance(Vector3 from, Vector3 to)
    {
        Vector3 fromDir = (from - transform.position).normalized;
        Vector3 toDir = (to - transform.position).normalized;

        float angle = Vector3.Angle(fromDir, toDir) * Mathf.Deg2Rad;
        return angle * _radius;
    }

    /// <summary>
    /// Check if a target is within range from a center point along the planet surface.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="target"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool IsWithinRange(Vector3 center, Vector3 target, float range)
    {
        return GetSurfaceDistance(center, target) <= range;
    }

    /// <summary>
    /// Calculate trajectory from a point for a given direction along the planet curvature.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    /// <param name="deltaTime"></param>
    /// <param name="steps"></param>
    /// <returns>Returns the list of trajectory points.</returns>
    public Vector3[] CalculateTrajectory(Vector3 from, Vector3 direction, float speed, float deltaTime, int steps = 50)
    {
        Vector3[] trajectoryPoints = new Vector3[steps];
        Vector3 currentPoint = GetSnappedPosition(from);

        Vector3 currentDirection = ProjectOnSurface(direction, currentPoint);

        for (int i = 0; i < steps; i++)
        {
            trajectoryPoints[i] = currentPoint;

            currentPoint = GetSurfaceStep(currentPoint, currentPoint + currentDirection, speed * deltaTime);
            currentDirection = ProjectOnSurface(currentDirection, currentPoint);
        }

        return trajectoryPoints;
    }
}
