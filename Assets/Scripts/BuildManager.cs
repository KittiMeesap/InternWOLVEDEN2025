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
    public GameObject backTilemap;
    public TileBase canPlaceTile;
    public TileBase cannotPlaceTile;
    public TileBase noBuildZoneTile;
    public TileBase clickablePointTile;

    public TileBase blackGrindTilebase;
    private HashSet<Vector3Int> unlockedCells = new HashSet<Vector3Int>();

    public LayerMask clickableGroundLayer;

    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();
    private Vector3Int lastHoveredCell = Vector3Int.one * -1;

    private Camera mainCamera;

    private int currentBuildingCost = 0;
    private GameObject currentBuildingPrefab;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        mainCamera = Camera.main;
    }

    private void Update()
    {
        bool isPointerOverUI = IsPointerOverUI();

        if (currentBuildingInstance != null)
        {
            Vector3 targetPosition = GetTargetPosition();
            currentBuildingInstance.transform.position = targetPosition;
            Vector3Int currentCell = groundTilemap.WorldToCell(targetPosition);
            if (!currentBuildingInstance.activeSelf)
            {
                if (Vector3.Distance(currentBuildingInstance.transform.position, GetTargetPosition()) > 0.1f) 
                {
                    currentBuildingInstance.SetActive(true); 
                }
            }
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
                    if (currentBuildingInstance.activeSelf) 
                    {
                        currentBuildingInstance.SetActive(false); 
                    }
                }
                UpgradePanelManager.instance.OpenUpgradePanel();
                backTilemap.SetActive(false);
                Destroy(currentBuildingInstance);
                currentBuildingInstance = null;
                lastHoveredCell = Vector3Int.one * -1;
                ClearAllHighlights();
                currentBuildingPrefab = null;
            }
        }
        else 
        {
            if (lastHoveredCell != Vector3Int.one * -1)
            {
                ClearHighlight();
                lastHoveredCell = Vector3Int.one * -1;
            }

            if (Input.GetMouseButtonDown(0) && !isPointerOverUI)
            {
                CheckForClickableTileWithRaycast();
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
            currentBuildingPrefab = prefabToInstantiate;
            currentBuildingCost = costOfBuilding;
            
            currentBuildingInstance = Instantiate(currentBuildingPrefab);
            currentBuildingInstance.SetActive(false); 
            currentBuildingInstance.transform.position = Vector3.zero; 
          

            UpgradePanelManager.instance.CloseUpgradePanel();
            backTilemap.SetActive(true);
            
            lastHoveredCell = Vector3Int.one * -1;
            ClearAllHighlights();
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public Vector3 GetTargetPosition()
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
            if (!currentBuildingInstance.activeSelf) 
            {
                currentBuildingInstance.SetActive(true); 
            }
            currentBuildingInstance.transform.position = position;

            BaseBuilding baseBuildingScript = currentBuildingInstance.GetComponent<BaseBuilding>();
            if (baseBuildingScript != null)
            {
                baseBuildingScript.StartBuilding();

                UnlockBlackGrindBuilding unlockBuilding = baseBuildingScript as UnlockBlackGrindBuilding;
                if (unlockBuilding != null)
                {
                    occupiedCells.Remove(cellToPlace);
                    if (currentBuildingInstance.activeSelf)
                    {
                        currentBuildingInstance.SetActive(false); 
                    }
                    Destroy(currentBuildingInstance);
                    currentBuildingInstance = null; 
                    lastHoveredCell = Vector3Int.one * -1;
                    ClearAllHighlights();
                }
                else
                {
                    occupiedCells.Add(cellToPlace);
                    currentBuildingInstance = null; 
                    lastHoveredCell = Vector3Int.one * -1;
                    ClearAllHighlights();
                }
            }
            else 
            {
                occupiedCells.Add(cellToPlace);
                currentBuildingInstance = null;
                lastHoveredCell = Vector3Int.one * -1;
                ClearAllHighlights();
            }
            
            if (PointManager.instance != null && currentBuildingPrefab != null && PointManager.instance.points >= currentBuildingCost)
            {
                 currentBuildingInstance = Instantiate(currentBuildingPrefab);
                 currentBuildingInstance.SetActive(false); 
                 currentBuildingInstance.transform.position = Vector3.zero;
               
            }
            else
            {
                 UpgradePanelManager.instance.OpenUpgradePanel();
                 backTilemap.SetActive(false);
                 currentBuildingPrefab = null; 
            }
        }
        else // ถ้าวางไม่ได้
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
                if (currentBuildingInstance.activeSelf) 
                {
                    currentBuildingInstance.SetActive(false);
                }
            }
            UpgradePanelManager.instance.OpenUpgradePanel();
            backTilemap.SetActive(false);
            Destroy(currentBuildingInstance);
            currentBuildingInstance = null;
            lastHoveredCell = Vector3Int.one * -1;
            ClearAllHighlights();
            currentBuildingPrefab = null;
        }
    }

    private bool IsCellOccupiedOrNoBuildZone(Vector3Int cell)
    {
        if (occupiedCells.Contains(cell))
        {
            return true;
        }
        bool isCurrentBuildingUnlocker = false;
        if (currentBuildingInstance != null)
        {
            isCurrentBuildingUnlocker = currentBuildingInstance.GetComponent<UnlockBlackGrindBuilding>() != null;
        }
        TileBase tileAtGroundMap = groundTilemap?.GetTile(cell);

        if (tileAtGroundMap != null && tileAtGroundMap == blackGrindTilebase)
        {
            if (!unlockedCells.Contains(cell))
            {
                if (isCurrentBuildingUnlocker)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        if (tileAtGroundMap != null)
        {
            if (tileAtGroundMap == noBuildZoneTile)
            {
                return true;
            }
            if (tileAtGroundMap == clickablePointTile)
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
            bool isCurrentBuildingUnlocker = false;
            if (currentBuildingInstance != null)
            {
                isCurrentBuildingUnlocker = currentBuildingInstance.GetComponent<UnlockBlackGrindBuilding>() != null;
            }

            if (blackGrindTilebase != null)
            {
                TileBase tileAtGroundMapForHighlight = groundTilemap?.GetTile(cell);
                if (tileAtGroundMapForHighlight != null && tileAtGroundMapForHighlight == blackGrindTilebase && !unlockedCells.Contains(cell))
                {
                    if (!isCurrentBuildingUnlocker)
                    {
                        highlightTilemap.SetTile(cell, cannotPlaceTile);
                        return;
                    }
                    else
                    {
                        highlightTilemap.SetTile(cell, canPlaceTile);
                        return;
                    }
                }
            }
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

    private void ClearAllHighlights()
    {
        if (highlightTilemap != null)
        {
            highlightTilemap.ClearAllTiles();
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
                        
                        if (FloatingTextPool.instance != null)
                        {
                            FloatingTextPool.instance.ShowFloatingText(hit.point, $"+ {PointManager.instance.currentPointsPerClickOnTile}");
                        }

                        if (SoundManager.instance != null && SoundManager.instance.soundTileClickPoint != null)
                        {
                            SoundManager.instance.PlaySound(SoundManager.instance.soundTileClickPoint);
                        }
                    }
                }
            }
        }
    }

    public void UnlockTile(Vector3Int cell)
    {
        if (groundTilemap != null && blackGrindTilebase != null)
        {
            TileBase tileAtCell = groundTilemap.GetTile(cell);
            if (tileAtCell != null && tileAtCell == blackGrindTilebase)
            {
                groundTilemap.SetTile(cell, null);
                unlockedCells.Add(cell);
            }
           
        }
    }
    
}