using UnityEngine;

public class FlashcardSkullViewer : MonoBehaviour
{
    [Header("Camera 3D (mesma da tela de info)")]
    public Camera infoCamera;          // a InfoCamera que já existe no projeto

    [Header("Prefab do Crânio (mesmo do AR)")]
    public GameObject prefabCranio;

    [Header("Layer 3D do viewer")]
    public LayerMask info3DLayer;      // SÓ a layer Info3D marcada

    [Header("Posição na tela (viewport)")]
    [Range(0f, 1f)]
    public float viewportX = 0.5f;     // 0.5 = centro horizontal
    [Range(0f, 1f)]
    public float viewportY = 0.55f;    // levemente acima do centro

    [Header("Distância e rotação")]
    public float distanciaDaCamera = 2.0f;
    public Vector3 rotacaoInicial = new Vector3(0, 180, 0);

    [Header("Interação (rotação)")]
    public float rotSpeedEditor = 5f;
    public float rotSpeedTouch  = 0.2f;

    private Transform _skullRoot;      // pivot no mundo
    private GameObject _skullInstance;
    private Vector3 _currentEuler;

    void OnEnable()
    {
        if (infoCamera == null || prefabCranio == null)
        {
            Debug.LogError("[FlashcardSkullViewer] Falta infoCamera ou prefabCranio.");
            return;
        }

        // Garante que a camera esteja ligada quando entrar na tela de flashcards
        if (!infoCamera.gameObject.activeSelf)
            infoCamera.gameObject.SetActive(true);

        // Cria o pivot se ainda não existir
        if (_skullRoot == null)
        {
            GameObject go = new GameObject("SkullViewer_Flashcards_Root");
            _skullRoot = go.transform;
        }

        // Instancia o crânio 1x
        if (_skullInstance == null)
        {
            _skullInstance = Instantiate(prefabCranio, _skullRoot);
            _skullInstance.name = "Cranio_Flashcards";

            _skullInstance.transform.localPosition = Vector3.zero;
            _skullInstance.transform.localRotation = Quaternion.identity;
            _skullInstance.transform.localScale    = Vector3.one;

            CentralizarNoPivot(_skullInstance);

            int layer = LayerMaskToLayer(info3DLayer);
            if (layer < 0) layer = 0;
            SetLayerRecursively(_skullRoot.gameObject, layer);
        }

        _skullRoot.gameObject.SetActive(true);
        _currentEuler = rotacaoInicial;
        _skullRoot.rotation = Quaternion.Euler(_currentEuler);
    }

    void OnDisable()
    {
        if (_skullRoot != null)
            _skullRoot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_skullRoot == null || infoCamera == null) return;
        if (!_skullRoot.gameObject.activeSelf) return;

        // Posiciona o pivot no mundo baseado na posição da viewport
        Vector3 vp = new Vector3(viewportX, viewportY, distanciaDaCamera);
        Vector3 worldPos = infoCamera.ViewportToWorldPoint(vp);
        _skullRoot.position = worldPos;

#if UNITY_EDITOR
        // Rotação com mouse no Editor
        if (Input.GetMouseButton(0))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            _currentEuler.y += -dx * rotSpeedEditor;
            _currentEuler.x +=  dy * rotSpeedEditor;

            _skullRoot.rotation = Quaternion.Euler(_currentEuler);
        }
#else
        // Rotação por touch no device
        if (Input.touchCount == 1)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
            {
                Vector2 d = t.deltaPosition;

                _currentEuler.y += -d.x * rotSpeedTouch;
                _currentEuler.x +=  d.y * rotSpeedTouch;

                _skullRoot.rotation = Quaternion.Euler(_currentEuler);
            }
        }
#endif
    }

    void CentralizarNoPivot(GameObject model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("[FlashcardSkullViewer] Nenhum Renderer no prefab do crânio.");
            return;
        }

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        Vector3 worldCenter = b.center;
        Vector3 localCenter = _skullRoot.InverseTransformPoint(worldCenter);

        model.transform.localPosition -= localCenter;
    }

    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }

    int LayerMaskToLayer(LayerMask mask)
    {
        int bitMask = mask.value;
        for (int i = 0; i < 32; i++)
        {
            if ((bitMask & (1 << i)) != 0)
                return i;
        }
        return -1;
    }
}
