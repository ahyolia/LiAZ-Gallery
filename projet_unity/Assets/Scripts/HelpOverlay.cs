using TMPro;
using UnityEngine;

// Encart d'aide permanent et discret (coin de l'écran) rappelant les règles de
// fonctionnement : déplacements, déclenchement vocal, clic sur les tableaux.
// Répond à l'exigence de l'énoncé "aide accessible facilement, dès le premier écran,
// pas cachée" — pas un écran de démarrage qu'on peut manquer ou fermer par erreur.
public class HelpOverlay : MonoBehaviour
{
    [SerializeField] private TMP_Text helpText;
    [SerializeField] private VoiceSearchInput voiceSearchInput;

    private void Start()
    {
        if (helpText == null)
            return;

        string triggerKeyword = "recherche";
        string stopKeyword = "terminé";

        if (voiceSearchInput != null)
        {
            triggerKeyword = voiceSearchInput.TriggerKeyword;
            stopKeyword = voiceSearchInput.StopKeyword;
        }

        helpText.text =
            "Deplacement : ZQSD/WASD, Espace (monter), Ctrl (descendre), Q/E (tourner), souris (regard)\n" +
            $"Recherche vocale : dire \"{triggerKeyword}\" puis dire \"{stopKeyword}\" pour valider\n" +
            "Clic sur un tableau : afficher les details de l'image\n" +
            "Echap : liberer/reprendre la souris";
    }
}
