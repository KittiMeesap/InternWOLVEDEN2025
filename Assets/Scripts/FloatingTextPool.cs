using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic; 

public class FloatingTextPool : MonoBehaviour
{
    [Header("Floating Text Prefab & Pool Settings")]
    public GameObject floatingTextPrefab; 
    [SerializeField] private int initialPoolSize = 10; 

    [Header("Floating Text Animation Settings")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Vector3 floatOffset = new Vector3(0, 1f, 0);
    private Queue<GameObject> pool = new Queue<GameObject>();
    private Transform poolParent; 
    
    public static FloatingTextPool instance;

    private void Awake()
    {
        instance = this;
        poolParent = new GameObject("FloatingTextPoolParent").transform;
        poolParent.SetParent(this.transform); 
        InitializePool();
    }
    
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject textInstance = Instantiate(floatingTextPrefab, poolParent); 
            textInstance.SetActive(false); 
            pool.Enqueue(textInstance); 
        }
    }
    
    private GameObject GetInstance()
    {
        if (pool.Count > 0)
        {
            GameObject textInstance = pool.Dequeue();
            textInstance.SetActive(true); 
            return textInstance;
        }
        else
        {
            GameObject newInstance = Instantiate(floatingTextPrefab, poolParent);
            newInstance.SetActive(true);
            return newInstance;
        }
    }
    
    private void ReturnInstance(GameObject textInstance)
    {
        textInstance.SetActive(false); 
        textInstance.transform.localPosition = Vector3.zero; 
        textInstance.transform.localRotation = Quaternion.identity; 
        pool.Enqueue(textInstance); 
    }
    
    public void ShowFloatingText(Vector3 worldPosition, string textValue)
    {
        GameObject floatingTextGO = GetInstance();
        floatingTextGO.transform.position = worldPosition;
        TMP_Text textMesh = floatingTextGO.GetComponentInChildren<TMP_Text>();
        textMesh.text = textValue;
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FloatAndFade(floatingTextGO, textMesh, textMesh.color, textMesh.color.a, floatSpeed, fadeDuration, floatOffset));
        }
        else
        {
            ReturnInstance(floatingTextGO);
        }
    }
    
    private IEnumerator FloatAndFade(GameObject textGO, TMP_Text textMesh, Color initialColor, float initialAlpha, float speed, float duration, Vector3 offset)
    {
        Color startColor = initialColor;
        startColor.a = initialAlpha; 
        if (startColor.a == 0) 
        {
            startColor.a = 1f; 
        }
        textMesh.color = startColor; 
        Vector3 startPosition = textGO.transform.position; 
        Vector3 endPosition = startPosition + offset; 
        float currentFloatTime = 0f;
        float floatDurationCalculated = offset.magnitude / speed; 
        // เฟสที่ 1: Text ลอยขึ้น
        while (currentFloatTime < floatDurationCalculated)
        {
            float t = currentFloatTime / floatDurationCalculated;
            textGO.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            currentFloatTime += Time.deltaTime;
            yield return null;
        }
        textGO.transform.position = endPosition; 
        // เฟสที่ 2: Text จางหายไป (Opacity ลดลง)
        float currentFadeTime = 0f;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); 
        while (currentFadeTime < duration)
        {
            float t = currentFadeTime / duration;
            textMesh.color = Color.Lerp(startColor, endColor, t);
            currentFadeTime += Time.deltaTime;
            yield return null;
        }
        textMesh.color = endColor; 
        ReturnInstance(textGO);
    }
}