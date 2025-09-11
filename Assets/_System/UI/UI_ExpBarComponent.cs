using UnityEngine;
using UnityEngine.UI;

public class UI_ExpBarComponent : MonoBehaviour
{

    [SerializeField]
    private LevelComponent _playerLevel = null;

    [SerializeField]
    private Slider _slider = null;


    private void Awake()
    {
        if (_playerLevel == null)
        {
            var player = FindFirstObjectByType<LevelComponent>();
            if (!player.TryGetComponent<LevelComponent>(out var playerLevel))
            {
                Debug.LogError($" Player {typeof(LevelComponent)} component not found");
                return;
            }
            _playerLevel = playerLevel;
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
        if (_playerLevel == null)
            return;

        _playerLevel.OnGainExp += HandleGainExp;
        _playerLevel.OnLevelChange += HandleLevelChange;
    }

    private void HandleGainExp(float gain, float exp)
    {
        float ratio = Mathf.Clamp01(exp / _playerLevel.LevelExpRequired);
        _slider.value = ratio;
    }

    private void HandleLevelChange(int level)
    {
        float ratio = Mathf.Clamp01(_playerLevel.Exp / _playerLevel.LevelExpRequired);
        _slider.value = ratio;
    }

}