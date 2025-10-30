using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class PlacementController : MonoBehaviour
{
    [Header("Referências")]
    public PlaneFinderBehaviour planeFinder;
    public Button confirmButton;
    public GameObject skeletonPrefab;
    public Transform groundPlaneStage;

    private GameObject pendingObject;
    private GameObject placedSkeleton;

    void Start()
    {
        confirmButton.gameObject.SetActive(false);
        planeFinder.OnInteractiveHitTest.AddListener(OnPlaneTouched);
        confirmButton.onClick.AddListener(ConfirmPlacement);
    }

    void OnPlaneTouched(HitTestResult result)
    {
        if (placedSkeleton != null) return; // só permite um esqueleto por cena

        // Mostra botão de confirmação
        confirmButton.gameObject.SetActive(true);

        // Guarda o ponto tocado (mas não instancia ainda)
        if (pendingObject == null)
            pendingObject = new GameObject("PendingPosition");

        pendingObject.transform.position = result.Position;
        pendingObject.transform.rotation = result.Rotation;
    }

    void ConfirmPlacement()
    {
        if (placedSkeleton != null)
            return; // já existe, ignora

        if (skeletonPrefab != null && pendingObject != null)
        {
            placedSkeleton = Instantiate(
                skeletonPrefab,
                pendingObject.transform.position,
                pendingObject.transform.rotation,
                groundPlaneStage
            );

            confirmButton.gameObject.SetActive(false);
            Destroy(pendingObject);
        }
    }
}
