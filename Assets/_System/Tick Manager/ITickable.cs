
using UnityEngine;

public interface ITickable
{
    public bool IsActive { get; }

    public Vector3 Position { get; }

    public void Tick(float deltaTime);
}
