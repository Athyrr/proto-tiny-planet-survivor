using UnityEngine;

/// <summary>
/// Represents an object that <see cref="CollectorComponent"/> could pick up.
/// </summary>
public interface ICollectible
{
#pragma warning disable IDE1006 // Naming Styles
    GameObject gameObject { get; }
#pragma warning restore IDE1006 // Naming Styles

    /// <summary>
    /// Can thi object be collected?
    /// </summary>
    /// <param name="collector"></param>
    /// <returns></returns>
    bool CanCollect(ICollector collector);

    /// <summary>
    /// Makes this collectible beeing picked up by a given collector. Called when the collectible starts to be attracted.
    /// </summary>
    /// <param name="collector"></param>
    /// <returns></returns>
    bool Collect(ICollector collector);

    /// <summary>
    /// Applies the collectible effects on pickup animation ends. Could be increase exp for exemple.
    /// </summary>
    /// <param name="collector"></param>
    /// <returns></returns>
    bool ApplyEffects(ICollector collector, LevelComponent leveller);

}
