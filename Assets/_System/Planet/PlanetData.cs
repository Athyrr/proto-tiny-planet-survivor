using UnityEngine;

/// <summary>
/// Contains datas of a planet, characteristics, environment, and inhabitants.
/// </summary>
[CreateAssetMenu(fileName = "NewPlanetData", menuName = Constants.CreateDataAssetMenu + "/Planet Data")]
public class PlanetData : ScriptableObject
{
    public string DisplayName = "New Planet";
}
