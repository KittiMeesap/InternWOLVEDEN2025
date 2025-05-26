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
    // private bool isCostAlreadyPaid = false; // <<< ลบตัวแปรนี้ออก ไม่จำเป็นแล้ว

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
                    // Disable Components ที่ไม่จำเป็นสำหรับ Visual เพื่อประสิทธิภาพ (เช่น Collider, Rigidbody)
                    DisableComponentsForVisual(currentBuildingVisualInstance);
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
                // ถ้าเป็น Building ที่ไม่ควรมี Visual (เช่น Remover/Unlocker), ให้ทำลาย Visual เก่าทิ้ง
                if (currentBuildingVisualInstance != null)
                {
                    Destroy(currentBuildingVisualInstance);
                    currentBuildingVisualInstance = null;
                }
            }

            // Highlight Cell ที่เมาส์อยู่
            if (currentCell != lastHoveredCell) // ถ้าเมาส์เปลี่ยน Cell
            {
                ClearHighlight(); // ลบ Highlight เก่า
                lastHoveredCell = currentCell; // อัปเดต Cell ใหม่
                HighlightCell(currentCell, IsCellOccupiedOrNoBuildZone(currentCell)); // Highlight Cell ใหม่
            }

            if (Input.GetMouseButtonDown(0)) // คลิกซ้ายเพื่อวาง/ใช้งาน
            {
                if (!isPointerOverUI) // ไม่ทำงานถ้าคลิกบน UI
                {
                    PlaceBuilding(targetPosition);
                }
            }

            if (Input.GetMouseButtonDown(1)) // คลิกขวาเพื่อยกเลิกโหมดสร้าง
            {
                // ยกเลิกโหมดสร้าง คืนเงิน (เพราะยกเลิกการเลือก)
                CancelCurrentBuilding(true); // <<< เปลี่ยนเป็น true เพื่อคืนเงินเมื่อยกเลิกการเลือก
            }
        }
        else // ถ้าไม่มี Building ที่เลือก (อยู่ในโหมดปกติ)
        {
            // เคลียร์ Highlight และ Visual ที่อาจค้างอยู่
            if (lastHoveredCell != Vector3Int.one * -1) 
            {
                ClearHighlight(); 
                lastHoveredCell = Vector3Int.one * -1; 
            }
            if (currentBuildingVisualInstance != null) 
            {
                Destroy(currentBuildingVisualInstance);
                currentBuildingVisualInstance = null;
            }

            // ตรวจจับการคลิกบน Clickable Tile
            if (Input.GetMouseButtonDown(0) && !isPointerOverUI) 
            {
                CheckForClickableTileWithRaycast();
            }
        }
    }

    // ช่วยปิด Component ที่ไม่จำเป็นสำหรับ visual preview
    private void DisableComponentsForVisual(GameObject visualInstance)
    {
        foreach (var collider in visualInstance.GetComponents<Collider2D>())
        {
            collider.enabled = false;
        }
        foreach (var rigidbody in visualInstance.GetComponents<Rigidbody2D>())
        {
            rigidbody.simulated = false;
        }
        // ถ้ามี Script อื่นๆ ที่ไม่ควรทำงานบน visual (เช่น Script ที่สร้างคะแนน), สามารถ Disable ได้ที่นี่
        BaseBuilding baseBuilding = visualInstance.GetComponent<BaseBuilding>();
        if (baseBuilding != null)
        {
            baseBuilding.enabled = false; // ปิด BaseBuilding script เพื่อไม่ให้มันทำงาน
        }
    }


    public bool CanInstantiateBuilding()
    {
        return currentBuildingPrefab == null; // ตรวจสอบว่าสามารถเลือก Building ได้หรือไม่
    }

    // ฟังก์ชันสำหรับเลือก Building ที่จะสร้าง (ถูกเรียกจาก UI)
    public void InstantiateBuilding(GameObject prefabToInstantiate, int costOfBuilding)
    {
        // ถ้ามี Building ที่เลือกอยู่แล้ว ให้ยกเลิกอันเก่า (และคืนเงิน เพราะกำลังจะเลือกใหม่)
        if (currentBuildingPrefab != null) 
        {
            CancelCurrentBuilding(true); // <<< เปลี่ยนเป็น true เพื่อคืนเงินเมื่อเลือก Building ใหม่
        }

        if (PointManager.instance == null)
        {
            Debug.LogError("PointManager instance is null! Cannot buy building.");
            return;
        }

        // ไม่หักเงินตรงนี้แล้ว! จะไปหักตอน PlaceBuilding()
        // PointManager.instance.AddPoints(-currentBuildingCost); // <<< ลบออก
        // Debug.Log($"Points deducted for first item: {currentBuildingCost}. Remaining points: {PointManager.instance.points}"); // <<< ลบออก

        Debug.Log($"[BuildManager] Selected: {prefabToInstantiate.name} with cost: {costOfBuilding}. Current points: {PointManager.instance.points}");
        
        // กำหนด Building ที่เลือก
        currentBuildingPrefab = prefabToInstantiate; 
        currentBuildingCost = costOfBuilding; 
        // isCostAlreadyPaid = true; // <<< ลบตัวแปรนี้ออก

        // สร้าง Visual Preview (ถ้าจำเป็น)
        bool isBuildingThatShouldHaveVisual = 
            currentBuildingPrefab.GetComponent<UnlockBlackGrindBuilding>() == null && 
            currentBuildingPrefab.GetComponent<RemoverBuilding>() == null;

        if (isBuildingThatShouldHaveVisual)
        {
            currentBuildingVisualInstance = Instantiate(currentBuildingPrefab);
            DisableComponentsForVisual(currentBuildingVisualInstance); // ปิด Components สำหรับ Visual
            currentBuildingVisualInstance.SetActive(false); // ซ่อนไว้ก่อน
            currentBuildingVisualInstance.transform.position = Vector3.zero; 
        }
          
        UpgradePanelManager.instance.CloseUpgradePanel(); // ปิด Panel UI
        backTilemap.SetActive(true); // เปิด Background Tilemap (ถ้ามี)
        
        lastHoveredCell = Vector3Int.one * -1; // รีเซ็ต Highlight
        ClearAllHighlights();
        Debug.Log($"[BuildManager] Entered build mode for: {currentBuildingPrefab.name}");
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // แปลงตำแหน่งเมาส์เป็น World Space และ Cell Center ของ Tilemap
    public Vector3 GetTargetPosition()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3Int currentCell = groundTilemap.WorldToCell(mousePos);
        return groundTilemap.GetCellCenterWorld(currentCell);
    }

    // ฟังก์ชันสำหรับวาง Building จริงๆ หรือใช้งาน Building พิเศษ
 // BuildManager.cs (เฉพาะเมธอด PlaceBuilding)

    // ฟังก์ชันสำหรับวาง Building จริงๆ หรือใช้งาน Building พิเศษ
    private void PlaceBuilding(Vector3 position)
    {
        if (currentBuildingPrefab == null) return; // ไม่มี Building ที่เลือกอยู่ ก็ไม่ทำอะไร

        Vector3Int cellToPlace = groundTilemap.WorldToCell(position);

        // ตรวจสอบว่าเป็น Building ประเภทพิเศษ (Unlocker, Remover) หรือไม่
        bool isCurrentBuildingUnlocker = (currentBuildingPrefab.GetComponent<UnlockBlackGrindBuilding>() != null);
        bool isCurrentBuildingRemover = (currentBuildingPrefab.GetComponent<RemoverBuilding>() != null);
        bool isSpecialBuilding = isCurrentBuildingUnlocker || isCurrentBuildingRemover;

        bool canPlace = true; // ตั้งค่าเริ่มต้นให้สามารถใช้งาน/วางได้
        if (isSpecialBuilding)
        {
            if (isCurrentBuildingRemover)
            {
                // Remover วางได้ถ้ามี Building อยู่ใน Cell นั้นๆ
                canPlace = occupiedCellsAndBuildings.ContainsKey(cellToPlace); 
            }
            else if (isCurrentBuildingUnlocker)
            {
                TileBase tileAtGroundMap = groundTilemap?.GetTile(cellToPlace);
                // Unlocker วางได้ถ้าเป็น Black Grind Tile และยังไม่เคยปลดล็อคใน HashSet
                canPlace = (tileAtGroundMap != null && tileAtGroundMap == blackGrindTilebase && !unlockedCells.Contains(cellToPlace));
            }

            if (!canPlace) // ถ้าใช้งานไม่ได้ (เช่น Remover ไม่มี Building ให้ลบ, Unlocker ไม่ใช่ Black Grind)
            {
                Debug.Log($"[BuildManager] Cannot use special building ({currentBuildingPrefab.name}) here. Condition not met. Try another spot.");
                // ไม่ต้องทำอะไรต่อ แค่ไม่ดำเนินการ ไม่มีการคืนเงิน ไม่มีการออกจากโหมด
                return; // ออกจากฟังก์ชัน ไม่ต้องทำอะไรต่อ
            }
        }
        else // Building ทั่วไป (ไม่ใช่ Unlocker หรือ Remover)
        {
            // วาง Building ทั่วไปได้ถ้า Cell ว่างและไม่ใช่ No Build Zone หรือ Clickable Point Tile
            canPlace = !IsCellOccupiedOrNoBuildZone(cellToPlace); 
            if (!canPlace) 
            {
                Debug.Log($"[BuildManager] Cannot place regular building ({currentBuildingPrefab.name}) here. Cell occupied or no-build zone. Exiting build mode.");
                // ถ้าวาง Building ปกติไม่ได้ ให้ยกเลิกโหมดสร้าง (ไม่ต้องคืนเงิน เพราะเงินยังไม่ถูกหัก)
                CancelCurrentBuilding(false); 
                return; 
            }
        }
        
        // *** หักเงินตรงนี้! เมื่อยืนยันว่าสามารถวาง/ใช้งานได้แล้วเท่านั้น ***
        if (PointManager.instance.points < currentBuildingCost)
        {
            Debug.Log($"[BuildManager] Not enough points to {currentBuildingPrefab.name} (cost: {currentBuildingCost}). Current points: {PointManager.instance.points}. Exiting build mode.");
            // ไม่พอจ่ายก็ออกจากโหมดสร้าง
            CancelCurrentBuilding(false); 
            return;
        }
        PointManager.instance.AddPoints(-currentBuildingCost); // หักเงิน!
        Debug.Log($"[BuildManager] Points deducted for {currentBuildingPrefab.name}: {currentBuildingCost}. Remaining points: {PointManager.instance.points}");


        // *** ถ้ามาถึงตรงนี้ แสดงว่าสามารถวาง/ใช้งานได้แล้ว และจ่ายเงินแล้ว ***
        GameObject actualBuildingInstance = Instantiate(currentBuildingPrefab, position, Quaternion.identity);
        actualBuildingInstance.SetActive(true); 

        // ตรวจสอบ Script ของ Building ที่เพิ่งวางไป
        BaseBuilding baseBuildingScript = actualBuildingInstance.GetComponent<BaseBuilding>();
        if (baseBuildingScript != null)
        {
            PassivePointBuilding passiveBuilding = baseBuildingScript as PassivePointBuilding;
            if (passiveBuilding != null) 
            {
                int appliedMultiplier = 1; 
                // ตรวจสอบ Bonus Tilemap สำหรับ Passive Building
                if (bonusTilemap != null && passiveBonusTile != null)
                {
                    TileBase tileAtBonusMap = bonusTilemap.GetTile(cellToPlace);
                    Debug.Log($"[BuildManager] Checking bonus at cell {cellToPlace}. Tile on BonusTilemap: {tileAtBonusMap?.name ?? "None"}. Expected Passive Bonus Tile: {passiveBonusTile?.name ?? "None"}");

                    if (tileAtBonusMap != null && tileAtBonusMap == passiveBonusTile)
                    {
                        appliedMultiplier = 2; // ถ้ามี Bonus Tile ให้คูณ 2
                        Debug.Log($"[BuildManager] !!! BONUS DETECTED !!! Applied x2 bonus to Passive Building at {cellToPlace}");
                    }
                    else
                    {
                        Debug.Log($"[BuildManager] No bonus tile found at {cellToPlace} or tile mismatch. Applying x1 multiplier.");
                    }
                }
                else
                {
                    Debug.Log("[BuildManager] Bonus Tilemap or Passive Bonus Tile not set in BuildManager Inspector. No bonus will be applied.");
                }
                
                // (A) สำคัญ: เรียก ApplyBonusMultiplier ก่อน StartBuilding 
                // เพื่อให้ค่า CurrentPointsPerInterval ถูกตั้งค่าก่อนที่จะถูก Register
                passiveBuilding.ApplyBonusMultiplier(appliedMultiplier); 
            }

            // (B) เรียก StartBuilding หลังจาก ApplyBonusMultiplier (ถ้าเป็น PassiveBuilding) หรือทันที (สำหรับ Building อื่นๆ)
            // StartBuilding จะลงทะเบียน Building กับ PointManager และสั่ง Recalculate (สำหรับ PassiveBuilding)
            baseBuildingScript.StartBuilding(); 
        }

        // --- จัดการ Building พิเศษหลังการใช้งานสำเร็จ ---
        if (isCurrentBuildingUnlocker)
        {
            ProcessUnlockBuilding(cellToPlace, actualBuildingInstance); // ปลดล็อค Tile
            Debug.Log($"[BuildManager] Unlocker used successfully. Remaining in unlock mode.");
            // ทำความสะอาด Highlight (สำคัญเพื่อให้ Highlight เปลี่ยนเมื่อเมาส์เคลื่อนที่)
            lastHoveredCell = Vector3Int.one * -1;
            ClearAllHighlights();
            return; // ออกจากฟังก์ชัน เพื่อให้ยังคงอยู่ในโหมด Unlocker
        }
        else if (isCurrentBuildingRemover)
        {
            DemolishBuildingAtCell(cellToPlace, 50); // รื้อถอน Building (คืนเงิน 50% สมมติ)
            Destroy(actualBuildingInstance); // ทำลาย Remover instance ที่สร้างขึ้นมา (ซึ่งไม่มี visual อยู่แล้ว)
            Debug.Log($"[BuildManager] Remover used successfully. Remaining in remove mode.");
            // ทำความสะอาด Highlight
            lastHoveredCell = Vector3Int.one * -1;
            ClearAllHighlights();
            return; // ออกจากฟังก์ชัน เพื่อให้ยังคงอยู่ในโหมด Remover
        }
        
        // --- สำหรับ Building ทั่วไป (ที่ไม่ใช่ Unlocker หรือ Remover) ---
        // เพิ่ม Building ลงใน Dictionary ของ Cell ที่ถูกยึดครอง
        occupiedCellsAndBuildings.Add(cellToPlace, actualBuildingInstance); 
        Debug.Log($"[BuildManager] Regular building placed successfully. Remaining in build mode for {currentBuildingPrefab.name}.");
        
        // ไม่มีการรีเซ็ต currentBuildingPrefab = null; ตรงนี้แล้ว
        // ไม่มีการเรียก UpgradePanelManager.instance.OpenUpgradePanel();
        // ไม่มีการเรียก backTilemap.SetActive(false);
        
        // ทำความสะอาด Highlight
        lastHoveredCell = Vector3Int.one * -1;
        ClearAllHighlights();
        // ไม่มี return; ตรงนี้แล้ว เพื่อให้ Building ทั่วไปยังสามารถวางตัวต่อไปได้
        // การออกจากโหมดสร้างจะเกิดขึ้นเมื่อผู้เล่นคลิกขวาเท่านั้น
    }
    // ตรวจสอบว่า Cell ถูกยึดครองหรืออยู่ใน No Build Zone หรือไม่
    private bool IsCellOccupiedOrNoBuildZone(Vector3Int cell)
    {
        if (occupiedCellsAndBuildings.ContainsKey(cell)) 
        {
            return true; // มี Building อื่นวางอยู่แล้ว
        }

        // ตรวจสอบประเภทของ Building ที่กำลังเลือก
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
            // Clickable Point Tile วาง Building ปกติไม่ได้ (ยกเว้น Remover)
            if (tileAtGroundMap == clickablePointTile && !isCurrentBuildingRemover) 
            {
                return true; 
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
            Destroy(unlockerGameObject, 0.01f); // ทำลาย Unlocker instance ที่สร้างขึ้นมา
        }
        // ไม่ต้องมี logic ในการรีเซ็ต currentBuildingPrefab ที่นี่
        // เพราะ PlaceBuilding จะจัดการให้
    }

    // ฟังก์ชันสำหรับยกเลิกโหมดสร้าง
    // refundIfCostPaid: true = คืนเงิน, false = ไม่คืนเงิน
    private void CancelCurrentBuilding(bool refundIfCostPaid)
    {
       
        if (refundIfCostPaid && currentBuildingPrefab != null && currentBuildingCost > 0) 
        {
        
            Debug.Log($"[BuildManager] Points refunded for cancelling: {currentBuildingCost}. Remaining points: {PointManager.instance.points} (Note: Refund occurs if selected and cancelled before placing).");
        } else if (!refundIfCostPaid) { // กรณีคลิกขวาออกจากโหมด
            Debug.Log("[BuildManager] Build mode cancelled. No refund needed for normal cancellation.");
        }
        
        // รีเซ็ตตัวแปรสถานะ
        currentBuildingCost = 0; 
        // isCostAlreadyPaid = false; // <<< ลบออก

        // ทำลาย Visual Preview (ถ้ามี)
        if (currentBuildingVisualInstance != null) 
        {
            Destroy(currentBuildingVisualInstance); 
            currentBuildingVisualInstance = null; 
        }

        // รีเซ็ต UI และสถานะ
        UpgradePanelManager.instance.OpenUpgradePanel(); // เปิด Panel คืน
        backTilemap.SetActive(false); // ปิด Background Tilemap
        currentBuildingPrefab = null; // รีเซ็ต Building ที่เลือก
        lastHoveredCell = Vector3Int.one * -1; // รีเซ็ต Highlight
        ClearAllHighlights();
        Debug.Log("[BuildManager] Exited build mode.");
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

                // เรียก StopBuilding เพื่อ Unregister จาก PointManager (ถ้าเป็น Passive)
                baseBuildingScript.StopBuilding(); 
                
                if (SoundManager.instance != null && SoundManager.instance.soundDestroyBuilding != null)
                {
                    SoundManager.instance.PlaySound(SoundManager.instance.soundDestroyBuilding); 
                }

                Debug.Log($"[BuildManager] Demolished {baseBuildingScript.buildingName} at {cell}. Refunded {refundPoints} points."); 
            }
            
            occupiedCellsAndBuildings.Remove(cell); // ลบออกจาก Dictionary
            Destroy(buildingToDemolish); // ทำลาย GameObject ที่ถูกรื้อถอน

            ClearHighlight(); // ลบ Highlight
        }
        else
        {
            Debug.Log($"[BuildManager] No building found at {cell} to demolish."); 
        }
        // ไม่ต้องมี logic ในการรีเซ็ต currentBuildingPrefab ที่นี่
        // เพราะ PlaceBuilding จะจัดการให้ (ถ้าถูกเรียกผ่าน Remover)
    }
}