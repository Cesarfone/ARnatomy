using UnityEngine;

public class AppScreenManager : MonoBehaviour
{
    [Header("Roots / Telas")]
    public GameObject arRoot;            // ARRoot (ARCamera + ImageTarget dentro)
    public GameObject panelRA;           // Panel_RA
    public GameObject panelFlashcards;   // Panel_Flashcards
    public GameObject panelQuiz;         // Panel_Quiz (placeholder)

    void Start()
    {
        ShowRA();
    }

    public void ShowRA()
    {
        if (arRoot)          arRoot.SetActive(true);
        if (panelRA)         panelRA.SetActive(true);
        if (panelFlashcards) panelFlashcards.SetActive(false);
        if (panelQuiz)       panelQuiz.SetActive(false);
    }

    public void ShowFlashcards()
    {
        if (panelRA)         panelRA.SetActive(false);
        if (panelFlashcards) panelFlashcards.SetActive(true);
        if (panelQuiz)       panelQuiz.SetActive(false);
        // ARRoot continua ligado (câmera viva), mas coberto pela UI
    }

    public void ShowQuiz()
    {
        if (panelRA)         panelRA.SetActive(false);
        if (panelFlashcards) panelFlashcards.SetActive(false);
        if (panelQuiz)       panelQuiz.SetActive(true);
    }
}
