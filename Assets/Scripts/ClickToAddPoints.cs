using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClickToAddPoints : MonoBehaviour
{
    public int points = 0; // total score
    public int pointsPerClick = 1; // points added per click
    public Text pointsText; // UI score text

    void Start()
    {
        UpdatePointsText();
    }

    void Update()
    {
        //Left Click to add points
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
