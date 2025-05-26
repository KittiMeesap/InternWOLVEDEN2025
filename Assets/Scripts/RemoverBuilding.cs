using UnityEngine;

public class RemoverBuilding : BaseBuilding
{
    [Header("Remover Building Settings")]
    [SerializeField] private int refundPercentage = 50; 

    public override void StartBuilding()
    {
        base.StartBuilding();

        if (BuildManager.instance != null)
        {
            Vector3Int cellToDemolish = BuildManager.instance.groundTilemap.WorldToCell(transform.position);
            BuildManager.instance.DemolishBuildingAtCell(cellToDemolish, refundPercentage);
        }
        
        Destroy(gameObject); // ทำให้ GameObject ตัวนี้หายไปทันทีหลังรื้อถอน
    }

    public override void StopBuilding()
    {
        base.StopBuilding();
    }
}