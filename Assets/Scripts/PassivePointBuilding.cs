using UnityEngine;
using TMPro;

public class PassivePointBuilding : BuildIngWithFloatingText
{
    [Header("Passive Point Settings")]
    [SerializeField] private int pointsPerInterval = 1;
    public int PointsPerInterval => pointsPerInterval;
    protected  void Start()
    {
        if (pointsText != null)
        {
            pointsText.text = $"+ {pointsPerInterval}";
        }
        SetPointsPerInterval(pointsPerInterval);
    }

    public void SetPointsPerInterval(int newPoints)
    {
        pointsPerInterval = newPoints;
        if (pointsText != null)
        {
            pointsText.text = $"+ {pointsPerInterval}";
        }
    }

  

    public override void StartBuilding()
    {
        base.StartBuilding();
        if (PointManager.instance != null)
        {
            PointManager.instance.RegisterPassiveBuilding(pointsPerInterval, this);
        }
    }

    public void ShowFloatingText()
    {
        if (pointsText != null)
        {
            if (currentFloatRoutine != null)
            {
                StopCoroutine(currentFloatRoutine);
            }
            currentFloatRoutine = StartCoroutine(FloatAndFadeText(pointsPerInterval));
        }
    }
    public override void StopBuilding()
    {
        base.StopBuilding(); 
        PointManager.instance.UnregisterPassiveBuilding(pointsPerInterval, this);
    }
}