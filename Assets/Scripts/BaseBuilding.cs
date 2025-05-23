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
            SoundManager.instance?.PlaySound(SoundManager.instance.soundBuildingClick);
        }
    }

    public virtual void StopBuilding()
    {
        isBuildingActive = false;
        SoundManager.instance?.PlaySound(SoundManager.instance.soundDestroyBuilding);
    }

}