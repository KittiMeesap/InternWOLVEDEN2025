using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItem : MonoBehaviour
{
    [Header("Upgrade Settings")]
    public int baseCost = 10;
    public int basePointsPerSec = 1;
    public float costMultiplier = 1.5f;

    [Header("UI References")]
    public Button buyButton;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI nameZoneText;
    public Image zoneImage;

    [Header("Zone Info")]
    public string zoneName = "NameZone";
    public Sprite zoneSprite;

    private int level = 0;
    private int currentCost;
    private BuildIng buildIng;

    private void Start()
    {
        currentCost = baseCost;

        buildIng = GetComponent<BuildIng>();
        if (buildIng == null)
        {
            buildIng = gameObject.AddComponent<BuildIng>();
            buildIng.pointsPerSec = 0;
        }

        if (zoneImage != null && zoneSprite != null)
        {
            zoneImage.sprite = zoneSprite;
        }

        if (nameZoneText != null)
        {
            nameZoneText.text = zoneName;
        }

        UpdateUI();
        buyButton.onClick.AddListener(BuyOrUpgrade);
    }

    private void Update()
    {
        buyButton.interactable = PointManager.instance.points >= currentCost;
    }

    private void BuyOrUpgrade()
    {
        if (PointManager.instance.points < currentCost)
            return;

        PointManager.instance.AddPoints(-currentCost);
        level++;

        buildIng.pointsPerSec += basePointsPerSec;

        currentCost = Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, level));
        UpdateUI();
    }

    private void UpdateUI()
    {
        nameZoneText.text = $"{zoneName} Lv.{level}";
        pointText.text = $"Point+{buildIng.pointsPerSec}";
        costText.text = $"Point: {currentCost}";
        buttonText.text = level == 0 ? "Buy" : "Level Up";
    }
}
