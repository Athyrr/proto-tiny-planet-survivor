/// <summary>
/// Interface for arena managers that manage one of a planet's systems.
/// </summary>
public interface IArenaManager
{
    /// <summary>
    /// Is the manager initialized?
    /// </summary>
    bool IsManagerIntialized { get; }

    bool Initialize(PlanetData planetData, PlanetComponent planet, PlayerControllerComponent player);
}
