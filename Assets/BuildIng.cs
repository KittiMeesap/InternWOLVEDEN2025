using UnityEngine;

public class BuildIng : MonoBehaviour
{
    [SerializeField] private float timeInterval = 1f;
    [SerializeField] public int pointsPerSec = 1;

    private float timer;

    void Start()
    {
        timer = timeInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            PointManager.instance.AddPoints(pointsPerSec);
            timer = timeInterval;
        }
    }
}
