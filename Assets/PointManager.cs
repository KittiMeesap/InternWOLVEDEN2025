using TMPro;
using UnityEngine;

public class PointManager : MonoBehaviour
{
    public static PointManager instance { get; private set; }

    public int points = 0;
    public int pointsPerClick = 1;
    public TMP_Text pointsText;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        UpdatePointsText();
    }

    public void AddPoints(int amount)
    {
        points += amount;
        UpdatePointsText();
    }

    public void UpdatePointsText()
    {
        if (pointsText != null)
        {
            pointsText.text = "Points: " + points.ToString();
        }
    }
}
