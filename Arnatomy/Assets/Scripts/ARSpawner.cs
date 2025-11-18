using UnityEngine;
using Vuforia;

public class ARSpawner : MonoBehaviour
{
    [Header("Refs")]
    public ObserverBehaviour imageTarget;               // arraste o ImageTarget
    public GameObject prefabEsqueletoSemCranio;         // prefab do esqueleto sem crânio
    public GameObject prefabCranio;                     // prefab do crânio
    public Transform headSocketInSkeleton;              // opcional: socket/manual; se vazio, script tenta achar
    public LayerMask arLayer;                           // LayerMask ARContent

    [Header("Spawn do Esqueleto")]
    public bool spawnDeitado = false;
    public Vector3 offsetSpawn = Vector3.zero;
    public Vector3 rotEmPe = new Vector3(0, 0, 0);
    public Vector3 rotDeitado = new Vector3(90, 0, 0);
    public float escalaInicial = 1f;                    // ESCALA GLOBAL DO ESQUELETO + CRÂNIO

    [Header("Alinhamento do Crânio")]
    [Tooltip("Se true, usa o alinhamento 'autorado' dos prefabs em world, depois reparenta pro socket mantendo posição/rotação.")]
    public bool preserveAuthoredWorldAlignment = true;

    [Tooltip("Nomes candidatos do socket/cabeça dentro do esqueleto.")]
    public string[] headSocketNames = new[]
    {
        "Head", "Bip001 Head", "mixamorig:Head", "Armature/Head", "Hips/Spine/Chest/Neck/Head"
    };

    [Header("Ajuste fino (apenas se NÃO usar preserveAuthoredWorldAlignment)")]
    public Vector3 skullLocalOffset = Vector3.zero;
    public Vector3 skullLocalEuler  = Vector3.zero;
    public float   skullLocalScale  = 1f;

    private GameObject _skeletonInstance;
    private GameObject _skullInstance;

    void Awake()
    {
        if (!imageTarget) imageTarget = GetComponent<ObserverBehaviour>();
        if (imageTarget) imageTarget.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    void OnDestroy()
    {
        if (imageTarget) imageTarget.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool found = status.Status == Status.TRACKED ||
                     status.Status == Status.EXTENDED_TRACKED ||
                     status.Status == Status.LIMITED;

        if (found && _skeletonInstance == null)
            SpawnSkeleton();
    }

    private void SpawnSkeleton()
    {
        if (!imageTarget || prefabEsqueletoSemCranio == null || prefabCranio == null)
        {
            Debug.LogError("[ARSpawner] Referências faltando no Inspector.");
            return;
        }

        // 1) Instancia o esqueleto como filho do ImageTarget
        _skeletonInstance = Instantiate(prefabEsqueletoSemCranio, imageTarget.transform);
        _skeletonInstance.name = "Esqueleto_AR";
        _skeletonInstance.transform.localPosition   = offsetSpawn;
        _skeletonInstance.transform.localEulerAngles = spawnDeitado ? rotDeitado : rotEmPe;
        _skeletonInstance.transform.localScale      = Vector3.one * escalaInicial;
        SetLayerRecursively(_skeletonInstance, LayerMaskToLayer(arLayer));

        // 2) Achar o socket da cabeça
        var socket = headSocketInSkeleton ? headSocketInSkeleton : FindHeadSocket(_skeletonInstance.transform);
        if (!socket)
        {
            Debug.LogError("[ARSpawner] Head socket não encontrado. Ajuste 'headSocketNames' ou arraste manualmente.");
            socket = _skeletonInstance.transform;
        }
        headSocketInSkeleton = socket;

        // 3) Instanciar o CRÂNIO
        if (preserveAuthoredWorldAlignment)
        {
            // Nasce como IRMÃO do esqueleto, alinhado à origem do ImageTarget (como quando você arrasta na cena)
            _skullInstance = Instantiate(prefabCranio, imageTarget.transform);
            _skullInstance.name = "Cranio_AR";

            // Pose local neutra dentro do ImageTarget
            _skullInstance.transform.localPosition = Vector3.zero;
            _skullInstance.transform.localRotation = Quaternion.identity;

            // *** ESCALA DO CRÂNIO SEGUE A MESMA ESCALA DO ESQUELETO ***
            _skullInstance.transform.localScale = Vector3.one * escalaInicial;

            // Agora reparenta para o socket mantendo a pose WORLD (posição/rotação/worldScale)
            _skullInstance.transform.SetParent(socket, true); // worldPositionStays = true
        }
        else
        {
            // Modo manual: crânio diretamente no socket, usando offsets locais
            _skullInstance = Instantiate(prefabCranio, socket, false);
            _skullInstance.name = "Cranio_AR";
            _skullInstance.transform.localPosition    = skullLocalOffset;
            _skullInstance.transform.localEulerAngles = skullLocalEuler;
            _skullInstance.transform.localScale       = Vector3.one * (skullLocalScale * escalaInicial);
        }

        // 4) Collider + Tag
        if (_skullInstance.GetComponent<Collider>() == null)
        {
            var mc = _skullInstance.AddComponent<MeshCollider>();
            mc.convex = true;
        }
        _skullInstance.tag = "Bone_Skull";

        // 5) Layer em toda a hierarquia do crânio
        SetLayerRecursively(_skullInstance, LayerMaskToLayer(arLayer));

        // 6) Entrega o root do esqueleto ao manipulador (se existir)
        var manip = FindObjectOfType<ARTouchManipulator>();
        if (manip) manip.targetRoot = _skeletonInstance.transform;
    }

    private Transform FindHeadSocket(Transform root)
    {
        // tenta pelos nomes definidos
        foreach (var name in headSocketNames)
        {
            var t = root.Find(name);
            if (t) return t;
        }

        // fallback: qualquer transform que contenha "head"
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.ToLower().Contains("head"))
                return t;
        }

        return null;
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
        return 0;
    }
}
