using UnityEngine;

public class Building : MonoBehaviour
{
    
    [SerializeField]private float timeInterval = 1f;
    [SerializeField] private int pointsPerSec = 1;
    private float timer; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = timeInterval; 
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        // ถ้า timer น้อยกว่าหรือเท่ากับ 0 หมายความว่าครบ 1 วินาทีแล้ว
        if (timer <= 0)
        {
            PointsManager.instance.points += pointsPerSec; // เพิ่มคะแนน
            PointsManager.instance.UpdatePointsText();
            timer = timeInterval; // รีเซ็ต timer กลับไปที่ timeInterval
        }
    }
}
