using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilitySO", menuName = Constants.CreateDataAssetMenu + "/Ability")]
public class AbilitySO : ScriptableObject
{

    #region Fields

    [SerializeField]
    private string _displayName = "Le string de Marius";

    [SerializeField]
    private Sprite _icon = null;

    //@todo fill properties

    #endregion


    #region Lifecycle
    #endregion


    #region Public API

    public Ability Build(AbilitySO data)
    {
        //@todo fill this shit

        return new Ability(data);
    }

    #endregion

}
