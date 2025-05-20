using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    public GameObject buildingPrefab; 
    private GameObject currentBuildingInstance; 
    public Tilemap groundTilemap; 
    
    public Tilemap highlightTilemap; 
    public TileBase canPlaceTile;    
    public TileBase cannotPlaceTile; 
    
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>(); 
    private Vector3Int lastHoveredCell = Vector3Int.one * -1; // ใช้ค่าที่ไม่น่าเป็นไปได้แทน 9999
    
    // Cached Camera reference เพื่อลดการเรียก Camera.main บ่อยๆ
    private Camera mainCamera; 

    private void Awake()
    {
        mainCamera = Camera.main; // เก็บ reference ของกล้องหลักเมื่อเริ่มต้น
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Please ensure your camera is tagged 'MainCamera'.");
        }
    }

    private void Update()
    {
        // ตรวจสอบ Input.GetMouseButtonDown(0) เพียงครั้งเดียว
        // และ currentBuildingInstance == null เพียงครั้งเดียว
        bool mouseLeftButtonDown = Input.GetMouseButtonDown(0);
        bool mouseLeftButtonUp = Input.GetMouseButtonUp(0);

        if (mouseLeftButtonDown && currentBuildingInstance == null)
        {
            currentBuildingInstance = Instantiate(buildingPrefab); 
        }

        if (currentBuildingInstance != null)
        {
            Vector3 targetPosition = GetTargetPosition();
            currentBuildingInstance.transform.position = targetPosition;
            
            Vector3Int currentCell = groundTilemap.WorldToCell(targetPosition);
            
            // Optimize: ตรวจสอบเฉพาะเมื่อ Cell ที่เมาส์อยู่เปลี่ยนไป
            if (currentCell != lastHoveredCell)
            {
                ClearHighlight(); 
                lastHoveredCell = currentCell;
                HighlightCell(currentCell, IsCellOccupied(currentCell));
            }
            
            if (mouseLeftButtonUp)
            {
                PlaceBuilding(targetPosition);
            }
        }
        else if (lastHoveredCell != Vector3Int.one * -1) // ใช้ค่าที่ไม่น่าเป็นไปได้เดียวกัน
        {
            // Optimize: ล้างไฮไลต์เมื่อไม่มีการลากและมีไฮไลต์ค้างอยู่
            ClearHighlight();
            lastHoveredCell = Vector3Int.one * -1; 
        }
    }

    private Vector3 GetTargetPosition()
    {
        // ใช้ cached camera
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition); 
        mousePos.z = 0f; 
        Vector3Int currentCell = groundTilemap.WorldToCell(mousePos); 
        return groundTilemap.GetCellCenterWorld(currentCell); 
    }

    private void PlaceBuilding(Vector3 position)
    {
        Vector3Int cellToPlace = groundTilemap.WorldToCell(position);

        if (!IsCellOccupied(cellToPlace))
        {
            currentBuildingInstance.transform.position = position; 
            
            BuildIng buildIngScript = currentBuildingInstance.GetComponent<BuildIng>();
            if (buildIngScript != null)
            {
                buildIngScript.StartBuilding(); 
            }
            
            occupiedCells.Add(cellToPlace); 
            Debug.Log($"Building placed at: {cellToPlace}"); // ใช้ string interpolation
            ClearHighlight(); 
            lastHoveredCell = Vector3Int.one * -1; 
            currentBuildingInstance = null; 
        }
        else
        {
            Debug.LogWarning($"Cannot place building: Cell {cellToPlace} is already occupied!");
            // การ Destroy GameObject ที่เพิ่ง Instantiate มาทันทีก็ไม่ใช่ปัญหาประสิทธิภาพใหญ่
            Destroy(currentBuildingInstance); 
            currentBuildingInstance = null;
            ClearHighlight(); 
            lastHoveredCell = Vector3Int.one * -1; 
        }
    }

    private bool IsCellOccupied(Vector3Int cell)
    {
        return occupiedCells.Contains(cell);
    }

    private void HighlightCell(Vector3Int cell, bool isOccupied)
    {
        if (highlightTilemap != null)
        {
            highlightTilemap.SetTile(cell, isOccupied ? cannotPlaceTile : canPlaceTile); // ใช้ Conditional (Ternary) Operator
        }
    }

    private void ClearHighlight()
    {
        if (highlightTilemap != null && lastHoveredCell != Vector3Int.one * -1)
        {
            highlightTilemap.SetTile(lastHoveredCell, null); 
        }
    }
}