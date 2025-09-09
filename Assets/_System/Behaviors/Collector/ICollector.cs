using UnityEngine;

/// <summary>
/// Represents an object able to pick up collectibles.
/// </summary>
public interface ICollector
{

#pragma warning disable IDE1006 // Naming Styles
    GameObject gameObject { get; }
#pragma warning restore IDE1006 // Naming Styles

}