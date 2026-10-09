using TMPro;
using UnityEngine;

// Panneau de détails unique affiché en overlay écran (Canvas Screen Space) quand on
// clique sur un ResultPanel mural, par-dessus un fond assombri semi-transparent qui
// couvre tout l'écran. Contient titre + source + lien (les seules infos que Serper
// fournit réellement, pas de fausse description inventée), un bouton pour ouvrir le
// lien dans le navigateur, et une croix de fermeture.
// Singleton : un seul panneau de détails existe dans la scène, tous les ResultPanel le
// référencent via DetailPanelController.Instance.
public class DetailPanelController : MonoBehaviour
{
    public static DetailPanelController Instance { get; private set; }

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text sourceText;
    [SerializeField] private TMP_Text linkText;

    private string currentLink;

    private void Awake()
    {
        Instance = this;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show(string title, string source, string link)
    {
        currentLink = link;

        if (titleText != null)
            titleText.text = string.IsNullOrEmpty(title) ? "(sans titre)" : title;

        if (sourceText != null)
            sourceText.text = string.IsNullOrEmpty(source) ? "Source inconnue" : source;

        if (linkText != null)
            linkText.text = string.IsNullOrEmpty(link) ? "" : link;

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    // Appelé par le bouton "Ouvrir" du panneau (souris ou XR Simple Interactable).
    public void OpenCurrentLink()
    {
        if (!string.IsNullOrEmpty(currentLink))
            Application.OpenURL(currentLink);
    }
}
