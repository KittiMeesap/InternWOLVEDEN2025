using UnityEngine;


// เปลี่ยนไปสืบทอดจาก BaseBuilding
public class BonusClickBuilding : BaseBuilding 
{
    [Header("Bonus Click Settings")]
    [SerializeField] private int bonusPointsPerBuilding = 1;

    protected override void Start()
    {
        base.Start(); // เรียก Start ของ BaseBuilding

        if (PointManager.instance != null)
        {
            PointManager.instance.RegisterBonusClickBuilding(bonusPointsPerBuilding);
        }
       
    }

    private void OnDestroy()
    {
        if (PointManager.instance != null)
        {
            PointManager.instance.UnregisterBonusClickBuilding(bonusPointsPerBuilding);
        }
    }

    public void SetBonusPoints(int newBonus)
    {
        if (PointManager.instance != null)
        {
            PointManager.instance.UnregisterBonusClickBuilding(bonusPointsPerBuilding);
        }
        bonusPointsPerBuilding = newBonus;
        if (PointManager.instance != null)
        {
            PointManager.instance.RegisterBonusClickBuilding(bonusPointsPerBuilding);
        }
     
    }
}