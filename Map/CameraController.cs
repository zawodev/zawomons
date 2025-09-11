using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float panSpeed = 2f;
    public float panSmoothTime = 0.3f;
    public bool invertPan = true;
    
    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoom = 3f;
    public float maxZoom = 20f;
    public float zoomSmoothTime = 0.2f;
    
    [Header("Map Boundaries")]
    public bool useBoundaries = true;
    public Vector2 mapMinBounds = new Vector2(-10, -10);
    public Vector2 mapMaxBounds = new Vector2(10, 10);
    public float boundaryPadding = 1f;
    
    [Header("Input Settings")]
    public KeyCode panKey = KeyCode.Mouse0;
    public bool allowMiddleMousePan = true;
    public LayerMask tileLayerMask = -1;
    
    // Private variables
    private Camera cam;
    private Vector3 lastMousePosition;
    private Vector3 targetPosition;
    private float targetZoom;
    private Vector3 panVelocity;
    private float zoomVelocity;
    private bool isPanning = false;
    private bool isDragging = false;
    private float dragThreshold = 5f; // Minimum distance to consider it a drag
    private Vector3 mouseDownPosition;
    
    // Events
    public System.Action<Vector3> OnCameraMoved;
    public System.Action<float> OnCameraZoomed;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
    }
    
    private void Start()
    {
        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;
    }
    
    private void Update()
    {
        HandleInput();
        UpdateCameraMovement();
        UpdateCameraZoom();
    }
    
    private void HandleInput()
    {
        // Don't handle input if pointer is over UI
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        // Handle zoom
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0)
        {
            ZoomTowardsMouse(-scrollWheel * zoomSpeed);
        }
        
        // Handle panning
        bool panKeyPressed = Input.GetKey(panKey) || (allowMiddleMousePan && Input.GetKey(KeyCode.Mouse2));
        
        if (panKeyPressed)
        {
            if (Input.GetKeyDown(panKey) || Input.GetKeyDown(KeyCode.Mouse2))
            {
                mouseDownPosition = Input.mousePosition;
                lastMousePosition = Input.mousePosition; // Zmień na pozycję myszki w pikselach
                isPanning = true;
                isDragging = false;
            }
            
            if (isPanning)
            {
                // Check if we've dragged enough to consider it a drag operation
                if (!isDragging)
                {
                    float dragDistance = Vector3.Distance(Input.mousePosition, mouseDownPosition);
                    if (dragDistance > dragThreshold)
                    {
                        isDragging = true;
                    }
                }
                
                if (isDragging)
                {
                    // Oblicz różnicę w pozycji myszki
                    Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
                    lastMousePosition = Input.mousePosition;
                    
                    // Przelicz na jednostki świata (dostosuj współczynnik do zoom)
                    float worldUnitsPerPixel = cam.orthographicSize * 2f / cam.pixelHeight;
                    Vector3 worldDelta = new Vector3(mouseDelta.x, mouseDelta.y, 0) * worldUnitsPerPixel;
                    
                    if (invertPan)
                    {
                        worldDelta = -worldDelta;
                    }
                    
                    // For 2D top-down, only use X and Y
                    Pan(worldDelta);
                }
            }
        }
        else if (isPanning)
        {
            isPanning = false;
            isDragging = false;
        }
    }
    
    private void Pan(Vector3 delta)
    {
        targetPosition += new Vector3(delta.x, delta.y, 0); // Keep Z unchanged for 2D
        
        if (useBoundaries)
        {
            targetPosition = ClampToBoundaries(targetPosition);
        }
        
        OnCameraMoved?.Invoke(targetPosition);
    }
    
    private void Zoom(float zoomAmount)
    {
        targetZoom = Mathf.Clamp(targetZoom + zoomAmount, minZoom, maxZoom);
        OnCameraZoomed?.Invoke(targetZoom);
    }
    
    private void ZoomTowardsMouse(float zoomAmount)
    {
        // Get mouse position in world coordinates before zoom
        Vector3 mouseWorldPosBefore = GetMouseWorldPosition();
        
        // Calculate new zoom level
        float newZoom = Mathf.Clamp(targetZoom + zoomAmount, minZoom, maxZoom);
        
        // Only proceed if zoom actually changed
        if (Mathf.Abs(newZoom - targetZoom) < 0.001f)
            return;
            
        float zoomRatio = newZoom / targetZoom;
        targetZoom = newZoom;
        
        // Get mouse position in world coordinates after zoom change
        // We need to calculate this manually since the camera hasn't moved yet
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(cam.transform.position.z);
        
        // Calculate what the mouse world position would be after zoom
        float newCameraHeight = newZoom;
        float newCameraWidth = newCameraHeight * cam.aspect;
        
        // Convert screen position to world position with new zoom
        Vector3 normalizedScreenPos = new Vector3(
            (mouseScreenPos.x / cam.pixelWidth) - 0.5f,
            (mouseScreenPos.y / cam.pixelHeight) - 0.5f,
            0
        );
        
        Vector3 mouseWorldPosAfter = targetPosition + new Vector3(
            normalizedScreenPos.x * newCameraWidth * 2f,
            normalizedScreenPos.y * newCameraHeight * 2f,
            0
        );
        
        // Calculate the offset and adjust camera position
        Vector3 offset = mouseWorldPosBefore - mouseWorldPosAfter;
        targetPosition += new Vector3(offset.x, offset.y, 0); // Keep Z unchanged
        
        // Clamp to boundaries if enabled
        if (useBoundaries)
        {
            targetPosition = ClampToBoundaries(targetPosition);
        }
        
        OnCameraZoomed?.Invoke(targetZoom);
    }
    
    private void UpdateCameraMovement()
    {
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref panVelocity, panSmoothTime);
        }
    }
    
    private void UpdateCameraZoom()
    {
        if (Mathf.Abs(cam.orthographicSize - targetZoom) > 0.01f)
        {
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
        }
    }
    
    private Vector3 ClampToBoundaries(Vector3 position)
    {
        // Calculate current camera bounds based on zoom
        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;
        
        // Apply padding
        float minX = mapMinBounds.x + cameraWidth + boundaryPadding;
        float maxX = mapMaxBounds.x - cameraWidth - boundaryPadding;
        float minZ = mapMinBounds.y + cameraHeight + boundaryPadding;
        float maxZ = mapMaxBounds.y - cameraHeight - boundaryPadding;
        
        // Preserve the original Z position
        float originalZ = position.z;
        
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minZ, maxZ); // Use Y for 2D top-down
        position.z = originalZ; // Keep Z unchanged
        
        return position;
    }
    
    public void SetMapBoundaries(Vector2 minBounds, Vector2 maxBounds)
    {
        mapMinBounds = minBounds;
        mapMaxBounds = maxBounds;
        
        // Update current position to respect new boundaries
        if (useBoundaries)
        {
            targetPosition = ClampToBoundaries(targetPosition);
        }
    }
    
    public void FocusOnPosition(Vector3 worldPosition, float zoomLevel = -1)
    {
        targetPosition = new Vector3(worldPosition.x, worldPosition.y, targetPosition.z); // Keep camera Z position
        
        if (zoomLevel > 0)
        {
            targetZoom = Mathf.Clamp(zoomLevel, minZoom, maxZoom);
        }
        
        if (useBoundaries)
        {
            targetPosition = ClampToBoundaries(targetPosition);
        }
    }
    
    public void FocusOnTile(Vector2Int gridPosition, MapSystem mapSystem, float zoomLevel = -1)
    {
        if (mapSystem != null)
        {
            Vector3 worldPos = HexMath.HexToWorldPosition(gridPosition, mapSystem.hexSize, mapSystem.pointyTop);
            FocusOnPosition(worldPos, zoomLevel);
        }
    }
    
    public bool IsDragging()
    {
        return isDragging;
    }
    
    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(cam.transform.position.z); // Distance from camera for 2D
        return cam.ScreenToWorldPoint(mouseScreenPos);
    }
    
    public HexTile GetTileUnderMouse()
    {
        Debug.Log("GetTileUnderMouse called");
        
        // Don't detect tiles if pointer is over UI
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            Debug.Log("EventSystem.current is null");
        }
        else
        {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            Debug.Log("IsPointerOverGameObject: " + isOverUI);
            
            if (isOverUI)
            {
                Debug.Log("Pointer is over UI, not detecting tiles.");
                return null;
            }
        }
        
        if (isDragging) return null; // Don't detect tiles while dragging
        
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayerMask))
        {
            return hit.collider.GetComponent<HexTile>();
        }
        
        return null;
    }
}
