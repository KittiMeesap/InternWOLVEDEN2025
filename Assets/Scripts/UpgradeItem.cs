// ใน UpgradeItem.cs
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
    public Image ImagePerfab;
    public TextMeshProUGUI effectText;

    [Header("Building to Purchase")]
    public GameObject buildingPrefabToBuy;

    private BaseBuilding buildingInfoFromPrefab;

    private void Awake()
    {
        if (buildingPrefabToBuy != null)
        {
            buildingInfoFromPrefab = buildingPrefabToBuy.GetComponent<BaseBuilding>();
        }
    }

    private void Start()
    {
        if (BuildManager.instance == null)
        {
            buyButton.interactable = false;
            return;
        }
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
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (PointManager.instance != null && PointManager.instance.points >= buildingInfoFromPrefab.buildingCost)
            {
            
                    if (buildingPrefabToBuy != null)
                    {
                        BuildManager.instance.InstantiateBuilding(buildingPrefabToBuy, buildingInfoFromPrefab.buildingCost);
                        UpdateUI();
                    }
                
            }
        }
    }

    private void UpdateUI()
    {
        if (buildingInfoFromPrefab != null)
        {
            SpriteRenderer spriteRenderer = buildingPrefabToBuy.GetComponent<SpriteRenderer>();
            nameText.text = buildingInfoFromPrefab.buildingName;
            costText.text = $"Cost: {buildingInfoFromPrefab.buildingCost} Points";
            buttonText.text = $"Buy {buildingInfoFromPrefab.buildingName}";

            if (ImagePerfab != null)
            {
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    ImagePerfab.sprite = spriteRenderer.sprite;
                    ImagePerfab.color = new Color(ImagePerfab.color.r, ImagePerfab.color.g, ImagePerfab.color.b, 1f);
                }
                else
                {
                    ImagePerfab.sprite = null;
                    ImagePerfab.color = new Color(ImagePerfab.color.r, ImagePerfab.color.g, ImagePerfab.color.b, 0f);
                }
            }

            if (effectText != null)
            {
                switch (buildingInfoFromPrefab)
                {
                    case PassivePointBuilding passive:
                        effectText.text = $"Produces: {passive.PointsPerInterval} Pts/sec";
                        break;
                    case BonusClickBuilding bonus:
                        effectText.text = $"Click Bonus: +{bonus.BonusPointsPerBuilding} Pts";
                        break;
                    case UnlockBlackGrindBuilding unlocker:
                        effectText.text = $"Unlocks area "; 
                        break;
                    case RemoverBuilding remover: // เพิ่ม case สำหรับ RemoverBuilding
                        effectText.text = $"Removes building"; //
                        break;
                    default:
                        effectText.text = "No Special Effect";
                        break;
                }
            }
        }
    }
}