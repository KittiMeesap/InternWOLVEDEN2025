using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeItem : MonoBehaviour
{
    [Header("Purchase Settings")]
    public int purchaseCost = 10;

    [Header("UI References")]
    public Button buyButton;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI costText;

    [Header("Building to Purchase")]
    public GameObject buildingPrefabToBuy;

    private void Start()
    {
        if (BuildManager.Instance == null)
        {
            Debug.LogError("BuildManager.Instance not found in the scene! Ensure BuildManager is present and its Awake() has run.");
            return;
        }

        UpdateUI();
        buyButton.onClick.AddListener(AttemptPurchase);
    }

    private void Update()
    {
        if (PointManager.instance != null && BuildManager.Instance != null)
        {
            bool hasEnoughPoints = PointManager.instance.points >= purchaseCost;
            bool canInstantiate = BuildManager.Instance.CanInstantiateBuilding();
            buyButton.interactable = hasEnoughPoints && canInstantiate;
        }
        else
        {
            buyButton.interactable = false;
        }
    }

    private void AttemptPurchase()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (PointManager.instance != null && PointManager.instance.points >= purchaseCost)
            {
                if (BuildManager.Instance != null && BuildManager.Instance.CanInstantiateBuilding())
                {
                    PointManager.instance.AddPoints(-purchaseCost);
                    if (buildingPrefabToBuy != null)
                    {
                        BuildManager.Instance.InstantiateBuilding(buildingPrefabToBuy, purchaseCost);
                        UpdateUI();
                    }
                    else
                    {
                        Debug.LogError("Building Prefab To Buy is not assigned in UpgradeItem!");
                    }
                }
                else
                {
                    Debug.LogWarning("BuildManager is not ready or busy. Cannot purchase a new building yet.");
                }
            }
            else
            {
                Debug.Log("Not enough points to purchase!");
            }
        }
        else
        {
            Debug.LogWarning("Attempted purchase click not over UI. Blocking.");
        }
    }

    private void UpdateUI()
    {
        buttonText.text = "Buy Building";
        costText.text = $"Cost: {purchaseCost} Points";
    }
}