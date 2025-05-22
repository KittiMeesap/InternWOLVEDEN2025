  using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeItem : MonoBehaviour
{

    [Header("UI References")]
    public Button buyButton;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI effectText;
    public Image ImagePerfab;
    [Header("Building to Purchase")]
    public GameObject buildingPrefabToBuy;

    private BaseBuilding buildingInfoFromPrefab;

    private void Awake() 
    {
            buildingInfoFromPrefab = buildingPrefabToBuy.GetComponent<BaseBuilding>();
    }

    private void Start() 
    {
        UpdateUI();
        buyButton.onClick.AddListener(AttemptPurchase);
    }

    private void Update()
    {
        if (buildingInfoFromPrefab == null)
        {
            buyButton.interactable = false;
            return;
        }
        
        if (PointManager.instance != null && BuildManager.instance != null)
        {
            bool hasEnoughPoints = PointManager.instance.points >= buildingInfoFromPrefab.buildingCost;
            bool canInstantiate = BuildManager.instance.CanInstantiateBuilding();
            buyButton.interactable = hasEnoughPoints && canInstantiate;
        }
        else
        {
            buyButton.interactable = false;
        }
    }

    private void AttemptPurchase()
    {
        if (buildingInfoFromPrefab == null)
        {
            return;
        }
        if (BuildManager.instance == null)
        {
             return;
        }
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (PointManager.instance != null && PointManager.instance.points >= buildingInfoFromPrefab.buildingCost)
            {
                if (BuildManager.instance.CanInstantiateBuilding())
                {
                    PointManager.instance.AddPoints(-buildingInfoFromPrefab.buildingCost);
                    if (buildingPrefabToBuy != null)
                    {
                        BuildManager.instance.InstantiateBuilding(buildingPrefabToBuy, buildingInfoFromPrefab.buildingCost);
                        UpgradePanelManager.instance.CloseUpgradePanel();
                        UpdateUI();
                    }
                }
            }
        }
       
    }

    private void UpdateUI()
    {
        SpriteRenderer spriteRenderer = buildingPrefabToBuy.GetComponent<SpriteRenderer>();
        nameText.text = buildingInfoFromPrefab.buildingName;
        costText.text = $"Cost: {buildingInfoFromPrefab.buildingCost} Points";
        buttonText.text = $"Buy";
        ImagePerfab.sprite = spriteRenderer.sprite;
        switch (buildingInfoFromPrefab) 
        {
            case PassivePointBuilding passive: 
                effectText.text = $"{passive.PointsPerInterval} Pts/sec";
                break;
            case BonusClickBuilding bonus: 
                effectText.text = $"{bonus.BonusPointsPerBuilding} Pts/Click";
                break;
            default: 
                break;
        }
        
    }
}