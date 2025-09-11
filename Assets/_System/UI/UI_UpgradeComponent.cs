using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_UpgradeComponent : MonoBehaviour
{

    #region Fields

    [SerializeField]
    private Image _icon;

    [SerializeField]
    private TMP_Text _name;

    [SerializeField]
    private TMP_Text _effects;

    [SerializeField]
    private Button _selectButton;

    private UI_UpgradeSelectionComponent _selectionManager;
    private UpgradeSO _upgrade;

    #endregion


    #region Lifecycle

    private void Awake()
    {
        if (_selectButton == null)
            _selectButton = GetComponent<Button>();

        if (_selectButton != null)
            _selectButton.onClick.AddListener(OnUpgradeSelected);
    }

    private void OnDestroy()
    {
        if (_selectButton != null)
            _selectButton.onClick.RemoveListener(OnUpgradeSelected);
    }

    #endregion


    #region Public API

    public UpgradeSO Upgrade => _upgrade;

    public void Initialize(UI_UpgradeSelectionComponent selectionManager, UpgradeSO upgradeData)
    {
        _selectionManager = selectionManager;
        SetUpgradeData(upgradeData);
    }

    public void SetUpgradeData(UpgradeSO upgrade)
    {
        if (upgrade == null)
            return;

        if (_upgrade == upgrade)
            return;

        _upgrade = upgrade;

        RefreshValues();
    }

    #endregion


    #region Private API

    private void RefreshValues()
    {
        if (_upgrade == null)
            return;

        if (_icon != null && _upgrade.Icon != null)
            _icon.sprite = _upgrade.Icon;

        if (_name != null)
            _name.text = _upgrade.DisplayName ?? _upgrade.name;

        if (_effects != null)
            _effects.text = _upgrade.Description ?? "No description available";
    }

    private void OnUpgradeSelected()
    {
        if (_upgrade != null && _selectionManager != null)
        {
            _selectionManager.SelectUpgrade(_upgrade);
        }
    }

    #endregion

}
