/// <summary>
/// Represents an ability in runtime.
/// </summary>
public class Ability
{

    #region Fields

    private AbilitySO _data = null;

    #endregion


    #region Lifecycle

    ///<inheritdoc cref="Ability"/>
    public Ability(AbilitySO data)
    {
        _data = data;
    }

    #endregion


    #region Public API

    /// <summary>
    /// Use the ability.
    /// </summary>
    /// <returns></returns>
    public bool UseAbility()
    {
        return true;
    }

    #endregion

}
