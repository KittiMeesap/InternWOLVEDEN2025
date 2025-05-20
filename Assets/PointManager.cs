using TMPro;
using UnityEngine;
public class PointManager : MonoBehaviour
{
    public static PointManager instance{ get; private set; }
    public int points = 0; // total score     
    public int pointsPerClick = 1; // score per click
    public TMP_Text pointsText; // UI score text

    void Start()
    {
        instance = this; 
        UpdatePointsText();
    }

    void Update()
    {
        // left click to add points
      /*  if (Input.GetMouseButtonDown(0))
        {
            AddPoints(pointsPerClick);
        }*/
    }
   public void AddPoints(int pointsAdd)
    {
        points += pointsAdd;
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
