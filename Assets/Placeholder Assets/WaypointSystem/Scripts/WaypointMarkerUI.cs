using UnityEngine;
using UnityEngine.UI; // Required for Image component
using TMPro; // Required for TextMeshProUGUI

namespace WrightAngle.Waypoint
{
    /// <summary>
    /// Controls the visual state of a single waypoint marker instance on the UI Canvas.
    /// Attach this script to your waypoint marker prefab. It handles positioning the marker
    /// correctly on-screen or clamping it to the screen edge as an off-screen indicator,
    /// including rotation to point towards the target. It also handles distance-based scaling and distance text.
    /// </summary>
    [AddComponentMenu("WrightAngle/Waypoint Marker UI")]
    [RequireComponent(typeof(RectTransform))]
    public class WaypointMarkerUI : MonoBehaviour
    {
        [Header("UI Element References")]
        [Tooltip("The core visual element of your marker (e.g., an arrow, dot, or custom icon). Must have an Image component.")]
        [SerializeField] private Image markerIcon;

        [Tooltip("Assign the TextMeshProUGUI element here for displaying distance. Optional.")]
        [SerializeField] private TextMeshProUGUI distanceTextElement;

        // Cached components for performance
        private RectTransform rectTransform;
        private Vector3 initialScale; // Store the initial scale of the marker from the prefab
        private Quaternion initialTextRotation = Quaternion.identity; // Store initial rotation if needed, but we'll force it upright.

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            initialScale = rectTransform.localScale; // Store the prefab's scale

            // Ensure the essential icon component is assigned
            if (markerIcon == null)
            {
                Debug.LogError($"<b>[{gameObject.name}] WaypointMarkerUI Error:</b> Marker Icon is not assigned in the Inspector. This is required for the marker to be visible.", this);
                // Do not disable the whole component if text might still be used, or if icon is optional.
                // For now, assume icon is essential for the core marker.
                // enabled = false;
            }
            else
            {
                // Optimize performance by disabling raycast target for the icon (markers are typically non-interactive)
                markerIcon.raycastTarget = false;
            }

            if (distanceTextElement != null)
            {
                distanceTextElement.raycastTarget = false;
                // We will enforce upright rotation in UpdateDisplay, so storing initial rotation might not be strictly necessary
                // unless you have a specific design for the text's default orientation within the prefab.
                // Forcing Quaternion.identity relative to canvas is usually desired for readability.
            }
        }

        /// <summary>
        /// Updates the marker's position, rotation, scale, and distance text based on the target's screen-space information and distance.
        /// Called frequently by the WaypointUIManager.
        /// </summary>
        /// <param name="screenPosition">Target's projected position on the screen (can be off-screen).</param>
        /// <param name="isOnScreen">Indicates if the target is currently within the camera's viewport.</param>
        /// <param name="isBehindCamera">Indicates if the target is located behind the camera.</param>
        /// <param name="cam">The reference camera used for calculations.</param>
        /// <param name="settings">The active WaypointSettings asset providing configuration.</param>
        /// <param name="distanceToTarget">The world-space distance from the camera to the waypoint target.</param>
        public void UpdateDisplay(Vector3 screenPosition, bool isOnScreen, bool isBehindCamera, Camera cam, WaypointSettings settings, float distanceToTarget)
        {
            // Safety checks for required components and settings
            if (settings == null || rectTransform == null || cam == null) // markerIcon can be null if not essential
            {
                if (gameObject.activeSelf) gameObject.SetActive(false); // Hide if setup is invalid
                return;
            }

            // Apply distance scaling if enabled (also handles icon visibility)
            bool isMarkerVisible = ApplyDistanceScaling(settings, distanceToTarget, isOnScreen);

            if (!isMarkerVisible && settings.MinScaleFactor == 0f && settings.EnableDistanceScaling) // If marker is scaled to invisible
            {
                if (distanceTextElement != null) distanceTextElement.gameObject.SetActive(false);
                if (markerIcon != null && !markerIcon.enabled) // if icon specifically was disabled by scaling
                {
                    // If markerIcon itself is disabled, and no text, we might hide the whole gameobject
                    // For now, let scaling handle icon, and text handle itself.
                }
                // If marker becomes invisible due to scaling, and it's configured to do so, hide everything.
                // However, MaxVisibleDistance check in UIManager should primarily handle complete hiding.
                // This is more about scaling to zero.
                // The current ApplyDistanceScaling already handles markerIcon.enabled.
            }


            if (isOnScreen)
            {
                // --- Target ON Screen ---
                rectTransform.position = screenPosition;
                rectTransform.rotation = Quaternion.identity; // Ensure parent (main marker) is upright on screen
                if (markerIcon != null && !markerIcon.gameObject.activeSelf && isMarkerVisible) markerIcon.gameObject.SetActive(true);
            }
            else // --- Target OFF Screen ---
            {
                if (!settings.UseOffScreenIndicators)
                {
                    if (markerIcon != null && markerIcon.gameObject.activeSelf) markerIcon.gameObject.SetActive(false);
                    if (distanceTextElement != null && distanceTextElement.gameObject.activeSelf) distanceTextElement.gameObject.SetActive(false);
                    return;
                }

                if (markerIcon != null && !markerIcon.gameObject.activeSelf && isMarkerVisible) markerIcon.gameObject.SetActive(true);


                float margin = settings.ScreenEdgeMargin;
                Vector2 screenCenter = new Vector2(cam.pixelWidth * 0.5f, cam.pixelHeight * 0.5f);
                Rect screenBounds = new Rect(margin, margin, cam.pixelWidth - (margin * 2f), cam.pixelHeight - (margin * 2f));
                Vector3 positionToClamp;
                Vector2 directionForRotation;

                if (isBehindCamera)
                {
                    Vector2 screenPos2D = new Vector2(screenPosition.x, screenPosition.y);
                    Vector2 directionFromCenter = screenPos2D - screenCenter;
                    directionFromCenter.x *= -1;
                    directionFromCenter.y = -Mathf.Abs(directionFromCenter.y);
                    if (directionFromCenter.sqrMagnitude < 0.001f) directionFromCenter = Vector2.down;
                    directionFromCenter.Normalize();
                    float farDistance = cam.pixelWidth + cam.pixelHeight;
                    positionToClamp = new Vector3(screenCenter.x + directionFromCenter.x * farDistance, screenCenter.y + directionFromCenter.y * farDistance, 0);
                    directionForRotation = directionFromCenter;
                }
                else
                {
                    positionToClamp = screenPosition;
                    directionForRotation = (new Vector2(screenPosition.x, screenPosition.y) - screenCenter).normalized;
                }

                Vector2 clampedPosition = IntersectWithScreenBounds(screenCenter, positionToClamp, screenBounds);
                rectTransform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0f);

                if (markerIcon != null && directionForRotation.sqrMagnitude > 0.001f)
                {
                    float angle = Vector2.SignedAngle(Vector2.right, directionForRotation);
                    float flipAngle = settings.FlipOffScreenMarkerY ? 180f : 0f;
                    // The main rectTransform is positioned, the icon within it can rotate.
                    // Or, if the main rectTransform rotates, text needs to counter-rotate.
                    // Let's assume the rectTransform (this component's GO) is what rotates for off-screen.
                    rectTransform.rotation = Quaternion.Euler(0, 0, angle + flipAngle - 90f);
                }
                else if (markerIcon != null) // Default rotation if direction is zero
                {
                    float flipAngle = settings.FlipOffScreenMarkerY ? 180f : 0f;
                    rectTransform.rotation = Quaternion.Euler(0, 0, -180f + flipAngle);
                }
            }

            // Update and manage distance text
            UpdateDistanceText(settings, distanceToTarget, isMarkerVisible);
        }

        /// <summary>
        /// Applies distance-based scaling to the RectTransform if enabled in settings.
        /// Returns true if the marker should be visible based on scale, false otherwise.
        /// </summary>
        private bool ApplyDistanceScaling(WaypointSettings settings, float distanceToTarget, bool isOnScreen)
        {
            float currentVisualScaleFactor = settings.DefaultScaleFactor; // Start with default

            if (settings.EnableDistanceScaling)
            {
                if (distanceToTarget <= settings.DistanceForDefaultScale)
                {
                    currentVisualScaleFactor = settings.DefaultScaleFactor;
                }
                else if (distanceToTarget >= settings.MaxScalingDistance)
                {
                    currentVisualScaleFactor = settings.MinScaleFactor;
                }
                else
                {
                    float t = (distanceToTarget - settings.DistanceForDefaultScale) / (settings.MaxScalingDistance - settings.DistanceForDefaultScale);
                    currentVisualScaleFactor = Mathf.Lerp(settings.DefaultScaleFactor, settings.MinScaleFactor, t);
                }
                rectTransform.localScale = initialScale * currentVisualScaleFactor;
            }
            else
            {
                rectTransform.localScale = initialScale * settings.DefaultScaleFactor; // Or just initialScale if DefaultScaleFactor is only for scaling mode
                                                                                       // Let's use DefaultScaleFactor as the "normal" size multiplier always.
                currentVisualScaleFactor = settings.DefaultScaleFactor; // What is considered "visible" scale
            }

            bool shouldBeVisible = currentVisualScaleFactor > 0.001f || (settings.EnableDistanceScaling && settings.MinScaleFactor > 0f) || !settings.EnableDistanceScaling;

            if (markerIcon != null)
            {
                markerIcon.enabled = shouldBeVisible;
            }
            return shouldBeVisible;
        }


        private void UpdateDistanceText(WaypointSettings settings, float distanceToTarget, bool isMarkerVisuallyScaledToShow)
        {
            if (distanceTextElement == null) return; // No text element assigned

            if (settings.DisplayDistanceText && isMarkerVisuallyScaledToShow)
            {
                distanceTextElement.gameObject.SetActive(true);

                string distanceString;
                string suffix;

                if (settings.UnitSystem == WaypointSettings.DistanceUnitSystem.Metric)
                {
                    if (distanceToTarget < WaypointSettings.METERS_PER_KILOMETER)
                    {
                        distanceString = distanceToTarget.ToString($"F{settings.DistanceDecimalPlaces}");
                        suffix = settings.SuffixMeters;
                    }
                    else
                    {
                        distanceString = (distanceToTarget / WaypointSettings.METERS_PER_KILOMETER).ToString($"F{settings.DistanceDecimalPlaces}");
                        suffix = settings.SuffixKilometers;
                    }
                }
                else // Imperial
                {
                    float distanceInFeet = distanceToTarget * WaypointSettings.FEET_PER_METER;
                    if (distanceInFeet < WaypointSettings.FEET_PER_MILE)
                    {
                        distanceString = distanceInFeet.ToString($"F{settings.DistanceDecimalPlaces}");
                        suffix = settings.SuffixFeet;
                    }
                    else
                    {
                        distanceString = (distanceInFeet / WaypointSettings.FEET_PER_MILE).ToString($"F{settings.DistanceDecimalPlaces}");
                        suffix = settings.SuffixMiles;
                    }
                }
                distanceTextElement.text = $"{distanceString}{suffix}";

                // Ensure text remains upright regardless of parent (rectTransform) rotation
                // This sets its world rotation to identity, making it screen-aligned.
                distanceTextElement.rectTransform.rotation = Quaternion.identity;
            }
            else
            {
                distanceTextElement.gameObject.SetActive(false);
            }
        }


        /// <summary>
        /// Calculates the exact intersection point of a line (from screen center towards a target point)
        /// with the edges of a rectangular boundary. Ensures accurate clamping to the screen edge.
        /// </summary>
        private Vector2 IntersectWithScreenBounds(Vector2 center, Vector2 targetPoint, Rect bounds)
        {
            Vector2 direction = (targetPoint - center).normalized;
            if (direction.sqrMagnitude < 0.0001f) return new Vector2(bounds.center.x, bounds.yMin);

            float tXMin = (direction.x != 0) ? (bounds.xMin - center.x) / direction.x : Mathf.Infinity;
            float tXMax = (direction.x != 0) ? (bounds.xMax - center.x) / direction.x : Mathf.Infinity;
            float tYMin = (direction.y != 0) ? (bounds.yMin - center.y) / direction.y : Mathf.Infinity;
            float tYMax = (direction.y != 0) ? (bounds.yMax - center.y) / direction.y : Mathf.Infinity;

            float minT = Mathf.Infinity;
            if (tXMin > 0 && center.y + tXMin * direction.y >= bounds.yMin && center.y + tXMin * direction.y <= bounds.yMax) minT = Mathf.Min(minT, tXMin);
            if (tXMax > 0 && center.y + tXMax * direction.y >= bounds.yMin && center.y + tXMax * direction.y <= bounds.yMax) minT = Mathf.Min(minT, tXMax);
            if (tYMin > 0 && center.x + tYMin * direction.x >= bounds.xMin && center.x + tYMin * direction.x <= bounds.xMax) minT = Mathf.Min(minT, tYMin);
            if (tYMax > 0 && center.x + tYMax * direction.x >= bounds.xMin && center.x + tYMax * direction.x <= bounds.xMax) minT = Mathf.Min(minT, tYMax);

            if (float.IsInfinity(minT))
            {
                // Debug.LogWarning("WaypointMarkerUI: Could not find screen bounds intersection. Using fallback clamping.", this);
                return new Vector2(Mathf.Clamp(targetPoint.x, bounds.xMin, bounds.xMax),
                                   Mathf.Clamp(targetPoint.y, bounds.yMin, bounds.yMax));
            }
            return center + direction * minT;
        }

    } // End Class
} // End Namespace