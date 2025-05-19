using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildManager : MonoBehaviour
{
    public GameObject buildingPrefab; 
    private GameObject currentBuildingInstance; 
    public Tilemap groundTilemap; 

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && currentBuildingInstance == null)
        {
            currentBuildingInstance = Instantiate(buildingPrefab); 
        }

        // ถ้ามีสิ่งก่อสร้างที่กำลังลากอยู่
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

    // ฟังก์ชันหาตำแหน่งที่จะวางสิ่งก่อสร้าง
    private Vector3 GetTargetPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        mousePos.z = 0f; 
        Vector3Int currentCell = groundTilemap.WorldToCell(mousePos); // แปลงตำแหน่งจากโลกจริงไป Cell ใน Tilemap 
        return groundTilemap.GetCellCenterWorld(currentCell); //  แปลงตำแหน่งกึ่งกลางของ  Cell ไปเป็นตำแหน่งจากโลกจริง
    }

    // ฟังก์ชันวางสิ่งก่อสร้าง
    private void PlaceBuilding(Vector3 position)
    {
        currentBuildingInstance.transform.position = position; 
        currentBuildingInstance = null; 
    }
}