using UnityEngine;

public class DistractionEnemy : MonoBehaviour
{
    private float timeAlive = 0f;
    private float nextPointLossTime = 1f;
    private int damageRate = 1;
    private bool isActive = false;

    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        isActive = true;
        timeAlive = 0f;
        nextPointLossTime = 1f;
        damageRate = 1;
        gameObject.SetActive(true);

        Vector2 randomPos = new Vector2(Random.Range(100f, Screen.width - 100f), Random.Range(100f, Screen.height - 100f));
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, randomPos, canvas.worldCamera, out anchoredPos);
        rectTransform.anchoredPosition = anchoredPos;
    }

    public void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isActive) return;

        timeAlive += Time.deltaTime;
        if (timeAlive >= nextPointLossTime)
        {
            PointManager.instance.points -= damageRate;
            if (PointManager.instance.points < 0) PointManager.instance.points = 0;
            PointManager.instance.UpdatePointsText();

            nextPointLossTime += 1f;
            damageRate++;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos, canvas.worldCamera))
            {
                Deactivate();
            }
        }
    }
}
