using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways] // so gizmo math updates even in Edit mode
public class MinimapController : MonoBehaviour
{
    [Header("Scene / World")]
    [Tooltip("The player transform in the world")]
    public Transform player;

    [Tooltip("World-space min corner of this area (X = left, Z = bottom/south)")]
    public Vector2 worldMinXZ = new Vector2(-50f, -50f);

    [Tooltip("World-space max corner of this area (X = right, Z = top/north)")]
    public Vector2 worldMaxXZ = new Vector2(50f, 50f);

    [Header("UI References")]
    [Tooltip("The RectTransform of the masked minimap window (the viewport box on your Canvas).")]
    public RectTransform minimapViewport;

    [Tooltip("The RectTransform of the BIG map image (the full PNG). This gets moved around.")]
    public RectTransform mapImage;

    [Tooltip("The RectTransform of the player icon (the little arrow / dot). Should sit centered in the viewport.")]
    public RectTransform playerIcon;

    [Header("Mapping / Calibration")]
    [Tooltip("If true, we swap axes so world Z drives minimap X, and world X drives minimap Y.")]
    public bool swapAxes = false;

    [Tooltip("Flip the minimap horizontal axis (mirror left/right).")]
    public bool flipHorizontal = false;

    [Tooltip("Flip the minimap vertical axis (mirror up/down).")]
    public bool flipVertical = false;

    [Header("Player Icon Facing")]
    [Tooltip("Rotate the player icon to match player facing (Y rotation). Map stays north-up.")]
    public bool rotatePlayerIcon = true;

    [Tooltip("Extra rotation added to the icon so the arrow graphic visually points forward.")]
    public float iconRotationOffsetDeg = 0f;

    private void LateUpdate()
    {
        if (player == null || minimapViewport == null || mapImage == null)
            return;

        // --- 1. Get player world position ---
        Vector3 p = player.position;
        float worldX = p.x;
        float worldZ = p.z;

        // --- 2. Normalize both axes 0..1 in world bounds ---
        // tX_raw = % across X range
        float tX_raw = Mathf.InverseLerp(worldMinXZ.x, worldMaxXZ.x, worldX);
        // tZ_raw = % across Z range (worldMinXZ.y == minZ, worldMaxXZ.y == maxZ)
        float tZ_raw = Mathf.InverseLerp(worldMinXZ.y, worldMaxXZ.y, worldZ);

        // Decide which normalized axis maps to minimap horizontal/vertical.
        // Default (swapAxes=false):
        //   minimapU = tX_raw -> left/right
        //   minimapV = tZ_raw -> up/down
        // Swapped (swapAxes=true):
        //   minimapU = tZ_raw
        //   minimapV = tX_raw
        float minimapU = swapAxes ? tZ_raw : tX_raw;
        float minimapV = swapAxes ? tX_raw : tZ_raw;

        // Optional flips
        if (flipHorizontal) minimapU = 1f - minimapU;
        if (flipVertical) minimapV = 1f - minimapV;

        // --- 3. Convert normalized (0..1) to local coords in mapImage space ---
        // NOTE: mapImage pivot should be (0.5, 0.5)
        float mapWidth = mapImage.rect.width;
        float mapHeight = mapImage.rect.height;

        float localX = Mathf.Lerp(-mapWidth * 0.5f, mapWidth * 0.5f, minimapU);
        float localY = Mathf.Lerp(-mapHeight * 0.5f, mapHeight * 0.5f, minimapV);

        // --- 4. Center player in the viewport: move the map the opposite way ---
        Vector2 desiredAnchoredPos = new Vector2(-localX, -localY);

        // --- 5. Clamp so we don't scroll past edges (show black) ---
        float viewW = minimapViewport.rect.width;
        float viewH = minimapViewport.rect.height;

        float maxOffsetX = (mapWidth - viewW) * 0.5f;
        float maxOffsetY = (mapHeight - viewH) * 0.5f;

        maxOffsetX = Mathf.Max(0f, maxOffsetX);
        maxOffsetY = Mathf.Max(0f, maxOffsetY);

        float clampedX = Mathf.Clamp(desiredAnchoredPos.x, -maxOffsetX, maxOffsetX);
        float clampedY = Mathf.Clamp(desiredAnchoredPos.y, -maxOffsetY, maxOffsetY);

        mapImage.anchoredPosition = new Vector2(clampedX, clampedY);

        // --- 6. Rotate player icon to match facing (optional) ---
        if (playerIcon != null && rotatePlayerIcon)
        {
            float yaw = player.eulerAngles.y;
            playerIcon.localEulerAngles = new Vector3(
                0f,
                0f,
                -(yaw + iconRotationOffsetDeg)
            );
        }
    }

    // =====================================================
    // GIZMO VISUALIZER
    // =====================================================
    //
    // This draws:
    // 1. A wire box showing your worldMinXZ/worldMaxXZ bounds.
    // 2. A little sphere where the player is inside that box.
    //
    // You will see it in Scene view (not Game view).
    //
    private void OnDrawGizmos()
    {
        // if we don't have a player yet, still draw the box so you can tune
        // worldMinXZ/worldMaxXZ in edit mode.
        // We'll draw at some fixed Y height so it's visible above ground.

        // 1. Compute bounds center & size in world space using X/Z
        float minX = worldMinXZ.x;
        float maxX = worldMaxXZ.x;
        float minZ = worldMinXZ.y; // remember .y is actually Z
        float maxZ = worldMaxXZ.y;

        float centerX = (minX + maxX) * 0.5f;
        float centerZ = (minZ + maxZ) * 0.5f;
        float sizeX = Mathf.Abs(maxX - minX);
        float sizeZ = Mathf.Abs(maxZ - minZ);

        // pick a Y to draw the box. We'll try to sit near player height if possible.
        float drawY = 0f;
        if (player != null)
            drawY = player.position.y + 0.5f; // float a bit above player

        Vector3 boxCenter = new Vector3(centerX, drawY, centerZ);
        Vector3 boxSize = new Vector3(sizeX, 0.1f, sizeZ); // thin, flat rectangle

        // 2. Draw the world bounds as a green wire box
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(boxCenter, boxSize);

        // 3. Draw the player's position relative to that box in yellow
        if (player != null)
        {
            Vector3 p = player.position;
            Vector3 markerPos = new Vector3(p.x, drawY, p.z);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(markerPos, 0.5f);

            // also draw a line straight up so it's easy to see
            Gizmos.DrawLine(markerPos, markerPos + Vector3.up * 2f);
        }

        // 4. Also label corners so you understand which field is which:
        //    - worldMinXZ -> bottom-left
        //    - worldMaxXZ -> top-right
        Vector3 minCorner = new Vector3(minX, drawY, minZ);
        Vector3 maxCorner = new Vector3(maxX, drawY, maxZ);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(minCorner, 0.3f); // min corner
        Gizmos.DrawSphere(maxCorner, 0.3f); // max corner

        // optional debug lines between corners:
        Gizmos.DrawLine(minCorner, new Vector3(minX, drawY, maxZ)); // left edge
        Gizmos.DrawLine(minCorner, new Vector3(maxX, drawY, minZ)); // bottom edge
        Gizmos.DrawLine(maxCorner, new Vector3(minX, drawY, maxZ)); // top edge
        Gizmos.DrawLine(maxCorner, new Vector3(maxX, drawY, minZ)); // right edge
    }
}
