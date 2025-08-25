
public interface ITickable
{
    public bool IsActive { get; }

    public void Tick(float deltaTime);
}
