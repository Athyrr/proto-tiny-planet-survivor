using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_UpgradeSelectionComponent : MonoBehaviour
{

    #region Fields

    ///<inheritdoc cref="UpgradesComponent"/>
    [SerializeField]
    private UpgradesComponent _upgrader = null;

    [SerializeField]
    private UI_UpgradeComponent _upgradePrefab;

    [SerializeField]
    private Transform _upgradesContainer = null;

    [SerializeField]
    private GameObject _selectionPanel = null;

    private List<UI_UpgradeComponent> _currentUpgradeUIs = new List<UI_UpgradeComponent>();

    #endregion


    #region Lifecycle

    private void Awake()
    {
        if (_upgrader == null)
            _upgrader = FindAnyObjectByType<UpgradesComponent>();

        if (_upgradesContainer == null)
            _upgradesContainer = transform;

        if (_selectionPanel != null)
            _selectionPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (_upgrader != null)
        {
            _upgrader.OnUpgradesPresented += ShowUpgradeSelection;
            _upgrader.OnUpgradeSelected += HideSelectionPanel;
        }
    }

    private void OnDisable()
    {
        if (_upgrader != null)
        {
            _upgrader.OnUpgradesPresented -= ShowUpgradeSelection;
            _upgrader.OnUpgradeSelected -= HideSelectionPanel;
        }
    }

    #endregion


    #region Public API

    public bool SelectUpgrade(UpgradeSO selectedUpgrade)
    {
        if (selectedUpgrade == null || _upgrader == null)
            return false;

        var correspondingUpgrade = _currentUpgradeUIs.FirstOrDefault(ui => ui.Upgrade == selectedUpgrade);
        if (correspondingUpgrade == null)
            return false;

        return _upgrader.SelectUpgrade(selectedUpgrade);
    }

    #endregion


    #region Private API

    private void ShowUpgradeSelection(UpgradeSO[] upgrades)
    {
        if (upgrades == null || upgrades.Length <= 0)
            return;

        ClearCurrentUpgrades();
        CreateUpgradeUIs(upgrades);
        ShowSelectionPanel();
    }

    private void ShowSelectionPanel()
    {
        if (_selectionPanel != null)
            _selectionPanel.SetActive(true);
    }

    private void HideSelectionPanel(UpgradeSO _)
    {
        ClearCurrentUpgrades();

        if (_selectionPanel != null)
            _selectionPanel.SetActive(false);
    }

    private void CreateUpgradeUIs(UpgradeSO[] upgrades)
    {
        if (_upgradePrefab == null)
        {
            Debug.LogError("Upgrade prefab is not assigned!", this);
            return;
        }

        if (upgrades.Length <= 0)
        {
            Debug.LogError("Invalid number of upgrade UI!", this);
            return;
        }

        for (int i = 0; i < upgrades.Length; i++)
        {
            var upgradeData = upgrades[i];
            if (upgradeData == null)
                continue;

            UI_UpgradeComponent upgradeUI = Instantiate(_upgradePrefab, _upgradesContainer);

            //upgradeUI.enabled = true;
            upgradeUI.Initialize(this, upgradeData);
            //upgradeUI.SetUpgradeData(upgradeData);

            _currentUpgradeUIs.Add(upgradeUI);
        }
    }

    private void ClearCurrentUpgrades()
    {
        foreach (var upgradeUI in _currentUpgradeUIs)
        {
            if (upgradeUI != null && upgradeUI.gameObject != null)
            {
                DestroyImmediate(upgradeUI.gameObject);
            }
        }
        _currentUpgradeUIs.Clear();
    }

    #endregion

}
