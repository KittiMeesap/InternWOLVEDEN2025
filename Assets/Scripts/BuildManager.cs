using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.EventSystems; // ต้องใช้สำหรับการเช็ค UI

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance { get; private set; }

    private GameObject currentBuildingVisualInstance; 
    public Tilemap groundTilemap;
    public Tilemap highlightTilemap;
    public GameObject backTilemap; // Canvas หรือ GameObject ที่ปิดเมื่อเข้าโหมดสร้าง
    public TileBase canPlaceTile;
    public TileBase cannotPlaceTile;
    public TileBase noBuildZoneTile;
    public TileBase clickablePointTile; // สำหรับ Clicker game

    public TileBase blackGrindTilebase; // Tile ที่ต้องปลดล็อค
    private HashSet<Vector3Int> unlockedCells = new HashSet<Vector3Int>(); // เซลล์ที่ปลดล็อคแล้ว

    public LayerMask clickableGroundLayer; // Layer สำหรับ Raycast ตรวจจับ Tile

    private Dictionary<Vector3Int, GameObject> occupiedCellsAndBuildings = new Dictionary<Vector3Int, GameObject>(); // Cell ที่มีสิ่งก่อสร้างและ GameObject ของสิ่งก่อสร้างนั้น
    private Vector3Int lastHoveredCell = Vector3Int.one * -1; // Cell ล่าสุดที่เมาส์อยู่

    private Camera mainCamera;

    private int currentBuildingCost = 0; // ราคาของ Building ที่เลือกอยู่
    private GameObject currentBuildingPrefab; // Prefab ของ Building ที่เลือกอยู่
    private bool isCostAlreadyPaid = false; // ตรวจสอบว่าหักเงินไปแล้วหรือไม่ (ตอนเลือกซื้อ)

    [Header("Bonus Tilemap Settings")]
    public Tilemap bonusTilemap; // Tilemap สำหรับ Bonus Area
    public TileBase passiveBonusTile; // Tile ที่แสดงว่ามี Bonus

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        mainCamera = Camera.main; // อ้างอิง Main Camera
    }

    private void Update()
    {
        bool isPointerOverUI = IsPointerOverUI(); // เช็คว่าเมาส์อยู่เหนือ UI หรือไม่

        if (currentBuildingPrefab != null) // ถ้ามีการเลือก Building มาสร้าง
        {
            Vector3 targetPosition = GetTargetPosition(); // ตำแหน่งเมาส์บน World Space
            Vector3Int currentCell = groundTilemap.WorldToCell(targetPosition); // Cell ที่เมาส์อยู่

            // ตรวจสอบว่าเป็น Building ที่ควรมี Visual (Preview) หรือไม่
            bool isBuildingThatShouldHaveVisual = 
                currentBuildingPrefab.GetComponent<UnlockBlackGrindBuilding>() == null && 
                currentBuildingPrefab.GetComponent<RemoverBuilding>() == null;

            if (isBuildingThatShouldHaveVisual)
            {
                if (currentBuildingVisualInstance == null)
                {
                    currentBuildingVisualInstance = Instantiate(currentBuildingPrefab);
                    currentBuildingVisualInstance.SetActive(false); // เริ่มต้นปิดไว้ก่อน
                }
                currentBuildingVisualInstance.transform.position = targetPosition;
                if (!currentBuildingVisualInstance.activeSelf)
                {
                    // แสดง Visual เมื่อมีการเคลื่อนไหวของเมาส์มากพอ
                    if (Vector3.Distance(currentBuildingVisualInstance.transform.position, GetTargetPosition()) > 0.1f) 
                    {
                        currentBuildingVisualInstance.SetActive(true); 
                    }
                }
            } else {
                // ถ้าเป็น Building ที่ไม่ควรมี Visual (เช่น Remover), ให้ทำลาย Visual เก่าทิ้ง
                if (currentBuildingVisualInstance != null)
                {
                    Destroy(currentBuildingVisualInstance);
                    currentBuildingVisualInstance = null;
                }
            }


            if (currentCell != lastHoveredCell) // ถ้าเมาส์เปลี่ยน Cell
            {
                ClearHighlight(); // ลบ Highlight เก่า
                lastHoveredCell = currentCell; // อัปเดต Cell ใหม่
                HighlightCell(currentCell, IsCellOccupiedOrNoBuildZone(currentCell)); // Highlight Cell ใหม่
            }

            if (Input.GetMouseButtonDown(0)) // คลิกซ้ายเพื่อวาง
            {
                if (!isPointerOverUI) // ไม่วางถ้าคลิกบน UI
                {
                    PlaceBuilding(targetPosition);
                }
            }

            if (Input.GetMouseButtonDown(1)) // คลิกขวาเพื่อยกเลิก
            {
                CancelCurrentBuilding(true); // ไม่คืนเงินถ้าเป็นการกดคลิกขวาเพื่อยกเลิกเฉยๆ (ยังไม่ได้วาง)
            }
        }
        else // ถ้าไม่มี Building ที่เลือก (อยู่ในโหมดปกติ)
        {
            if (lastHoveredCell != Vector3Int.one * -1) // ถ้ามี Cell ที่เคย Highlight อยู่
            {
                ClearHighlight(); // ลบ Highlight ทิ้ง
                lastHoveredCell = Vector3Int.one * -1; // รีเซ็ตค่า
            }
            if (currentBuildingVisualInstance != null) // ถ้ามี Visual ค้างอยู่ (ไม่ควรมี)
            {
                Destroy(currentBuildingVisualInstance);
                currentBuildingVisualInstance = null;
            }

            if (Input.GetMouseButtonDown(0) && !isPointerOverUI) // คลิกซ้ายเพื่อเก็บ Point จาก Tile (ถ้ามี)
            {
                CheckForClickableTileWithRaycast();
            }
        }
    }

    public bool CanInstantiateBuilding()
    {
        return currentBuildingPrefab == null; // ตรวจสอบว่าสามารถเลือก Building ได้หรือไม่
    }

    // ฟังก์ชันสำหรับเลือก Building ที่จะสร้าง
    public void InstantiateBuilding(GameObject prefabToInstantiate, int costOfBuilding)
    {
        if (currentBuildingPrefab != null) // ถ้ามี Building ที่เลือกอยู่แล้ว
        {
            CancelCurrentBuilding(false); // ยกเลิกอันเก่าโดยไม่คืนเงิน (ถือว่าผู้เล่นเปลี่ยนใจ)
        }

        if (PointManager.instance == null)
        {
            Debug.LogError("PointManager instance is null! Cannot buy building.");
            return;
        }

        Debug.Log($"Attempting to buy: {prefabToInstantiate.name} with cost: {costOfBuilding}. Current points: {PointManager.instance.points}");

        if (PointManager.instance.points < costOfBuilding)
        {
            Debug.Log("Not enough points to buy this building!");
            return; // ไม่พอจ่าย
        }
        
        // กำหนด Building ที่เลือก
        currentBuildingPrefab = prefabToInstantiate; 
        currentBuildingCost = costOfBuilding; 
        isCostAlreadyPaid = true; // ตั้งค่าว่าจ่ายเงินแล้ว

        // หักเงินทันทีที่เลือกซื้อ
        PointManager.instance.AddPoints(-currentBuildingCost); 
        Debug.Log($"Points deducted for first item: {currentBuildingCost}. Remaining points: {PointManager.instance.points}");

        // สร้าง Visual Preview (ถ้าจำเป็น)
        bool isBuildingThatShouldHaveVisual = 
            currentBuildingPrefab.GetComponent<UnlockBlackGrindBuilding>() == null && 
            currentBuildingPrefab.GetComponent<RemoverBuilding>() == null;

        if (isBuildingThatShouldHaveVisual)
        {
            currentBuildingVisualInstance = Instantiate(currentBuildingPrefab);
            currentBuildingVisualInstance.SetActive(false); // ซ่อนไว้ก่อน
            currentBuildingVisualInstance.transform.position = Vector3.zero; 
        }
          
        UpgradePanelManager.instance.CloseUpgradePanel(); // ปิด Panel
        backTilemap.SetActive(true); // เปิด Background Tilemap
        
        lastHoveredCell = Vector3Int.one * -1; // รีเซ็ต Highlight
        ClearAllHighlights();
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // แปลงตำแหน่งเมาส์เป็น World Space และ Cell Center
    public Vector3 GetTargetPosition()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3Int currentCell = groundTilemap.WorldToCell(mousePos);
        return groundTilemap.GetCellCenterWorld(currentCell);
    }

    // ฟังก์ชันสำหรับวาง Building จริงๆ
    private void PlaceBuilding(Vector3 position)
    {
        if (currentBuildingPrefab == null) return; 

        Vector3Int cellToPlace = groundTilemap.WorldToCell(position);

        bool isSpecialBuilding = (currentBuildingPrefab.GetComponent<RemoverBuilding>() != null || 
                                  currentBuildingPrefab.GetComponent<UnlockBlackGrindBuilding>() != null);

        bool canPlace = true; // ตั้งค่าเริ่มต้นให้วางได้
        if (isSpecialBuilding)
        {
            if (currentBuildingPrefab.GetComponent<RemoverBuilding>() != null)
            {
                canPlace = occupiedCellsAndBuildings.ContainsKey(cellToPlace); // Remover วางได้ถ้ามี Building อยู่
            }
            else if (currentBuildingPrefab.GetComponent<UnlockBlackGrindBuilding>() != null)
            {
                TileBase tileAtGroundMap = groundTilemap?.GetTile(cellToPlace);
                // Unlocker วางได้ถ้าเป็น Black Grind Tile และยังไม่เคยปลดล็อค
                canPlace = (tileAtGroundMap != null && tileAtGroundMap == blackGrindTilebase && !unlockedCells.Contains(cellToPlace));
            }
            if (!canPlace)
            {
                Debug.Log($"Cannot place special building ({currentBuildingPrefab.name}) here. Cancelling building mode.");
                CancelCurrentBuilding(true); // ยกเลิกและคืนเงิน (เพราะพยายามวางแล้วไม่ได้)
                return; 
            }
        }
        else // Building ทั่วไป
        {
            canPlace = !IsCellOccupiedOrNoBuildZone(cellToPlace); // วางได้ถ้า Cell ว่างและไม่ใช่ No Build Zone
            if (!canPlace) 
            {
                Debug.Log($"Cannot place regular building ({currentBuildingPrefab.name}) here. Cancelling building mode.");
                CancelCurrentBuilding(true); // ยกเลิกและคืนเงิน (เพราะพยายามวางแล้วไม่ได้)
                return; 
            }
        }

        // *** ถ้ามาถึงตรงนี้ แสดงว่าสามารถวางได้แล้ว ***
        GameObject actualBuildingInstance = Instantiate(currentBuildingPrefab, position, Quaternion.identity);
        actualBuildingInstance.SetActive(true); 

        BaseBuilding baseBuildingScript = actualBuildingInstance.GetComponent<BaseBuilding>();
        if (baseBuildingScript != null)
        {
            PassivePointBuilding passiveBuilding = baseBuildingScript as PassivePointBuilding;
            if (passiveBuilding != null) 
            {
                int appliedMultiplier = 1; 
                // ตรวจสอบ Bonus Tilemap
                if (bonusTilemap != null && passiveBonusTile != null)
                {
                    TileBase tileAtBonusMap = bonusTilemap.GetTile(cellToPlace);
                    Debug.Log($"Checking bonus at cell {cellToPlace}. Tile on BonusTilemap: {tileAtBonusMap?.name ?? "None"}. Expected Passive Bonus Tile: {passiveBonusTile?.name ?? "None"}");

                    if (tileAtBonusMap != null && tileAtBonusMap == passiveBonusTile)
                    {
                        appliedMultiplier = 2; // ถ้ามี Bonus Tile ให้คูณ 2
                        Debug.Log($"!!! BONUS DETECTED !!! Applied x2 bonus to Passive Building at {cellToPlace}");
                    }
                    else
                    {
                        Debug.Log($"No bonus tile found at {cellToPlace} or tile mismatch. Applying x1 multiplier.");
                    }
                }
                else
                {
                    Debug.Log("Bonus Tilemap or Passive Bonus Tile not set in BuildManager Inspector. No bonus will be applied.");
                }
                
                // *** (A) สำคัญ: เรียก ApplyBonusMultiplier ก่อน StartBuilding ***
                // เพื่อให้ค่า CurrentPointsPerInterval ถูกตั้งค่า (และ Recalculate ใน PointManager) ก่อนที่ Building จะ Register ตัวเอง
                passiveBuilding.ApplyBonusMultiplier(appliedMultiplier); 
            }

            // *** (B) เรียก StartBuilding หลังจาก ApplyBonusMultiplier ***
            // StartBuilding จะลงทะเบียน Building กับ PointManager
            baseBuildingScript.StartBuilding(); 
        }

        if (!isSpecialBuilding) // ถ้าไม่ใช่ Building พิเศษ (Remover/Unlocker) ให้เพิ่มลงใน Dictionary ของ Cell ที่ถูกยึดครอง
        {
            occupiedCellsAndBuildings.Add(cellToPlace, actualBuildingInstance); 
        }
        
        // *** ลบ Block การหักเงินซ้ำซ้อนที่เคยมีตรงนี้ออกไปเลย ***
        // เพราะเงินถูกหักไปแล้วใน InstantiateBuilding() ตอนที่เลือก Building

        // รีเซ็ตสถานะการสร้างหลังจากวางเสร็จสิ้นสมบูรณ์
        UpgradePanelManager.instance.OpenUpgradePanel();
        backTilemap.SetActive(false);
        currentBuildingPrefab = null; 
        currentBuildingCost = 0; 
        isCostAlreadyPaid = false; // รีเซ็ตเป็น false เมื่อวางสำเร็จ
        if (currentBuildingVisualInstance != null)
        {
            Destroy(currentBuildingVisualInstance);
            currentBuildingVisualInstance = null;
        }
        lastHoveredCell = Vector3Int.one * -1;
        ClearAllHighlights();
    }

    // ตรวจสอบว่า Cell ถูกยึดครองหรืออยู่ใน No Build Zone หรือไม่
    private bool IsCellOccupiedOrNoBuildZone(Vector3Int cell)
    {
        if (occupiedCellsAndBuildings.ContainsKey(cell)) 
        {
            return true; // มี Building อื่นวางอยู่แล้ว
        }

        bool isCurrentBuildingUnlocker = (currentBuildingPrefab != null && currentBuildingPrefab.GetComponent<UnlockBlackGrindBuilding>() != null);
        bool isCurrentBuildingRemover = (currentBuildingPrefab != null && currentBuildingPrefab.GetComponent<RemoverBuilding>() != null); 
       
        TileBase tileAtGroundMap = groundTilemap?.GetTile(cell);

        if (tileAtGroundMap != null && tileAtGroundMap == blackGrindTilebase)
        {
            if (!unlockedCells.Contains(cell))
            {
                if (isCurrentBuildingUnlocker)
                {
                    return false; // ถ้าเป็น Unlocker สามารถวางบน Black Grind ที่ยังไม่ปลดล็อคได้
                }
                else
                {
                    return true; // Building อื่นๆ วางบน Black Grind ที่ยังไม่ปลดล็อคไม่ได้
                }
            }
        }
        
        if (tileAtGroundMap != null)
        {
            if (tileAtGroundMap == noBuildZoneTile)
            {
                return true; // เป็น No Build Zone
            }
            if (tileAtGroundMap == clickablePointTile && !isCurrentBuildingRemover) 
            {
                return true; // Clickable Point Tile วาง Building ปกติไม่ได้ (ยกเว้น Remover)
            }
        }
        return false; // วางได้
    }

    // แสดง Highlight บน Cell
    private void HighlightCell(Vector3Int cell, bool isOccupied)
    {
        if (highlightTilemap != null)
        {
            bool isCurrentBuildingUnlocker = (currentBuildingPrefab != null && currentBuildingPrefab.GetComponent<UnlockBlackGrindBuilding>() != null);
            bool isCurrentBuildingRemover = (currentBuildingPrefab != null && currentBuildingPrefab.GetComponent<RemoverBuilding>() != null);

            if (isCurrentBuildingRemover) // ถ้าเป็น Remover
            {
                if (occupiedCellsAndBuildings.ContainsKey(cell)) 
                {
                    highlightTilemap.SetTile(cell, canPlaceTile); // Highlight เขียวถ้ามี Building ให้ Remove
                }
                else
                {
                    highlightTilemap.SetTile(cell, cannotPlaceTile); // Highlight แดงถ้าไม่มี
                }
                return; 
            }
            else if (isCurrentBuildingUnlocker) // ถ้าเป็น Unlocker
            {
                TileBase tileAtGroundMapForHighlight = groundTilemap?.GetTile(cell);
                if (tileAtGroundMapForHighlight != null && tileAtGroundMapForHighlight == blackGrindTilebase && !unlockedCells.Contains(cell))
                {
                    highlightTilemap.SetTile(cell, canPlaceTile); // Highlight เขียวถ้าเป็น Black Grind ที่ยังไม่ปลดล็อค
                }
                else
                {
                    highlightTilemap.SetTile(cell, cannotPlaceTile); // Highlight แดงถ้าไม่ใช่
                }
                return; 
            }
            
            // ถ้าเป็น Passive Building และมี Bonus Tile
            if (currentBuildingPrefab != null && currentBuildingPrefab.GetComponent<PassivePointBuilding>() != null && bonusTilemap != null && passiveBonusTile != null)
            {
                TileBase tileAtBonusMapForHighlight = bonusTilemap.GetTile(cell);
                if (tileAtBonusMapForHighlight != null && tileAtBonusMapForHighlight == passiveBonusTile)
                {
                    highlightTilemap.SetTile(cell, canPlaceTile); // Highlight เขียวเป็นพิเศษสำหรับ Bonus Area
                    return; 
                }
            }

            // ถ้าเป็น Black Grind ที่ยังไม่ปลดล็อค (สำหรับ Building ปกติ)
            if (blackGrindTilebase != null)
            {
                TileBase tileAtGroundMapForHighlight = groundTilemap?.GetTile(cell);
                if (tileAtGroundMapForHighlight != null && tileAtGroundMapForHighlight == blackGrindTilebase && !unlockedCells.Contains(cell))
                {
                    highlightTilemap.SetTile(cell, cannotPlaceTile); // Building ปกติวางบน Black Grind ที่ไม่ปลดล็อคไม่ได้
                    return;
                }
            }
            // Highlight ตามสถานะ Occupied (ว่างหรือไม่)
            highlightTilemap.SetTile(cell, isOccupied ? cannotPlaceTile : canPlaceTile);
        }
    }

    private void ClearHighlight()
    {
        if (highlightTilemap != null && lastHoveredCell != Vector3Int.one * -1)
        {
            highlightTilemap.SetTile(lastHoveredCell, null); // ลบ Tile ที่ Highlight อยู่
        }
    }

    private void ClearAllHighlights()
    {
        if (highlightTilemap != null)
        {
            highlightTilemap.ClearAllTiles(); // ลบ Highlight ทั้งหมด
        }
    }

    // ตรวจจับการคลิกบน Clickable Tile
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

    // ปลดล็อค Tile (เปลี่ยน Black Grind เป็น Tile ว่าง)
    public void UnlockTile(Vector3Int cell)
    {
        if (groundTilemap != null && blackGrindTilebase != null)
        {
            TileBase tileAtCell = groundTilemap.GetTile(cell);
            if (tileAtCell != null && tileAtCell == blackGrindTilebase)
            {
                groundTilemap.SetTile(cell, null); // ลบ Tile
                unlockedCells.Add(cell); // เพิ่มเข้า HashSet ว่าปลดล็อคแล้ว
            }
           
        }
    }

    // ประมวลผลเมื่อใช้ Unlock Building
    public void ProcessUnlockBuilding(Vector3Int cell, GameObject unlockerGameObject)
    {
        UnlockTile(cell); // ปลดล็อค Tile
        if (unlockerGameObject != null)
        {
            Destroy(unlockerGameObject, 0.01f); // ทำลาย Unlocker ทิ้ง
        }
    }

    // ฟังก์ชันสำหรับยกเลิกโหมดสร้าง
    // refundIfCostPaid: true = คืนเงิน, false = ไม่คืนเงิน
    private void CancelCurrentBuilding(bool refundIfCostPaid)
    {
        // คืนเงินเฉพาะกรณีที่ isCostAlreadyPaid เป็น true และ refundIfCostPaid เป็น true
        if (isCostAlreadyPaid && refundIfCostPaid) 
        {
            PointManager.instance.AddPoints(currentBuildingCost); 
            Debug.Log($"Points refunded: {currentBuildingCost}. Remaining points: {PointManager.instance.points}");
        } else if (isCostAlreadyPaid && !refundIfCostPaid) {
            // กรณีนี้คือผู้เล่นกดคลิกขวาเพื่อยกเลิก แต่ยังไม่ได้วาง
            // หรือเปลี่ยนใจเลือก Building ใหม่ (InstantiateBuilding เรียกมา)
            Debug.Log("Build mode cancelled, no refund needed as building was not placed.");
        }
        
        currentBuildingCost = 0; 
        isCostAlreadyPaid = false; 

        if (currentBuildingVisualInstance != null) 
        {
            Destroy(currentBuildingVisualInstance); 
            currentBuildingVisualInstance = null; 
        }

        UpgradePanelManager.instance.OpenUpgradePanel(); // เปิด Panel คืน
        backTilemap.SetActive(false); // ปิด Background Tilemap
        currentBuildingPrefab = null; // รีเซ็ต Building ที่เลือก
        lastHoveredCell = Vector3Int.one * -1; // รีเซ็ต Highlight
        ClearAllHighlights();
    }
    
    // รื้อถอน Building
    public void DemolishBuildingAtCell(Vector3Int cell, int refundPercentage) 
    {
        if (occupiedCellsAndBuildings.TryGetValue(cell, out GameObject buildingToDemolish)) 
        {
            BaseBuilding baseBuildingScript = buildingToDemolish.GetComponent<BaseBuilding>(); 
            if (baseBuildingScript != null)
            {
                int refundPoints = (int)(baseBuildingScript.buildingCost * (refundPercentage / 100f)); 
                if (PointManager.instance != null)
                {
                    PointManager.instance.AddPoints(refundPoints); 
                }

                baseBuildingScript.StopBuilding(); // เรียก StopBuilding เพื่อ Unregister จาก PointManager (ถ้าเป็น Passive)
                
                if (SoundManager.instance != null && SoundManager.instance.soundDestroyBuilding != null)
                {
                    SoundManager.instance.PlaySound(SoundManager.instance.soundDestroyBuilding); 
                }

                Debug.Log($"Demolished {baseBuildingScript.buildingName} at {cell}. Refunded {refundPoints} points."); 
            }
            
            occupiedCellsAndBuildings.Remove(cell); // ลบออกจาก Dictionary
            Destroy(buildingToDemolish); // ทำลาย GameObject

            ClearHighlight(); // ลบ Highlight
        }
        else
        {
            Debug.Log($"No building found at {cell} to demolish."); 
        }
    }
}