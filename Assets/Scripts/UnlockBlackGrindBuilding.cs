using UnityEngine;

public class UnlockBlackGrindBuilding : BaseBuilding
{
   
    public override void StartBuilding() 
    {
        base.StartBuilding();
        
        Vector3Int cellToUnlock = BuildManager.instance.groundTilemap.WorldToCell(transform.position);
        if (BuildManager.instance != null)
        {
            // *** เปลี่ยนมาเรียกเมธอดใหม่ใน BuildManager ***
            BuildManager.instance.ProcessUnlockBuilding(cellToUnlock, gameObject); // ส่ง GameObject ตัวเองไปด้วย
        }
        // *** ลบบรรทัด Destroy(gameObject); ออกจากที่นี่ ***
        // เพราะ BuildManager จะเป็นผู้ทำลายมันเอง
    }

    public override void StopBuilding()
    {
        base.StopBuilding();
        // ถ้า StopBuilding ถูกเรียก (เช่นเมื่อยกเลิกการวาง) ก็ควรทำลายตัวเองด้วย
        // เพื่อป้องกันการค้างกรณีที่ StartBuilding ยังไม่ถูกเรียก
        if (gameObject != null)
        {
            Destroy(gameObject, 0.01f); // ยังคงให้ Destroy หากถูก StopBuilding โดยตรง
        }
    }
}