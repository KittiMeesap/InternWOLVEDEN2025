using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance { get; private set; }

    private GameObject currentBuildingInstance;
    public Tilemap groundTilemap;
    public Tilemap highlightTilemap;
    public TileBase canPlaceTile;
    public TileBase cannotPlaceTile;
    public TileBase noBuildZoneTile;
    public TileBase clickablePointTile;

    public LayerMask clickableGroundLayer; // <--- ตรวจสอบว่าถูกตั้งค่าใน Inspector!

    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();
    private Vector3Int lastHoveredCell = Vector3Int.one * -1;

    private Camera mainCamera; // <--- ตรวจสอบว่ามีค่า ไม่เป็น null

    private int currentBuildingCost = 0;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            // ถ้าขึ้น Error นี้ แสดงว่ากล้องของคุณไม่ได้ถูก Tag เป็น "MainCamera"
            Debug.LogError("Main Camera not found! Please ensure your camera is tagged 'MainCamera'.");
        }
    }

    private void Update()
    {
        
        bool isPointerOverUI = IsPointerOverUI();

        if (currentBuildingInstance != null)
        {
            
            Vector3 targetPosition = GetTargetPosition();
            currentBuildingInstance.transform.position = targetPosition;
            Vector3Int currentCell = groundTilemap.WorldToCell(targetPosition);

            if (currentCell != lastHoveredCell)
            {
                ClearHighlight();
                lastHoveredCell = currentCell;
                HighlightCell(currentCell, IsCellOccupiedOrNoBuildZone(currentCell));
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!isPointerOverUI)
                {
                    PlaceBuilding(targetPosition);
                }
            }

            if (Input.GetMouseButtonDown(1)) // คลิกขวาเพื่อยกเลิก
            {
                Debug.Log("Building placement cancelled. Refunding points.");
                if (PointManager.instance != null && currentBuildingCost > 0)
                {
                    PointManager.instance.AddPoints(currentBuildingCost);
                    currentBuildingCost = 0;
                }
                if (currentBuildingInstance != null)
                {
                    BaseBuilding baseBuildingScript = currentBuildingInstance.GetComponent<BaseBuilding>();
                    if (baseBuildingScript != null)
                    {
                        baseBuildingScript.StopBuilding(); // <-- ถูกเรียกครั้งที่ 1
                    }
                }
                UpgradePanelManager.instance.OpenUpgradePanel();
                Destroy(currentBuildingInstance);
                currentBuildingInstance = null;
                ClearHighlight();
                lastHoveredCell = Vector3Int.one * -1;
            }
        }
        else // ไม่มี building กำลังถูกลากอยู่
        {
            if (Input.GetMouseButtonDown(0) && !isPointerOverUI)
            {
                CheckForClickableTileWithRaycast(); // <--- โค้ดส่วนนี้
            }

            if (lastHoveredCell != Vector3Int.one * -1)
            {
                ClearHighlight();
                lastHoveredCell = Vector3Int.one * -1;
            }
        }
    }

    public bool CanInstantiateBuilding()
    {
        return currentBuildingInstance == null;
    }

    public void InstantiateBuilding(GameObject prefabToInstantiate, int costOfBuilding)
    {
        if (currentBuildingInstance == null)
        {
            currentBuildingInstance = Instantiate(prefabToInstantiate);
            UpgradePanelManager.instance.CloseUpgradePanel();
            currentBuildingCost = costOfBuilding;
        }
        else
        {
            Debug.LogWarning("Already dragging a building. Cannot instantiate another one.");
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3Int currentCell = groundTilemap.WorldToCell(mousePos);
        return groundTilemap.GetCellCenterWorld(currentCell);
    }

    private void PlaceBuilding(Vector3 position)
    {
        Vector3Int cellToPlace = groundTilemap.WorldToCell(position);

        if (!IsCellOccupiedOrNoBuildZone(cellToPlace))
        {
            currentBuildingInstance.transform.position = position;

            BaseBuilding baseBuildingScript = currentBuildingInstance.GetComponent<BaseBuilding>();
            if (baseBuildingScript != null)
            {
                baseBuildingScript.StartBuilding();
            }

            occupiedCells.Add(cellToPlace);
            ClearHighlight();
            lastHoveredCell = Vector3Int.one * -1;
            currentBuildingInstance = null;
        }
        else
        {
            if (PointManager.instance != null && currentBuildingCost > 0)
            {
                PointManager.instance.AddPoints(currentBuildingCost);
                currentBuildingCost = 0;
            }
            if (currentBuildingInstance != null)
            {
                BaseBuilding baseBuildingScript = currentBuildingInstance.GetComponent<BaseBuilding>();
                if (baseBuildingScript != null)
                {
                    baseBuildingScript.StopBuilding(); 
                }
            }
            Destroy(currentBuildingInstance);
            currentBuildingInstance = null;
            ClearHighlight();
            lastHoveredCell = Vector3Int.one * -1;
        }
        UpgradePanelManager.instance.OpenUpgradePanel();
    }

    private bool IsCellOccupiedOrNoBuildZone(Vector3Int cell)
    {
        if (occupiedCells.Contains(cell))
        {
            return true;
        }

        TileBase tileAtCell = groundTilemap.GetTile(cell);
        if (tileAtCell != null)
        {
            if (tileAtCell == noBuildZoneTile)
            {
                return true;
            }
            if (tileAtCell == clickablePointTile)
            {
                return true;
            }
        }
        return false;
    }
    private void HighlightCell(Vector3Int cell, bool isOccupied)
    {
        if (highlightTilemap != null)
        {
            highlightTilemap.SetTile(cell, isOccupied ? cannotPlaceTile : canPlaceTile);
        }
    }

    private void ClearHighlight()
    {
        if (highlightTilemap != null && lastHoveredCell != Vector3Int.one * -1)
        {
            highlightTilemap.SetTile(lastHoveredCell, null);
        }
    }

    private void CheckForClickableTileWithRaycast()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, clickableGroundLayer);

        if (hit.collider != null)
        {
            Tilemap hitTilemap = hit.collider.GetComponent<Tilemap>();
            if (hitTilemap != null && hitTilemap == groundTilemap)
            {
                Vector3Int clickedCell = groundTilemap.WorldToCell(hit.point);

                TileBase tileAtClickedCell = groundTilemap.GetTile(clickedCell);

                if (tileAtClickedCell != null && tileAtClickedCell == clickablePointTile)
                {
                    if (PointManager.instance != null)
                    {
                        PointManager.instance.AddPointsForTileClick();
                        SoundManager.instance.PlaySound(SoundManager.instance.audioClickPoints);
                    }
                }
            }
        }
       
    }
}