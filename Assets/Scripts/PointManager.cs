// PointManager.cs
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PointManager : MonoBehaviour
{
    public static PointManager instance { get; private set; }

    public int points = 0;
    public TextMeshProUGUI pointsText;

    [Header("Click Settings")]
    public int basePointsPerClick = 1;
    public int currentPointsPerClickOnTile = 1;

    private List<PassivePointBuilding> activePassiveBuildings = new List<PassivePointBuilding>();
    private int totalPassivePointsPerSecond = 0;

    private int totalBonusPointsPerClick = 0;

    void Awake()
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
        StartCoroutine(GeneratePassivePointsRoutine());
        // เรียก Recalculate ครั้งแรกเมื่อ PointManager เริ่มทำงาน
        // ณ จุดนี้ activePassiveBuildings อาจยังว่าง ถ้าไม่มี Building ที่ Instantiate/Awake/Start ก่อน
        RecalculateTotalPassivePoints();
        Debug.Log($"[PointManager.Start] Initial Recalculation. Total Passive Points: {totalPassivePointsPerSecond}");
    }

    public void AddPoints(int amount)
    {
        points += amount;
        UpdatePointsText();
    }

    public void UpdatePointsText()
    {
        if (pointsText != null)
        {
            pointsText.text = "Points: " + points.ToString();
        }
    }

    public void AddPointsForTileClick()
    {
        AddPoints(currentPointsPerClickOnTile);
    }

    // === Passive Point Building Management ===
    public void RegisterPassivePointBuilding(PassivePointBuilding building)
    {
        if (!activePassiveBuildings.Contains(building))
        {
            activePassiveBuildings.Add(building);
            Debug.Log($"[PointManager] Registered Passive Building: {building.name}. Active count: {activePassiveBuildings.Count}");
            // ลบการเรียก RecalculateTotalPassivePoints() ออกจากที่นี่
            // เพราะ PassivePointBuilding.StartBuilding() จะเป็นผู้เรียกหลังจาก Register แล้ว
        }
        else
        {
            Debug.LogWarning($"[PointManager] Attempted to register {building.name} again, but it's already in the list.");
        }
    }

    public void UnregisterPassivePointBuilding(PassivePointBuilding building)
    {
        if (activePassiveBuildings.Contains(building))
        {
            activePassiveBuildings.Remove(building);
            Debug.Log($"[PointManager] Unregistered Passive Building: {building.name}. Active count: {activePassiveBuildings.Count}");
            // RecalculateTotalPassivePoints() จะถูกเรียกจาก StopBuilding ใน PassivePointBuilding แล้ว
        }
    }

    public void RecalculateTotalPassivePoints()
    {
        totalPassivePointsPerSecond = 0;
        foreach (var building in activePassiveBuildings)
        {
            // ตรวจสอบว่า building ไม่ได้เป็น null หรือถูกทำลายไปแล้ว (edge case)
            if (building != null)
            {
                totalPassivePointsPerSecond += building.CurrentPointsPerInterval;
                Debug.Log($"  - Building: {building.name}, CurrentPoints: {building.CurrentPointsPerInterval}");
            }
        }
        Debug.Log($"[PointManager] Recalculated Total Passive Points: {totalPassivePointsPerSecond}");
    }

    private System.Collections.IEnumerator GeneratePassivePointsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (totalPassivePointsPerSecond > 0)
            {
                AddPoints(totalPassivePointsPerSecond);
                Debug.Log($"[PointManager] Generated {totalPassivePointsPerSecond} passive points from PointManager.");

                foreach (var building in activePassiveBuildings)
                {
                    if (building != null) // เช็ค null อีกครั้งเผื่อ Building ถูกทำลายระหว่าง Loop
                    {
                        building.ShowPointsFloatingText();
                    }
                }
            }
        }
    }

    // === Bonus Click Building Management ===
    public void RegisterBonusClickBuilding(int bonusPoints)
    {
        totalBonusPointsPerClick += bonusPoints;
        currentPointsPerClickOnTile = basePointsPerClick + totalBonusPointsPerClick;
        Debug.Log($"Registered Bonus Click. Current Click Points: {currentPointsPerClickOnTile}");
    }

    public void UnregisterBonusClickBuilding(int bonusPoints)
    {
        totalBonusPointsPerClick -= bonusPoints;
        currentPointsPerClickOnTile = basePointsPerClick + totalBonusPointsPerClick;
        Debug.Log($"Unregistered Bonus Click. Current Click Points: {currentPointsPerClickOnTile}");
    }
}