using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildManager : MonoBehaviour
{
    public GameObject buildingPrefab;
    private GameObject currentBuildingInstance;
    public Tilemap groundTilemap;

    private void Update()
    {
        // เริ่มสร้างเมื่อคลิกซ้าย และยังไม่มีสิ่งก่อสร้างที่กำลังวาง
        if (Input.GetMouseButtonDown(0) && currentBuildingInstance == null)
        {
            currentBuildingInstance = Instantiate(buildingPrefab);
        }

        if (currentBuildingInstance != null)
        {
            Vector3 targetPosition = GetTargetPosition();
            currentBuildingInstance.transform.position = targetPosition;

            if (Input.GetMouseButtonUp(0))
            {
                PlaceBuilding(targetPosition);
            }
        }
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3Int cell = groundTilemap.WorldToCell(mousePos);
        return groundTilemap.GetCellCenterWorld(cell);
    }

    private void PlaceBuilding(Vector3 position)
    {
        currentBuildingInstance.transform.position = position;
        currentBuildingInstance = null;
    }
}
