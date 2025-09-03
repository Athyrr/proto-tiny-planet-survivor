using UnityEngine;

[CreateAssetMenu(fileName = "NewDamagerData", menuName = Constants.CreateDataAssetMenu + "/Damager Data")]

public class DamagerData : ScriptableObject
{
    [Min(0)]
    public float Damage = 0f;

    [Min(0)]
    public float AttackRadius = 0f;

    [Min(0)]
    public float Cooldown = 0f;

    public LayerMask TargetLayer = ~0;

    public Color DebugColor = Color.clear;
}
