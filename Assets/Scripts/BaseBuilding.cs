using UnityEngine;

public abstract class BaseBuilding : MonoBehaviour
{
    [Header("Building Info")]
    [SerializeField] public string buildingName = "New Building";
    [SerializeField] public int buildingCost = 10;

    protected bool isBuildingActive = false;

    public virtual void StartBuilding()
    {
        if (!isBuildingActive)
        {
            isBuildingActive = true;
            Debug.Log(buildingName + " activated");
            SoundManager.instance?.PlayBuildingClick();
        }
    }

    public virtual void StopBuilding()
    {
        isBuildingActive = false;
        Debug.Log(buildingName + " deactivated");

        SoundManager.instance?.PlayDestroyBuilding();
    }

}