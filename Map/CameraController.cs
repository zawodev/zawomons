using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float panSpeed = 2f;
    public float panSmoothTime = 0.3f;
    public bool invertPan = false;
    
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
    private float dragThreshold = 10f; // Pixel distance to consider it a drag (zwiększono z 5)
    private Vector3 mouseDownPosition;
    private Vector3 mouseDownWorldPosition;
    private MapSystem mapSystem;
    
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
        
        // Znajdź MapSystem w scenie
        mapSystem = FindFirstObjectByType<MapSystem>();
    }
    
    private void Update()
    {
        HandleInput();
        UpdateCameraMovement();
        UpdateCameraZoom();
    }
    
    private void HandleInput()
    {
        // Handle zoom
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0)
        {
            Zoom(-scrollWheel * zoomSpeed);
        }
        
        // Handle panning
        bool panKeyPressed = Input.GetKey(panKey) || (allowMiddleMousePan && Input.GetKey(KeyCode.Mouse2));
        
        if (panKeyPressed)
        {
            if (Input.GetKeyDown(panKey) || Input.GetKeyDown(KeyCode.Mouse2))
            {
                mouseDownPosition = Input.mousePosition;
                mouseDownWorldPosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
                lastMousePosition = mouseDownWorldPosition;
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
                    // Smooth drag - kursor zostaje na tym samym miejscu w świecie gry
                    Vector3 currentMouseWorldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));
                    Vector3 worldDelta = mouseDownWorldPosition - currentMouseWorldPos;
                    
                    // Dla 2D top-down używamy tylko X i Y
                    Pan(new Vector3(worldDelta.x, worldDelta.y, 0));
                    
                    // Zaktualizuj pozycję referencyjną żeby ruch był płynny
                    mouseDownWorldPosition = currentMouseWorldPos + worldDelta;
                }
            }
        }
        else if (isPanning)
        {
            // Po puszczeniu LPM sprawdź czy to był click czy drag
            if (!isDragging)
            {
                // To był click, nie drag - sprawdź czy kliknęliśmy kafelek
                HexTile clickedTile = GetTileUnderMouse();
                if (clickedTile != null && mapSystem != null)
                {
                    // Wywołaj kliknięcie kafelka bezpośrednio
                    clickedTile.OnTileClicked?.Invoke(clickedTile);
                }
            }
            
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
        
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minZ, maxZ); // Use Y for 2D top-down
        
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
        targetPosition = new Vector3(worldPosition.x, worldPosition.y, transform.position.z); // Keep camera Z position
        
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
