using System;
using UnityEngine;

/// <summary>
/// Represents the ability to levelling up of an entity.
/// </summary>
public class LevelComponent : MonoBehaviour, IUpgradableBehavior
{

    #region Delegates

    public delegate void OnLevelChangeDelegate(int level);
    public OnLevelChangeDelegate OnLevelChange = null;

    public delegate void OnGainExpDelegate(float gain, float exp);
    public OnGainExpDelegate OnGainExp = null;

    #endregion


    #region Fields

    [SerializeField]
    private int _maxLevel = 0;

    [SerializeField]
    [Tooltip("@todo temp static value. Later, make that a level has its own scalable amount of exp required.")]
    private float _levelExpRequired = 0;

    private int _level = 0;

    private float _exp = 0;

    #endregion


    #region Lifecycle
    #endregion


    #region Public API

    public int Level => _level;

    public float Exp => _exp;

    public int MaxLevel => _maxLevel;

    public float LevelExpRequired => _levelExpRequired;

    public bool LevelUp()
    {
        if (_level >= _maxLevel)
            return false;

        _level++;

        // @todo exp required for levels formula here
        _levelExpRequired *=  2.5f;

        OnLevelChange?.Invoke(_level);
        return true;
    }

    public bool CanLevelUp()
    {
        return true;
    }

    public bool GainExp(float gain)
    {
        if (gain <= 0)
            return false;

        if (_level >= _maxLevel && _exp >= _levelExpRequired)
            return false;

        _exp += gain;

        if (_exp >= _levelExpRequired)
            LevelUp();

        OnGainExp?.Invoke(gain, _exp);

        return true;
    }

    #endregion

}
