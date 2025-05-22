using UnityEngine;

public class BonusClickBuilding : BaseBuilding 
{
    [Header("Bonus Click Settings")]
    [SerializeField] private int bonusPointsPerBuilding = 1;
    public int BonusPointsPerBuilding => bonusPointsPerBuilding;
    

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
    public override void StartBuilding()
    {
        base.StartBuilding();
        PointManager.instance.RegisterBonusClickBuilding(bonusPointsPerBuilding);
            
    }
    public override void StopBuilding()
    {
        base.StopBuilding(); 
        PointManager.instance.UnregisterBonusClickBuilding(bonusPointsPerBuilding);
    }
}