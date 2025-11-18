using UnityEngine;
using UnityEngine.EventSystems;

public class ARBoneTapOpener : MonoBehaviour
{
    public Camera arCamera;
    public InfoOverlayController infoOverlay;
    public float rayDistance = 100f;
    public string requiredTag = "Bone_Skull";

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonUp(0))
        {
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
                return;
            TryHit(Input.mousePosition);
        }
#else
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            var touch = Input.GetTouch(0);
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;
            TryHit(touch.position);
        }
#endif
    }

    void TryHit(Vector3 screenPos)
    {
        if (!arCamera || infoOverlay == null) return;

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            if (hit.collider != null && hit.collider.CompareTag(requiredTag))
            {
                infoOverlay.Abrir();
            }
        }
    }
}
