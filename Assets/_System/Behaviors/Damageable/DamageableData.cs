
using UnityEngine;

[CreateAssetMenu(fileName = "NewDamageableData", menuName = Constants.CreateDataAssetMenu + "/Damageable Data")]
public class DamageableData : ScriptableObject
{
    [Min(0)]
    public float MaxHealth = 0f;

    public bool IsInvicible = false;
}
