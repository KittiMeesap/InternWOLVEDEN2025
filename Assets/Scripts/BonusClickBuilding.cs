using UnityEngine;

public class BonusClickBuilding : BaseBuilding 
{
    [Header("Bonus Click Settings")]
    [SerializeField] private int bonusPointsPerBuilding = 1;
    public int BonusPointsPerBuilding => bonusPointsPerBuilding;
    

   
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