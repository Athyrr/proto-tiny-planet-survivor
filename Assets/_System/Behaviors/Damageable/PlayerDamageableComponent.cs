public class PlayerDamageableComponent : BaseDamageableComponent
{
    public override bool Heal(float amount)
    {
        return true;
    }

    public override bool TakeDamage(BaseDamagerComponent source, float amount)
    {
        return true;
    }
}
