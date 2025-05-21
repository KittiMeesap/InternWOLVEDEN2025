using TMPro;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    public static PointManager instance { get; private set; }

    public int points = 0;

    [Header("Tile Click Settings")]
    public int basePointsPerClickOnTile = 1;
    private int totalBonusPointsFromClickBuildings = 0;
    public int currentPointsPerClickOnTile => basePointsPerClickOnTile + totalBonusPointsFromClickBuildings;

    public TMP_Text pointsText;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        UpdatePointsText();
    }

    public void AddPoints(int amount)
    {
        points += amount;
        UpdatePointsText();
    }

    public void AddPointsForTileClick()
    {
        points += currentPointsPerClickOnTile;
        UpdatePointsText();
    }

    public void RegisterBonusClickBuilding(int bonusAmount)
    {
        totalBonusPointsFromClickBuildings += bonusAmount;
    }

    public void UnregisterBonusClickBuilding(int bonusAmount)
    {
        totalBonusPointsFromClickBuildings -= bonusAmount;
        if (totalBonusPointsFromClickBuildings < 0) totalBonusPointsFromClickBuildings = 0;
    }

    public void UpdatePointsText()
    {
        if (pointsText != null)
        {
            pointsText.text = "Points: " + points.ToString();
        }
    }
}