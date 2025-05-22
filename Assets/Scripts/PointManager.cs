using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PointManager : MonoBehaviour
{
    public static PointManager instance { get; private set; }

    public int points = 0;

    [Header("Tile Click Settings")]
    [SerializeField] private int basePointsPerClickOnTile = 1;
    private int totalBonusPointsFromClickBuildings = 0;
    public int currentPointsPerClickOnTile => basePointsPerClickOnTile + totalBonusPointsFromClickBuildings;

    [Header("Passive Income Settings")]
    private int totalPassivePointsPerSecond = 0;
    private float passiveIncomeTimer = 0f;
    private const float PASSIVE_INCOME_INTERVAL = 1f;

    private List<PassivePointBuilding> activePassiveBuildings = new List<PassivePointBuilding>();

    [Header("UI References")]
    public TMP_Text pointsText;
    public TMP_Text pointPerSecText;
    public TMP_Text pointPerClickText;
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
        UpdatePointsText();
    }

    void Update()
    {
        passiveIncomeTimer -= Time.deltaTime;
        if (passiveIncomeTimer <= 0f)
        {
            if (totalPassivePointsPerSecond > 0)
            {
                AddPoints(totalPassivePointsPerSecond);
                Debug.Log($"Added {totalPassivePointsPerSecond} passive points. Total points: {points}");
                foreach (PassivePointBuilding building in activePassiveBuildings)
                {
                    if (building != null)
                    {
                        building.ShowFloatingText();
                    }
                }
            }
            passiveIncomeTimer = PASSIVE_INCOME_INTERVAL;
        }
    }

    public void AddPoints(int amount)
    {
        points += amount;
        UpdatePointsText();
    }

    public void AddPointsForTileClick()
    {
        AddPoints(currentPointsPerClickOnTile);
        
    }

    public void RegisterBonusClickBuilding(int bonusAmount)
    {
        totalBonusPointsFromClickBuildings += bonusAmount;
        UpdatePointsPerClickText(totalBonusPointsFromClickBuildings);
    }

    public void UnregisterBonusClickBuilding(int bonusAmount)
    {
        totalBonusPointsFromClickBuildings -= bonusAmount;
        if (totalBonusPointsFromClickBuildings < 0)
        {
            totalBonusPointsFromClickBuildings = 0;
        }
    }

    public void RegisterPassiveBuilding(int pointsPerSecondFromThisBuilding, PassivePointBuilding building)
    {
        totalPassivePointsPerSecond += pointsPerSecondFromThisBuilding;
        UpdatePointsPerSecondText(totalPassivePointsPerSecond);
        activePassiveBuildings.Add(building);
        
    }

    public void UnregisterPassiveBuilding(int pointsPerSecondFromThisBuilding, PassivePointBuilding building)
    {
        totalPassivePointsPerSecond -= pointsPerSecondFromThisBuilding;
        if (totalPassivePointsPerSecond < 0)
        {
            totalPassivePointsPerSecond = 0;
        }
        activePassiveBuildings.Remove(building);
    }

    public void UpdatePointsText()
    {
        if (pointsText != null)
        {
            pointsText.text = "Points: " + points.ToString();
        }
    }

    public void UpdatePointsPerSecondText(int pointsPerSecond)
    {
        pointPerSecText.text = pointsPerSecond.ToString()+ "Pts/Sec";
    }

    public void UpdatePointsPerClickText(int pointsPerClick)
    {
        pointPerClickText.text = pointsPerClick.ToString()+ "Pts/Click";
    }
}