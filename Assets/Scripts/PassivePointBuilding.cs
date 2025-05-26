// PassivePointBuilding.cs
using UnityEngine;
using System.Collections;

public class PassivePointBuilding : BuildIngWithFloatingText
{
    [Header("Passive Building Settings")]
    [SerializeField] private int pointsPerInterval = 1;
    [SerializeField] private float intervalTime = 1f;

    private int currentPointsPerInterval;
    private int bonusMultiplier = 1;
    private bool isBonusArea = false;

    public int PointsPerInterval => pointsPerInterval;
    public int CurrentPointsPerInterval => currentPointsPerInterval;

    public void SetPointsPerInterval(int newPoints)
    {
        pointsPerInterval = newPoints;
        ApplyBonusMultiplier(bonusMultiplier);
    }

    public void ApplyBonusMultiplier(int multiplier)
    {
        bonusMultiplier = multiplier;
        currentPointsPerInterval = pointsPerInterval * bonusMultiplier;
        isBonusArea = (multiplier > 1);

        Debug.Log($"Passive Building '{gameObject.name}' adjusted. Base: {pointsPerInterval}, Multiplier: {bonusMultiplier}, Current: {currentPointsPerInterval}");

        // *** ลบการเรียก PointManager.instance.RecalculateTotalPassivePoints() ออกจากที่นี่แล้ว ***
        // การคำนวณรวมจะเกิดขึ้นเมื่อ Building ถูก Register/Unregister เท่านั้น
    }

    public override void StartBuilding()
    {
        base.StartBuilding();
        // ตรวจสอบ PointManager.instance ก่อนเรียกใช้
        if (PointManager.instance != null)
        {
            PointManager.instance.RegisterPassivePointBuilding(this); // (1) ลงทะเบียน Building กับ PointManager ก่อน
            PointManager.instance.RecalculateTotalPassivePoints();  // (2) แล้วค่อยสั่งให้ PointManager คำนวณยอดรวมใหม่
            Debug.Log($"[PassiveBuilding {gameObject.name}.StartBuilding] Registered and triggered recalculation.");
        }
    }

    public override void StopBuilding()
    {
        base.StopBuilding();
        if (PointManager.instance != null)
        {
            PointManager.instance.UnregisterPassivePointBuilding(this); // (1) Unregister Building ก่อน
            PointManager.instance.RecalculateTotalPassivePoints(); // (2) แล้วค่อยสั่งให้ PointManager คำนวณยอดรวมใหม่
            Debug.Log($"[PassiveBuilding {gameObject.name}.StopBuilding] Unregistered and triggered recalculation.");
        }
    }

    public void ShowPointsFloatingText()
    {
        if (pointsText != null)
        {
            if (currentFloatRoutine != null)
            {
                StopCoroutine(currentFloatRoutine);
            }

            string textToShow = $"+ {currentPointsPerInterval}";
            if (isBonusArea)
            {
                textToShow += $" (x{bonusMultiplier})";
            }
            Debug.Log($"Floating Text for '{gameObject.name}': {textToShow}");
            currentFloatRoutine = StartCoroutine(FloatAndFadeText(textToShow));
        }
    }
}