using System.Linq;
using TMPro;
using UnityEngine;
#if PLATFORM_STANDALONE_WIN || UNITY_WSA
using UnityEngine.Windows.Speech;
#endif

// Interface vocale pour déclencher une recherche : un KeywordRecognizer écoute en
// permanence le mot-clé déclencheur (triggerKeyword). Une fois détecté, on bascule sur
// un DictationRecognizer pour capter la requête parlée (les deux ne peuvent pas tourner
// en même temps). Le mot-clé de fin (stopKeyword) est cherché DANS le texte dicté puis
// retiré de la requête, car un second KeywordRecognizer ne peut pas tourner pendant la
// dictée. Une fois la dictée terminée, le KeywordRecognizer est relancé.
// Ne fonctionne qu'en Windows Standalone/UWP (UnityEngine.Windows.Speech) — nécessite
// Paramètres Windows > Confidentialité > Voix > Reconnaissance vocale en ligne activée.
public class VoiceSearchInput : MonoBehaviour
{
    [SerializeField] private SearchManager searchManager;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string triggerKeyword = "recherche";
    [SerializeField] private string stopKeyword = "termine";

    // Exposés en lecture seule pour que HelpOverlay affiche les vraies valeurs
    // configurées dans l'Inspector, plutôt que de les dupliquer en dur ailleurs.
    public string TriggerKeyword => triggerKeyword;
    public string StopKeyword => stopKeyword;

#if PLATFORM_STANDALONE_WIN || UNITY_WSA
    private KeywordRecognizer keywordRecognizer;
    private DictationRecognizer dictationRecognizer;
#endif

    private void Start()
    {
        UpdateStatus($"Dis \"{triggerKeyword}\" pour lancer une recherche.");

#if PLATFORM_STANDALONE_WIN || UNITY_WSA
        StartKeywordRecognizer();
#else
        Debug.LogWarning("VoiceSearchInput: reconnaissance vocale disponible uniquement en Windows Standalone/UWP. Utilise TestSearchTrigger (touches T/G) pour tester ici.");
#endif
    }

    private void OnDestroy()
    {
#if PLATFORM_STANDALONE_WIN || UNITY_WSA
        keywordRecognizer?.Dispose();
        dictationRecognizer?.Dispose();
#endif
    }

#if PLATFORM_STANDALONE_WIN || UNITY_WSA
    private void StartKeywordRecognizer()
    {
        TryStartKeywordRecognizer();
    }

    private bool TryStartKeywordRecognizer()
    {
        try
        {
            // ConfidenceLevel.Low : un mot-clé unique en français est souvent reconnu
            // sous le seuil par défaut (Medium), surtout avec un accent/micro moyen.
            keywordRecognizer = new KeywordRecognizer(new[] { triggerKeyword }, ConfidenceLevel.Low);
            keywordRecognizer.OnPhraseRecognized += OnTriggerKeywordRecognized;
            keywordRecognizer.Start();
            Debug.Log($"VoiceSearchInput: KeywordRecognizer démarré, IsRunning={keywordRecognizer.IsRunning}.");
            return true;
        }
        catch (System.Exception e)
        {
            keywordRecognizer?.Dispose();
            keywordRecognizer = null;
            Debug.Log($"VoiceSearchInput: échec du démarrage du KeywordRecognizer (retentera) : {e.Message}");
            return false;
        }
    }

    private void OnTriggerKeywordRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log($"VoiceSearchInput: mot-clé déclencheur reconnu (\"{args.text}\", confiance={args.confidence}).");

        // Dispose() seul ne libère jamais le PhraseRecognitionSystem sous-jacent en build
        // Standalone (confirmé : 20/20 tentatives échouent avec la même erreur, quel que
        // soit le délai attendu) : il faut appeler explicitement Shutdown() pour forcer la
        // libération de la session native avant de démarrer le DictationRecognizer.
        keywordRecognizer.OnPhraseRecognized -= OnTriggerKeywordRecognized;
        keywordRecognizer.Dispose();
        keywordRecognizer = null;
        PhraseRecognitionSystem.Shutdown();

        UpdateStatus($"Je vous écoute... dites \"{stopKeyword}\" pour valider.");
        StartCoroutine(StartDictationWithRetry());
    }

    private System.Collections.IEnumerator StartDictationWithRetry()
    {
        const int maxAttempts = 20;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            yield return new WaitForSeconds(0.25f);

            if (TryStartDictation())
                yield break;

            Debug.Log($"VoiceSearchInput: DictationRecognizer pas encore disponible, nouvelle tentative ({attempt}/{maxAttempts})...");
        }

        Debug.LogError("VoiceSearchInput: échec définitif du démarrage du DictationRecognizer après plusieurs tentatives.");
    }

    private bool TryStartDictation()
    {
        try
        {
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationHypothesis += OnDictationHypothesis;
            dictationRecognizer.DictationResult += OnDictationResult;
            dictationRecognizer.DictationComplete += OnDictationComplete;
            dictationRecognizer.DictationError += OnDictationError;
            dictationRecognizer.Start();
            Debug.Log($"VoiceSearchInput: DictationRecognizer démarré, Status={dictationRecognizer.Status}.");
            return true;
        }
        catch (System.Exception e)
        {
            dictationRecognizer?.Dispose();
            dictationRecognizer = null;
            Debug.Log($"VoiceSearchInput: échec du démarrage du DictationRecognizer (retentera) : {e.Message}");
            return false;
        }
    }

    private void OnDictationHypothesis(string text)
    {
        UpdateStatus($"« {text} »");
    }

    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log($"VoiceSearchInput: DictationResult reçu (\"{text}\", confiance={confidence}).");

        bool containsStopKeyword = text.Split(' ').Any(word => word.Trim('.', ',', '!', '?').Equals(stopKeyword, System.StringComparison.OrdinalIgnoreCase));

        if (!containsStopKeyword)
            return;

        string query = RemoveStopKeyword(text);
        EndDictation();

        if (string.IsNullOrWhiteSpace(query))
        {
            UpdateStatus($"Requête vide, réessaie. Dis \"{triggerKeyword}\" pour recommencer.");
            return;
        }

        UpdateStatus($"Recherche : « {query} »");
        searchManager.LaunchSearch(query);
    }

    private void OnDictationComplete(DictationCompletionCause cause)
    {
        // Fin de dictée sans avoir entendu le mot-clé de fin (timeout, silence trop long...) :
        // on repasse simplement en écoute du mot-clé déclenchement.
        if (cause != DictationCompletionCause.Complete)
            EndDictation();
    }

    private void OnDictationError(string error, int hresult)
    {
        Debug.LogError($"VoiceSearchInput: erreur DictationRecognizer ({error}, hresult={hresult}).");
        EndDictation();
    }

    private string RemoveStopKeyword(string text)
    {
        string[] words = text.Split(' ');
        string cleaned = string.Join(" ", words.Where(word => !word.Trim('.', ',', '!', '?').Equals(stopKeyword, System.StringComparison.OrdinalIgnoreCase)));
        return cleaned.Trim();
    }

    private void EndDictation()
    {
        dictationRecognizer.DictationHypothesis -= OnDictationHypothesis;
        dictationRecognizer.DictationResult -= OnDictationResult;
        dictationRecognizer.DictationComplete -= OnDictationComplete;
        dictationRecognizer.DictationError -= OnDictationError;
        dictationRecognizer.Dispose();
        dictationRecognizer = null;
        PhraseRecognitionSystem.Shutdown();

        UpdateStatus($"Dis \"{triggerKeyword}\" pour lancer une recherche.");
        StartCoroutine(StartKeywordRecognizerWithRetry());
    }

    private System.Collections.IEnumerator StartKeywordRecognizerWithRetry()
    {
        const int maxAttempts = 20;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            yield return new WaitForSeconds(0.25f);

            if (TryStartKeywordRecognizer())
                yield break;

            Debug.Log($"VoiceSearchInput: KeywordRecognizer pas encore disponible, nouvelle tentative ({attempt}/{maxAttempts})...");
        }

        Debug.LogError("VoiceSearchInput: échec définitif du redémarrage du KeywordRecognizer après plusieurs tentatives.");
    }
#endif

    private void UpdateStatus(string message)
    {
        Debug.Log($"VoiceSearchInput: {message}");

        if (statusText != null)
            statusText.text = message;
    }
}
