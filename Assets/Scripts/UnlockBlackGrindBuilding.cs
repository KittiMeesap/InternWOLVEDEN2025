using UnityEngine;

public class UnlockBlackGrindBuilding : BaseBuilding
{
   
    public override void StartBuilding() 
    {
        base.StartBuilding();
        
        Vector3Int cellToUnlock = BuildManager.instance.groundTilemap.WorldToCell(transform.position);
        if (BuildManager.instance != null)
        {
            BuildManager.instance.UnlockTile(cellToUnlock); 
        }
    }

    public override void StopBuilding()
    {
        base.StopBuilding();
    }
}