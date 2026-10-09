using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Instancie les résultats SerpApi sous forme de panneaux-images, un par
// emplacement mural prédéfini dans la scène (wallSlots), dans l'ordre.
public class GalleryBuilder : MonoBehaviour
{
    [SerializeField] private ResultPanel resultPanelPrefab;

    // Transforms placés à la main sur les murs de la map (position + rotation
    // déjà orientées face à la pièce). Un résultat = un slot, dans l'ordre de la liste.
    [SerializeField] private List<Transform> wallSlots = new List<Transform>();

    private readonly List<ResultPanel> currentPanels = new List<ResultPanel>();
    private readonly List<Coroutine> currentImageLoads = new List<Coroutine>();

    public void BuildGallery(List<SerpImageResult> results)
    {
        ClearGallery();

        if (results == null || results.Count == 0 || wallSlots.Count == 0)
        {
            Debug.LogWarning($"GalleryBuilder: rien à construire (results={results?.Count ?? 0}, wallSlots={wallSlots.Count}).");
            return;
        }

        if (resultPanelPrefab == null)
        {
            Debug.LogError("GalleryBuilder: resultPanelPrefab n'est pas assigné dans l'Inspector.");
            return;
        }

        int count = Mathf.Min(results.Count, wallSlots.Count);
        Debug.Log($"GalleryBuilder: construction de {count} panneaux.");
        for (int i = 0; i < count; i++)
        {
            Transform slot = wallSlots[i];
            if (slot == null)
            {
                Debug.LogWarning($"GalleryBuilder: wallSlots[{i}] est vide (référence manquante).");
                continue;
            }

            ResultPanel panel = Instantiate(resultPanelPrefab, slot.position, slot.rotation, slot);
            Debug.Log($"GalleryBuilder: panneau {i} instancié à {slot.position}, actif={panel.gameObject.activeInHierarchy}.");

            SerpImageResult result = results[i];
            panel.Setup(result.title, result.link, result.source);
            currentPanels.Add(panel);

            currentImageLoads.Add(StartCoroutine(LoadImage(panel, result.thumbnail)));
        }
    }

    private IEnumerator LoadImage(ResultPanel panel, string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            panel.SetFallback();
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            // Le panneau peut avoir été détruit entre-temps (nouvelle recherche lancée) :
            // on vérifie avant de toucher à ses composants pour éviter une MissingReferenceException.
            if (panel == null)
                yield break;

            if (request.result != UnityWebRequest.Result.Success)
            {
                panel.SetFallback();
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            panel.SetTexture(texture);
        }
    }

    public void ClearGallery()
    {
        foreach (Coroutine coroutine in currentImageLoads)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        currentImageLoads.Clear();

        foreach (ResultPanel panel in currentPanels)
        {
            if (panel != null)
                Destroy(panel.gameObject);
        }
        currentPanels.Clear();
    }
}
