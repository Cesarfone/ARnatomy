using UnityEngine;

public class SkullUIRotator : MonoBehaviour
{
    [Header("Referência ao pivô do crânio")]
    public Transform skullPivot;

    [Header("Sensibilidade")]
    public float rotSpeedEditor = 5f;
    public float rotSpeedTouch  = 0.2f;

    private Vector3 _currentEuler;

    void Start()
    {
        if (skullPivot == null)
            skullPivot = transform;

        _currentEuler = skullPivot.rotation.eulerAngles;
    }

    void Update()
    {
        if (skullPivot == null) return;

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            _currentEuler.y += -dx * rotSpeedEditor;
            _currentEuler.x +=  dy * rotSpeedEditor;

            skullPivot.rotation = Quaternion.Euler(_currentEuler);
        }
#else
        if (Input.touchCount == 1)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
            {
                Vector2 d = t.deltaPosition;

                _currentEuler.y += -d.x * rotSpeedTouch;
                _currentEuler.x +=  d.y * rotSpeedTouch;

                skullPivot.rotation = Quaternion.Euler(_currentEuler);
            }
        }
#endif
    }
}
