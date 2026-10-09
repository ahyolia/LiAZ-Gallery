using TMPro;
using UnityEngine;

// Composant posé sur le prefab ResultPanel (Quad + Collider).
// Stocke les infos d'un résultat SerpApi et gère l'affichage + le clic (souris ou XR).
public class ResultPanel : MonoBehaviour
{
    [SerializeField] private Renderer quadRenderer;
    [SerializeField] private Color fallbackColor = Color.gray;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Color failColor = Color.red;

    private string title;
    private string sourceLink;
    private string source;

    public void Setup(string title, string link, string source)
    {
        this.title = title;
        sourceLink = link;
        this.source = source;

        // Le titre n'est plus affiché sur le panneau mural (juste l'image) ; il reste
        // toutefois utilisé par ShowDetails() pour le panneau de détails flottant.
        if (labelText != null)
            labelText.text = string.Empty;
    }

    public void SetTexture(Texture2D texture)
    {
        if (quadRenderer != null)
            quadRenderer.material.mainTexture = texture;
    }

    public void SetFallback()
    {
        // Le shader URP/Lit (Tableau.mat) expose "_BaseColor", pas "_Color" :
        // Material.color cible toujours "_Color" et échoue silencieusement sur ce shader.
        if (quadRenderer != null)
            quadRenderer.material.SetColor("_BaseColor", fallbackColor);

        if (labelText != null)
        {
            labelText.text = "Échec du chargement";
            labelText.color = failColor;
        }
    }

    // Appelé par MouseClickInteractor (desktop) ou par l'event Select Entered
    // d'un XR Simple Interactable (XR) sur le même prefab : ouvre le panneau de
    // détails flottant plutôt que directement le navigateur.
    public void ShowDetails()
    {
        DetailPanelController.Instance?.Show(title, source, sourceLink);
    }

    // Conservé pour le bouton "Ouvrir" à l'intérieur du panneau de détails.
    public void OpenLink()
    {
        if (!string.IsNullOrEmpty(sourceLink))
            Application.OpenURL(sourceLink);
    }
}
