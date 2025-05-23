using TMPro;
using UnityEngine;
using System.Collections;

public abstract class BuildIngWithFloatingText : BaseBuilding
{
    [Header("Floating Text Animation")]
    [SerializeField] protected TMP_Text pointsText;
    [SerializeField] protected float floatSpeed = 1f;
    [SerializeField] protected float fadeDuration = 1f;
    [SerializeField] protected Vector3 floatOffset = new Vector3(0, 1f, 0);

    protected Vector3 initialTextLocalPosition;
    protected Coroutine currentFloatRoutine;
    protected Transform pointsTextTransform;
    protected Color initialTextColor;

    protected void Awake()
    {
        if (pointsText != null)
        {
            pointsTextTransform = pointsText.transform;
            initialTextLocalPosition = pointsTextTransform.localPosition;

            initialTextColor = pointsText.color;
            initialTextColor.a = 1f;
            pointsText.color = initialTextColor;
            
            pointsText.gameObject.SetActive(false);
        }
    }
    
    protected IEnumerator FloatAndFadeText(int pointsToShow)
    {
        if (pointsText != null)
        {
            pointsText.color = initialTextColor;
            pointsText.gameObject.SetActive(true);
            pointsText.text = $"+ {pointsToShow}";
        }

        float currentFloatTime = 0f;
        float floatDurationCalculated = floatOffset.magnitude / floatSpeed;

        Vector3 startLocalPosition = initialTextLocalPosition;
        Vector3 endLocalPosition = initialTextLocalPosition + floatOffset;

        while (currentFloatTime < floatDurationCalculated)
        {
            float t = currentFloatTime / floatDurationCalculated;
            pointsTextTransform.localPosition = Vector3.Lerp(startLocalPosition, endLocalPosition, t);
            currentFloatTime += Time.deltaTime;
            yield return null;
        }
        pointsTextTransform.localPosition = endLocalPosition;

        float currentFadeTime = 0f;
        Color startColor = pointsText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (currentFadeTime < fadeDuration)
        {
            float t = currentFadeTime / fadeDuration;
            pointsText.color = Color.Lerp(startColor, endColor, t);
            currentFadeTime += Time.deltaTime;
            yield return null;
        }
        pointsText.color = endColor;
        
        pointsTextTransform.localPosition = initialTextLocalPosition;
        pointsText.color = initialTextColor;

        currentFloatRoutine = null;

        if (pointsText != null)
        {
            pointsText.gameObject.SetActive(false);
        }
    }
    
    public override void StopBuilding() 
    {
        base.StopBuilding(); 
        if (currentFloatRoutine != null)
        {
            StopCoroutine(currentFloatRoutine);
            currentFloatRoutine = null;
        }
        if (pointsText != null)
        {
            pointsTextTransform.localPosition = initialTextLocalPosition;
            pointsText.color = initialTextColor;
            pointsText.gameObject.SetActive(false);
        }
    }
}