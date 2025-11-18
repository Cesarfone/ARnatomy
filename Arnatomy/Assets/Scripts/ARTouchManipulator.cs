using UnityEngine;

public class ARTouchManipulator : MonoBehaviour
{
    [Header("Alvo (root do esqueleto no AR)")]
    public Transform targetRoot;

    [Header("Bloqueio quando overlay estiver ativo")]
    public Canvas infoCanvas;
    public Camera infoCamera;

    [Header("Rotação e Zoom")]
    public float rotSpeed = 0.2f;
    public float zoomSpeed = 0.005f;
    public float minScale = 0.2f;
    public float maxScale = 3.0f;

    private Vector2 _lastPos0, _lastPos1;

    void Update()
    {
        if (!targetRoot) return;
        if ((infoCanvas && infoCanvas.gameObject.activeSelf) ||
            (infoCamera && infoCamera.gameObject.activeSelf)) return;

        if (Input.touchCount == 1)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
            {
                Vector2 d = t.deltaPosition;
                var e = targetRoot.localEulerAngles;
                e.y += -d.x * rotSpeed;
                e.x +=  d.y * rotSpeed;
                targetRoot.localEulerAngles = e;
            }
        }
        else if (Input.touchCount >= 2)
        {
            var t0 = Input.GetTouch(0);
            var t1 = Input.GetTouch(1);
            if (t1.phase == TouchPhase.Began) { _lastPos0 = t0.position; _lastPos1 = t1.position; }
            float prev = (_lastPos0 - _lastPos1).magnitude;
            float curr = (t0.position - t1.position).magnitude;
            float diff = curr - prev;
            float s = Mathf.Clamp(targetRoot.localScale.x + diff * zoomSpeed, minScale, maxScale);
            targetRoot.localScale = Vector3.one * s;
            _lastPos0 = t0.position; _lastPos1 = t1.position;
        }
    }
}
