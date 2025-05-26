using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("UI References")]
    public GameObject tutorialClickUI;
    public GameObject topUI;
    public GameObject shopButtonUI;
    public GameObject shopButtonGuideUI;
    public GameObject shopUI;
    public GameObject shopBuyGuideUI;
    public GameObject placeGuideUI;
    public GameObject passivePointUI;
    public GameObject extendedBuyGuideUI;
    public GameObject extendedPlaceGuideUI;

    private bool clickedIsland = false;
    private bool shopButtonShown = false;
    private bool shopBuyGuideShown = false;
    private bool placeGuideShown = false;
    private bool passiveUIShown = false;
    private bool tutorialFinished = false;

    private bool extendedBuyGuideShown = false;
    private bool extendedPlaceGuideShown = false;

    private bool hasPlacedStructure = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        topUI.SetActive(false);
        shopButtonUI.SetActive(false);
        shopButtonGuideUI.SetActive(false);
        shopUI.SetActive(false);
        shopBuyGuideUI.SetActive(false);
        placeGuideUI.SetActive(false);
        passivePointUI.SetActive(false);
        tutorialClickUI.SetActive(true);

        extendedBuyGuideUI?.SetActive(false);
        extendedPlaceGuideUI?.SetActive(false);
    }

    public void OnIslandClicked()
    {
        if (!clickedIsland)
        {
            clickedIsland = true;
            tutorialClickUI.SetActive(false);
            topUI.SetActive(true);
        }
    }

    public bool IsTutorialStepAllowingShop()
    {
        return shopButtonShown;
    }

    public bool IsWaitingForPlacementGuide()
    {
        return shopBuyGuideShown && !placeGuideShown;
    }

    public void NotifyPurchase()
    {
        if (!placeGuideShown)
        {
            shopBuyGuideUI.SetActive(false);
            placeGuideUI.SetActive(true);
            placeGuideShown = true;
        }
        else if (placeGuideShown && extendedBuyGuideShown && !extendedPlaceGuideShown)
        {
            extendedBuyGuideUI.SetActive(false);
            extendedPlaceGuideUI.SetActive(true);
            extendedPlaceGuideShown = true;
        }
    }

    public bool HasPlacedStructure()
    {
        return hasPlacedStructure;
    }

    private void Update()
    {
        if (clickedIsland && !shopButtonShown && PointManager.instance.points >= 10)
        {
            shopButtonShown = true;
            shopButtonUI.SetActive(true);
            shopButtonGuideUI.SetActive(true);
            UpgradePanelManager.instance?.upgradeButton.SetActive(true);
        }

        if (shopButtonShown && !shopBuyGuideShown &&
            UpgradePanelManager.instance != null &&
            UpgradePanelManager.instance.upgradePanel.activeSelf)
        {
            shopBuyGuideShown = true;
            shopButtonGuideUI.SetActive(false);
            shopBuyGuideUI.SetActive(true);
        }

        if (placeGuideShown && !extendedBuyGuideShown &&
            BuildManager.instance != null &&
            BuildManager.instance.HasPlacedNewTile)
        {
            placeGuideUI.SetActive(false);
            extendedBuyGuideUI?.SetActive(true);
            extendedBuyGuideShown = true;
        }

        if (extendedPlaceGuideShown && !hasPlacedStructure &&
            BuildManager.instance != null && BuildManager.instance.HasPlacedNewTile)
        {
            hasPlacedStructure = true;
            extendedPlaceGuideUI?.SetActive(false);
        }

        if (extendedPlaceGuideShown && hasPlacedStructure && !passiveUIShown)
        {
            passiveUIShown = true;
            passivePointUI?.SetActive(true);

            if (!tutorialFinished)
            {
                tutorialFinished = true;
                Debug.Log("Tutorial Complete");
            }
        }
    }
}
