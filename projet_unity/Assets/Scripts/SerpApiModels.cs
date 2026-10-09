using System;
using System.Collections.Generic;

// Résultat d'image générique transmis à GalleryBuilder, indépendant du fournisseur
// de recherche (rempli par SearchManager à partir de la réponse Serper).
[Serializable]
public class SerpImageResult
{
    public string title;
    public string link;
    public string thumbnail;
    public string original;
    public string source;
}

// Classes de parsing JSON pour la réponse de Serper (https://google.serper.dev/images).
// Serper est utilisé en remplacement de l'API Google Custom Search officielle,
// dont le quota/la facturation sont trop contraignants pour un TP.
[Serializable]
public class SerperSearchRequest
{
    public string q;
}

[Serializable]
public class SerperImageResult
{
    public string title;
    public string imageUrl;
    public string link;
    public string thumbnailUrl;
    public string source;
}

[Serializable]
public class SerperImagesResponse
{
    public List<SerperImageResult> images;
}
