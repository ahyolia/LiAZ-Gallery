using System.Collections.Generic;
using UnityEngine;

// Script de debug temporaire :
// - T = lance une vraie recherche SerpApi codée en dur (nécessite une clé API valide).
// - G = construit une galerie factice (faux résultats, pas d'appel réseau), pour valider
//   le placement/l'orientation des wallSlots sans dépendre de l'API.
// À retirer ou désactiver une fois VoiceSearchInput opérationnel.
public class TestSearchTrigger : MonoBehaviour
{
    [SerializeField] private SearchManager searchManager;
    [SerializeField] private GalleryBuilder galleryBuilder;
    [SerializeField] private string debugQuery = "giyuu tomioka";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            BuildFakeGallery();

        if (Input.GetKeyDown(KeyCode.T))
            searchManager.LaunchSearch(debugQuery);
    }

    private void BuildFakeGallery()
    {
        var fakeResults = new List<SerpImageResult>();
        for (int i = 1; i <= 10; i++)
        {
            fakeResults.Add(new SerpImageResult
            {
                title = $"Résultat test {i}",
                link = "https://example.com",
                thumbnail = "", // vide -> déclenche SetFallback() dans GalleryBuilder
                original = ""
            });
        }

        galleryBuilder.BuildGallery(fakeResults);
    }
}
