using UnityEngine;
using TMPro;

public class PassivePointBuilding : BuildIngWithFloatingText
{
    [Header("Passive Point Settings")]
    [SerializeField] private float generateInterval = 3f;
    [SerializeField] private int pointsPerInterval = 1;

    private float timer;

    protected override void Start()
    {
        base.Start();
        timer = generateInterval;
        if (pointsText != null)
        {
            pointsText.text = $"+ {pointsPerInterval}";
        }
    }

    protected override void Update()
    {
        if (isBuildingActive)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                if (PointManager.instance != null)
                {
                    PointManager.instance.AddPoints(pointsPerInterval);
                }

                if (pointsText != null)
                {
                    if (currentFloatRoutine != null)
                    {
                        StopCoroutine(currentFloatRoutine);
                    }
                    currentFloatRoutine = StartCoroutine(FloatAndFadeText(pointsPerInterval));
                }
                timer = generateInterval;
            }
        }
    }

    public override void StartBuilding()
    {
        base.StartBuilding();
        timer = generateInterval;
    }

    public void SetPointsPerInterval(int newPoints)
    {
        pointsPerInterval = newPoints;
        if (pointsText != null)
        {
            pointsText.text = $"+ {pointsPerInterval}";
        }
    }
}