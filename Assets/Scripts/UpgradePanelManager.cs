using UnityEngine;

public class UpgradePanelManager : MonoBehaviour
{
    public GameObject upgradePanel;
    public GameObject upgradeButton;
    public static UpgradePanelManager instance; 
    void Start()
    {
        instance = this;
        upgradePanel.SetActive(false);
        upgradeButton.SetActive(true);
    }

    public void OpenUpgradePanel()
    {
        upgradePanel.SetActive(true);
        upgradeButton.SetActive(false);
    }

    public void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        upgradeButton.SetActive(true);
    }
}
