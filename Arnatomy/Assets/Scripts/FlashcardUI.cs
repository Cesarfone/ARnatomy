using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashcardUI : MonoBehaviour
{
    [Header("Managers / Navegação")]
    public FlashcardManager manager;
    public AppScreenManager appScreenManager;    // pra trocar pra RA
    public InfoOverlayController infoOverlay;    // pra abrir a tela 3D do crânio

    [Header("Header")]
    public TextMeshProUGUI deckTitleText;        // ex: Title_Flashcards
    public TextMeshProUGUI progressText;         // ex: Text_Progress (ACERTOS/TOTAL)

    [Header("Card")]
    public Button cardButton;                    // Button no CardPanel
    public TextMeshProUGUI textPergunta;         // Text_Pergunta (frente)
    public TextMeshProUGUI textResposta;         // Text_Resposta (verso, UMA caixa só)
    public Button btnVerEmRA;                    // Btn_VerEmRA (só aparece no verso)

    [Header("Autoavaliação")]
    public Button btnAcertei;                    // Btn_Acertei
    public Button btnErrei;                      // Btn_Errei
    public TextMeshProUGUI statsText;            // Text_Stats

    private bool mostrandoVerso = false;

    void Start()
    {
        if (manager == null)
        {
            Debug.LogError("[FlashcardUI] Manager não ligado.");
            return;
        }

        // Título fixo do deck
        if (deckTitleText != null)
            deckTitleText.text = "Deck: Sistema Esquelético";

        AtualizarUICompleta();

        // Eventos
        if (cardButton != null)
            cardButton.onClick.AddListener(ToggleFace);

        if (btnVerEmRA != null)
            btnVerEmRA.onClick.AddListener(VerEmRA);

        if (btnAcertei != null)
            btnAcertei.onClick.AddListener(OnAcertei);

        if (btnErrei != null)
            btnErrei.onClick.AddListener(OnErrei);
    }

    /// <summary>
    /// Carrega o conteúdo do card atual + progresso + stats.
    /// </summary>
    void AtualizarUICompleta()
    {
        var card = manager.GetCardAtual();
        if (card == null)
        {
            Debug.LogWarning("[FlashcardUI] Nenhum card configurado no FlashcardManager.");
            return;
        }

        // Frente: PERGUNTA
        if (textPergunta != null)
            textPergunta.text = card.pergunta;

        // Verso: TITULO + DESCRIÇÃO na MESMA caixa de texto
        if (textResposta != null)
        {
            string respostaCompleta = card.respostaTitulo;

            if (!string.IsNullOrWhiteSpace(card.respostaDescricao))
                respostaCompleta += "\n\n" + card.respostaDescricao;

            textResposta.text = respostaCompleta;
        }

        // Progresso: ACERTOS / TOTAL
        if (progressText != null)
            progressText.text = manager.GetProgresso();

        // Stats
        AtualizarStats();

        // Sempre começa mostrando a frente
        SetFace(false);
    }

    void SetFace(bool verso)
    {
        mostrandoVerso = verso;

        bool mostrarFrente = !verso;
        bool mostrarVerso  = verso;

        if (textPergunta != null)
            textPergunta.gameObject.SetActive(mostrarFrente);

        if (textResposta != null)
            textResposta.gameObject.SetActive(mostrarVerso);

        if (btnVerEmRA != null)
            btnVerEmRA.gameObject.SetActive(mostrarVerso);
    }

    public void ToggleFace()
    {
        SetFace(!mostrandoVerso);
    }

    void OnAcertei()
    {
        manager.MarcarAcerto();
        AtualizarDepoisDeResponder();
    }

    void OnErrei()
    {
        manager.MarcarErro();
        AtualizarDepoisDeResponder();
    }

    void AtualizarDepoisDeResponder()
    {
        AtualizarUICompleta();   // manager já avançou pro próximo card
    }

    void AtualizarStats()
    {
        if (statsText == null) return;
        statsText.text = $"Acertos: {manager.acertos}  |  Erros: {manager.erros}";
    }

    void VerEmRA()
    {
        // Volta para a tela RA
        if (appScreenManager != null)
            appScreenManager.ShowRA();

        // Abre a tela 3D do crânio (InfoOverlay)
        if (infoOverlay != null)
            infoOverlay.Abrir();
    }
}
