using UnityEngine;
using UnityEngine.UI;

public class UIExpBarComponent : MonoBehaviour
{
    [SerializeField]
    private LevelComponent _playerlevel = null;

    [SerializeField]
    private Slider _slider = null;

    private void Awake()
    {
        if (_playerlevel == null)
        {
            var player = FindFirstObjectByType<LevelComponent>();
            if (!player.TryGetComponent<LevelComponent>(out var playerLevel))
            {
                Debug.LogError($" Player {typeof(LevelComponent)} component not found");
                return;
            }
            _playerlevel = playerLevel;
        }
    }

    private void Start()
    {
        _slider.enabled = true;

        _slider.value = 0;
        //_slider.maxValue = _playerlevel.LevelExpRequired;
        _slider.maxValue = 1;
    }

    private void OnEnable()
    {
        if (_playerlevel == null)
            return;

        _playerlevel.OnGainExp += HandleGainExp;
        _playerlevel.OnLevelChange += HandleLevelChange;
    }

    private void HandleGainExp(float gain, float exp)
    {
        float ratio = Mathf.Clamp01(exp / _playerlevel.LevelExpRequired);
        _slider.value = ratio;

        Debug.Log("Gain Exp");
    }

    private void HandleLevelChange(int level)
    {
        float ratio = Mathf.Clamp01(_playerlevel.Exp / _playerlevel.LevelExpRequired);
        _slider.value = ratio;
    }
}