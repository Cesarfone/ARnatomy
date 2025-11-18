using UnityEngine;

[System.Serializable]
public class FlashcardData
{
    [Header("Conteúdo do card")]
    [TextArea] public string pergunta;          // texto da frente
    public string respostaTitulo;               // ex: "Crânio"
    [TextArea] public string respostaDescricao; // ex: explicação

    [Header("Opcional - futuro (outros ossos)")]
    public string idOsso;                       // ex: "cranio"
}

public class FlashcardManager : MonoBehaviour
{
    [Header("Deck de flashcards")]
    public FlashcardData[] cards;

    [Header("Estado atual")]
    public int indiceAtual;
    public int acertos;
    public int erros;

    void Reset()
    {
        // Cria um card padrão se nada estiver configurado
        cards = new FlashcardData[1];
        cards[0] = new FlashcardData
        {
            idOsso = "cranio",
            pergunta = "Qual o nome desta estrutura do esqueleto humano?",
            respostaTitulo = "Crânio",
            respostaDescricao = "Estrutura óssea que protege o encéfalo e forma a face."
        };
    }

    public int Total => cards != null ? cards.Length : 0;

    public FlashcardData GetCardAtual()
    {
        if (cards == null || cards.Length == 0) return null;
        indiceAtual = Mathf.Clamp(indiceAtual, 0, cards.Length - 1);
        return cards[indiceAtual];
    }

    public void MarcarAcerto()
    {
        acertos++;
        ProximoCard();
    }

    public void MarcarErro()
    {
        erros++;
        ProximoCard();
    }

    void ProximoCard()
    {
        if (cards == null || cards.Length == 0) return;

        indiceAtual++;
        if (indiceAtual >= cards.Length)
        {
            // Protótipo: volta pro primeiro
            indiceAtual = 0;
        }
    }

    public string GetProgresso()
    {
        if (Total == 0) return "0/0";
        return $"{indiceAtual + 1}/{Total}";
    }
}
