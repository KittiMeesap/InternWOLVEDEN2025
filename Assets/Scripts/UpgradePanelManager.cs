using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelManager : MonoBehaviour
{
    public GameObject upgradePanel;
    public GameObject upgradeButton;
    public ScrollRect upgradeScrollRect;
    public GameObject produceContent;
    public GameObject clickContent;
    public static UpgradePanelManager instance;

    private Vector2 savedScrollPosition = Vector2.one;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        upgradePanel.SetActive(false);
        
        if (TutorialManager.instance != null && !TutorialManager.instance.IsTutorialStepAllowingShop())
        {
            upgradeButton.SetActive(false);
        }
        else
        {
            upgradeButton.SetActive(true);
        }
    }


    public void OpenUpgradePanel()
    {
        upgradePanel.SetActive(true);
        upgradeButton.SetActive(false);
        if (upgradeScrollRect != null)
        {
            upgradeScrollRect.normalizedPosition = savedScrollPosition;
        }
    }

    public void CloseUpgradePanel()
    {
        if (upgradeScrollRect != null)
        {
            savedScrollPosition = upgradeScrollRect.normalizedPosition;
        }
        upgradePanel.SetActive(false);
        upgradeButton.SetActive(true);
    }

    public void OpenClickContent()
    {
         clickContent.SetActive(true);
         produceContent.SetActive(false);
    }
    public void OpenProduceContent()
    {
        clickContent.SetActive(false);
        produceContent.SetActive(true);
    }
}
