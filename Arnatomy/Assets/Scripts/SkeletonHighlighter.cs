using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class SkeletonHighlighter : MonoBehaviour
{
    [Header("Referências")]
    public GameObject infoPanel;     // Painel UI lateral
    public Camera arCamera;          // ARCamera
    public GameObject modelPrefab;   // Prefab do esqueleto

    private GameObject displayModel; // Modelo em pé (modo observação)
    private bool observing = false;

    void OnMouseDown()
    {
        if (!observing)
            EnterObservationMode();
        else
            ExitObservationMode();
    }

    void EnterObservationMode()
    {
        // Pausa o feed da câmera RA (congela a imagem)
        if (VuforiaBehaviour.Instance != null)
            VuforiaBehaviour.Instance.enabled = false;

        // Mostra painel lateral
        if (infoPanel != null)
            infoPanel.SetActive(true);

        // Calcula posição na frente da câmera (1.2 m)
        Vector3 frontPos = arCamera.transform.position + arCamera.transform.forward * 1.2f;

        // Cria o modelo em pé
        displayModel = Instantiate(modelPrefab, frontPos, Quaternion.identity);
        displayModel.transform.rotation = Quaternion.Euler(0f, arCamera.transform.eulerAngles.y + 180f, 0f);
        displayModel.transform.localScale = Vector3.one * 0.7f;

        observing = true;
    }

    void ExitObservationMode()
    {
        // Reativa o feed de RA
        if (VuforiaBehaviour.Instance != null)
            VuforiaBehaviour.Instance.enabled = true;

        // Remove o modelo e o painel
        if (displayModel != null)
            Destroy(displayModel);

        if (infoPanel != null)
            infoPanel.SetActive(false);

        observing = false;
    }
}
