using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// Lance une recherche d'images via Serper (https://serper.dev), utilisé en
// remplacement de l'API Google Custom Search officielle (quota/facturation trop
// contraignants pour un TP), puis transmet les résultats à GalleryBuilder.
public class SearchManager : MonoBehaviour
{
    [SerializeField] private int maxResults = 8;
    [SerializeField] private GalleryBuilder galleryBuilder;

    private const string BaseUrl = "https://google.serper.dev/images";

    private string apiKey;
    private Coroutine currentSearch;

    void Awake()
    {
        // La clé vit dans Assets/Resources/SerperApiConfig.asset, exclu du dépôt via
        // .gitignore (voir SerperApiConfig.asset.example pour le gabarit), plutôt que
        // sérialisée directement sur ce composant dans la scène versionnée.
        SerperApiConfig config = Resources.Load<SerperApiConfig>("SerperApiConfig");
        apiKey = config != null ? config.apiKey : null;
    }

    public void LaunchSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            Debug.LogWarning("SearchManager: requête vide, recherche annulée.");
            return;
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("SearchManager: apiKey introuvable — copie Assets/Resources/SerperApiConfig.asset.example vers SerperApiConfig.asset (même dossier) et renseigne ta clé Serper dedans.");
            return;
        }

        if (currentSearch != null)
            StopCoroutine(currentSearch);

        currentSearch = StartCoroutine(SearchRoutine(query));
    }

    private IEnumerator SearchRoutine(string query)
    {
        string jsonBody = JsonUtility.ToJson(new SerperSearchRequest { q = query });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(BaseUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-API-KEY", apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"SearchManager: échec de la requête Serper ({request.error}).");
                currentSearch = null;
                yield break;
            }

            SerperImagesResponse response = JsonUtility.FromJson<SerperImagesResponse>(request.downloadHandler.text);

            if (response == null || response.images == null || response.images.Count == 0)
            {
                Debug.LogWarning("SearchManager: aucun résultat renvoyé par Serper.");
                galleryBuilder.BuildGallery(new List<SerpImageResult>());
                currentSearch = null;
                yield break;
            }

            List<SerpImageResult> results = new List<SerpImageResult>();
            foreach (SerperImageResult image in response.images)
            {
                results.Add(new SerpImageResult
                {
                    title = image.title,
                    link = image.link,
                    thumbnail = image.thumbnailUrl,
                    original = image.imageUrl,
                    source = image.source
                });
            }

            if (results.Count > maxResults)
                results = results.GetRange(0, maxResults);

            galleryBuilder.BuildGallery(results);
        }

        currentSearch = null;
    }
}
