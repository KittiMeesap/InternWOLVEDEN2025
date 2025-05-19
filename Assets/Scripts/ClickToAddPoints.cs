using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClickToAddPoints : MonoBehaviour
{
    public int points = 0;              //Total Scores
    public int pointsPerClick = 1;      // Score per Click
    public TMP_Text pointsText;             // UI Score Text

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
