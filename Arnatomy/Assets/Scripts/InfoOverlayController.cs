using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoOverlayController : MonoBehaviour
{
    [Header("UI / Câmera")]
    public Camera infoCamera;         // InfoCamera (DESATIVADA no início)
    public Canvas canvasInfo;         // Canvas_Info (DESATIVADO no início)
    public TextMeshProUGUI titulo;    // Header_Titulo
    public TextMeshProUGUI descricao; // Text_Descricao
    public Button botaoVoltar;        // Botao_Voltar

    [Header("Prefab do Crânio (TELA)")]
    public GameObject prefabCranio;   // MESMO prefab usado no AR

    [Header("Viewer 3D")]
    public float distanciaDaCamera = 2.0f;   // distância em Z a partir da InfoCamera
    public float escalaInicial     = 1.0f;   // escala do crânio
    public Vector3 rotacaoInicial  = new Vector3(0, 180, 0);

    [Header("Posição na tela (viewport)")]
    [Range(0f, 1f)]
    public float viewportX = 0.5f;    // 0.5 = centro horizontal
    [Range(0f, 1f)]
    public float viewportY = 0.6f;    // >0.5 empurra pra cima

    [Header("Interação")]
    public float rotSpeedEditor = 5f;   // sensibilidade no Editor (mouse)
    public float rotSpeedTouch  = 0.2f; // sensibilidade no device (touch)

    [Header("Integração com Flashcards (opcional)")]
    public FlashcardManager flashcardManager; // usa card atual p/ título e descrição

    [Header("Layer do Viewer 3D")]
    public LayerMask info3DLayer;     // selecione APENAS a layer Info3D aqui

    private Transform _skullRoot;       // pivot no mundo
    private GameObject _skullInstance;
    private Vector3 _currentEuler;

    void Awake()
    {
        if (canvasInfo) canvasInfo.gameObject.SetActive(false);
        if (infoCamera) infoCamera.gameObject.SetActive(false);

        if (botaoVoltar != null)
            botaoVoltar.onClick.AddListener(Fechar);

        // cria um root vazio no mundo para ser o pivot do crânio
        GameObject go = new GameObject("SkullViewerRoot");
        _skullRoot = go.transform;
        _skullRoot.gameObject.SetActive(false); // começa escondido
    }

    public void Abrir()
    {
        if (infoCamera == null || canvasInfo == null || prefabCranio == null)
        {
            Debug.LogError("[InfoOverlayController] Falta referência (infoCamera, canvasInfo ou prefabCranio).");
            return;
        }

        infoCamera.gameObject.SetActive(true);
        canvasInfo.gameObject.SetActive(true);
        _skullRoot.gameObject.SetActive(true);

        // instancia o crânio 1 vez
        if (_skullInstance == null)
        {
            _skullInstance = Instantiate(prefabCranio, _skullRoot);
            _skullInstance.name = "Cranio_Viewer";

            _skullInstance.transform.localPosition = Vector3.zero;
            _skullInstance.transform.localRotation = Quaternion.identity;
            _skullInstance.transform.localScale    = Vector3.one * escalaInicial;

            CentralizarNoPivot(_skullInstance);

            int layer = LayerMaskToLayer(info3DLayer);
            if (layer < 0) layer = 0;
            SetLayerRecursively(_skullRoot.gameObject, layer);
        }

        _currentEuler = rotacaoInicial;
        _skullRoot.rotation = Quaternion.Euler(_currentEuler);

        // ===== TÍTULO E DESCRIÇÃO VINDO DO FLASHCARD MANAGER =====
        FlashcardData card = null;
        if (flashcardManager != null)
            card = flashcardManager.GetCardAtual();

        if (titulo != null)
        {
            if (card != null && !string.IsNullOrWhiteSpace(card.respostaTitulo))
                titulo.text = card.respostaTitulo;
            else
                titulo.text = "Crânio";
        }

        if (descricao != null)
        {
            string desc = null;

            if (card != null && !string.IsNullOrWhiteSpace(card.respostaDescricao))
                desc = card.respostaDescricao;

            if (string.IsNullOrWhiteSpace(desc))
                desc = "Visão 3D interativa do crânio.";

            descricao.text = desc;
        }
    }

    void CentralizarNoPivot(GameObject model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("[InfoOverlayController] Nenhum Renderer encontrado no prefab do crânio.");
            return;
        }

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        Vector3 worldCenter = b.center;
        Vector3 localCenter = _skullRoot.InverseTransformPoint(worldCenter);

        model.transform.localPosition -= localCenter;
    }

    public void Fechar()
    {
        if (canvasInfo) canvasInfo.gameObject.SetActive(false);
        if (infoCamera) infoCamera.gameObject.SetActive(false);
        if (_skullRoot) _skullRoot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!canvasInfo || !canvasInfo.gameObject.activeSelf) return;
        if (_skullRoot == null || _skullInstance == null || infoCamera == null) return;

        Vector3 targetViewport = new Vector3(viewportX, viewportY, distanciaDaCamera);
        Vector3 centerWorld = infoCamera.ViewportToWorldPoint(targetViewport);
        _skullRoot.position = centerWorld;

    #if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            _currentEuler.y += -dx * rotSpeedEditor;
            _currentEuler.x +=  dy * rotSpeedEditor;

            _skullRoot.rotation = Quaternion.Euler(_currentEuler);
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

                _skullRoot.rotation = Quaternion.Euler(_currentEuler);
            }
        }
    #endif
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }

    private int LayerMaskToLayer(LayerMask mask)
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
