using TMPro;
using UnityEngine;

public class ClickToAddPoints : MonoBehaviour
{
    public int points = 0; // total score     
    public int pointsPerClick = 1; // score per click
    public TMP_Text pointsText; // UI score text

    void Start()
    {
        UpdatePointsText();
    }

    void Update()
    {
        // left click to add points
        if (Input.GetMouseButtonDown(0))
        {
            AddPoints();
        }
    }

    void AddPoints()
    {
        points += pointsPerClick;
        UpdatePointsText();
    }

    void UpdatePointsText()
    {
        if (pointsText != null)
        {
            pointsText.text = "Points: " + points.ToString();
        }
    }
}
