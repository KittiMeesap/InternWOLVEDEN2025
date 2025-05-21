using UnityEngine;
// ไม่ต้องใช้ TMPro ที่นี่แล้ว

// เปลี่ยนชื่อคลาสจาก BuildIng เป็น BaseBuilding
public abstract class BaseBuilding : MonoBehaviour
{
    // ลบ [Header("Floating Text Animation")] และตัวแปรทั้งหมดที่เกี่ยวข้องกับ Floating Text ออกไป
    // (pointsText, floatSpeed, fadeDuration, floatOffset, initialTextLocalPosition,
    // currentFloatRoutine, pointsTextTransform, initialTextColor)

    protected bool isBuildingActive = false;

    // ลบ Constructor หากมี (เช่น Awake, Start) ที่เกี่ยวข้องกับ Floating Text
    protected virtual void Awake()
    {
        // ไม่มี logic เกี่ยวกับ Floating Text ที่นี่แล้ว
    }

    protected virtual void Start()
    {
        // ไม่มี logic เกี่ยวกับ Floating Text ที่นี่แล้ว
    }

    protected virtual void Update()
    {
        // คลาสแม่นี้ไม่มี Update loop ที่ทำงานเสมอไป
    }

    public virtual void StartBuilding()
    {
        if (!isBuildingActive)
        {
            isBuildingActive = true;
        }
    }

    public virtual void StopBuilding()
    {
        if (isBuildingActive)
        {
            isBuildingActive = false;
            // ไม่ต้องหยุด Coroutine หรือซ่อน Text แล้ว
        }
    }

    // ลบ IEnumerator FloatAndFadeText() ออกไปทั้งหมด
}