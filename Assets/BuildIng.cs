using TMPro; 
using UnityEngine;
using System.Collections; 

public class BuildIng : MonoBehaviour
{
    [SerializeField] private float timeInterval = 1f; 
    public int pointsPerSec = 1; 
    [SerializeField] private TMP_Text pointsText; 

    [Header("Floating Text Animation")]
    [SerializeField] private float floatSpeed = 1f; 
    [SerializeField] private float fadeDuration = 1f; 
    [SerializeField] private Vector3 floatOffset = new Vector3(0, 1f, 0); 

    private float timer; 
    private bool isBuilding = false; 
    private Vector3 initialTextLocalPosition;
    private Coroutine currentFloatRoutine; 

    // Cached references
    private Transform pointsTextTransform; // เก็บ Transform ของ Text
    private Color initialTextColor; // เก็บสีเริ่มต้นของ Text

    // Awake ถูกเรียกก่อน Start เสมอ (แม้ Script จะถูกปิดอยู่)
    void Awake() 
    {
        if (pointsText != null)
        {
            pointsTextTransform = pointsText.transform; // เก็บ reference
            initialTextLocalPosition = pointsTextTransform.localPosition;
            
            initialTextColor = pointsText.color; // เก็บสีเริ่มต้น
            initialTextColor.a = 1f; // ตรวจสอบให้แน่ใจว่า alpha เริ่มต้นเป็น 1
            pointsText.color = initialTextColor;
            
            pointsText.gameObject.SetActive(false); 
        }
    }

    // Start จะถูกเรียกเมื่อ Component ถูกเปิดใช้งาน (enabled)
    void Start()
    {
        // ไม่ต้องทำอะไรตรงนี้มาก เพราะ Awake จัดการการเตรียมการแล้ว
        // และ StartBuilding() จะถูกเรียกเมื่อถึงเวลาที่เหมาะสม
        if (pointsText != null)
        {
            pointsText.text = $"+ {pointsPerSec}"; // อัปเดตข้อความเริ่มต้น
        }
    }
    
    void Update()
    {
        if (isBuilding)
        {
            timer -= Time.deltaTime;
            
            if (timer <= 0f) // ใช้ 0f เพื่อความแม่นยำทาง Floating Point
            {
                if (PointManager.instance != null)
                {
                    PointManager.instance.AddPoints(pointsPerSec);
                }
                
                if (pointsText != null)
                {
                    pointsText.text = $"+ {pointsPerSec}"; // อัปเดตข้อความก่อนเริ่มลอย (ใช้ string interpolation)
                    
                    // หาก Coroutine เก่ากำลังทำงาน ให้หยุด
                    if (currentFloatRoutine != null)
                    {
                        StopCoroutine(currentFloatRoutine);
                    }
                    
                    // รีเซ็ต Text และสีเป็นค่าเริ่มต้นก่อนเริ่ม Animation
                    pointsTextTransform.localPosition = initialTextLocalPosition;
                    pointsText.color = initialTextColor; // ใช้ initialTextColor ที่มี alpha = 1 แล้ว
                    
                    currentFloatRoutine = StartCoroutine(FloatAndFadeText());
                }
                timer = timeInterval;
            }
        }
    }
    
    IEnumerator FloatAndFadeText()
    {
        if (pointsText != null)
        {
            pointsText.gameObject.SetActive(true);
        }
        
        float currentFloatTime = 0f;
        // คำนวณ duration สำหรับแต่ละเฟสล่วงหน้า
        float floatDurationCalculated = floatOffset.magnitude / floatSpeed;

        Vector3 startLocalPosition = pointsTextTransform.localPosition; // ใช้ cached transform
        Vector3 endLocalPosition = startLocalPosition + floatOffset; 

        // เฟสที่ 1: Text ลอยขึ้น
        while (currentFloatTime < floatDurationCalculated)
        {
            float t = currentFloatTime / floatDurationCalculated; // คำนวณ t สำหรับ Lerp
            pointsTextTransform.localPosition = Vector3.Lerp(startLocalPosition, endLocalPosition, t);
            currentFloatTime += Time.deltaTime;
            yield return null;
        }
        pointsTextTransform.localPosition = endLocalPosition; // ตรวจสอบให้แน่ใจว่าอยู่ที่ตำแหน่งปลายทาง

        // เฟสที่ 2: Text จางหายไป (Opacity ลดลง)
        float currentFadeTime = 0f; // รีเซ็ตตัวจับเวลาสำหรับเฟสใหม่
        Color startColor = pointsText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (currentFadeTime < fadeDuration)
        {
            float t = currentFadeTime / fadeDuration; // คำนวณ t สำหรับ Lerp
            pointsText.color = Color.Lerp(startColor, endColor, t);
            currentFadeTime += Time.deltaTime;
            yield return null; 
        }
        pointsText.color = endColor; 
        
        // เฟสที่ 3: รีเซ็ต Text สำหรับรอบถัดไป
        pointsTextTransform.localPosition = initialTextLocalPosition; // รีเซ็ตเป็น Local Position
        pointsText.color = initialTextColor; // ใช้ initialTextColor ที่มี alpha = 1 แล้ว

        currentFloatRoutine = null;

        if (pointsText != null)
        {
            pointsText.gameObject.SetActive(false);
        }
    }
    
    public void StartBuilding()
    {
        if (!isBuilding) // ตรวจสอบเพื่อป้องกันการรีเซ็ตซ้ำซ้อน
        {
            isBuilding = true;
            timer = timeInterval; // เริ่มนับเวลาเมื่อเริ่ม Building
        }
    }

    public void StopBuilding()
    {
        if (isBuilding) // ตรวจสอบเพื่อป้องกันการทำซ้ำ
        {
            isBuilding = false;
            // อาจจะหยุด Coroutine ที่กำลังทำงานอยู่ด้วยถ้ามี
            if (currentFloatRoutine != null)
            {
                StopCoroutine(currentFloatRoutine);
                currentFloatRoutine = null;
            }
            // ซ่อน Text ทันทีหากหยุด building
            if (pointsText != null)
            {
                pointsText.gameObject.SetActive(false);
            }
        }
    }

    public void SetPointsPerSec(int newPoints)
    {
        pointsPerSec = newPoints;
        if (pointsText != null)
        {
            pointsText.text = $"+ {pointsPerSec}"; // ใช้ string interpolation
        }
    }   
}