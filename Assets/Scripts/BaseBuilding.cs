using UnityEngine;

public abstract class BaseBuilding : MonoBehaviour
{
    [Header("Building Info")]
    [SerializeField] public string buildingName = "New Building";
    [SerializeField] public int buildingCost = 10;

    protected bool isBuildingActive = false;

    public virtual void StartBuilding()
    {
        if (!isBuildingActive)
        {
            isBuildingActive = true;
            Debug.Log(buildingName + " activated"); // ควรจะเห็น Log นี้ตอนวางสำเร็จ
            if (SoundManager.instance != null && SoundManager.instance.audioClickBuildings != null)
            {
                SoundManager.instance.PlaySound(SoundManager.instance.audioClickBuildings);
            } else {
                Debug.LogWarning("SoundManager or audioClickBuildings not found for " + buildingName + " on StartBuilding.");
            }
        }
    }

    public virtual void StopBuilding()
    {
        Debug.Log($"Attempting to Stop Building: {buildingName}. Current isBuildingActive: {isBuildingActive}"); // เพิ่ม Log ตรวจสอบ
            isBuildingActive = false;
            Debug.Log(buildingName + " deactivated (Stopping Building)."); // Log เมื่อหยุดสำเร็จ
            // --- เพิ่มส่วนนี้สำหรับเสียงตอน Stop/Destroy ---
            if (SoundManager.instance != null && SoundManager.instance.audioDestroyBuildings != null) // สมมติว่ามี audioDestroyBuilding
            {
                SoundManager.instance.PlaySound(SoundManager.instance.audioDestroyBuildings);
            } else {
                Debug.LogWarning("SoundManager or audioDestroyBuilding not found for " + buildingName + " on StopBuilding.");
            }
            // ---------------------------------------------
    }
}