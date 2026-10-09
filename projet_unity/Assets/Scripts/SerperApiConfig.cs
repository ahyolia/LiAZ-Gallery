using UnityEngine;

// Contient la clé API Serper, chargée depuis Resources plutôt que sérialisée
// dans une scène (qui, elle, est versionnée). L'asset réel (Resources/SerperApiConfig.asset)
// est exclu du dépôt via .gitignore ; voir Assets/Resources/SerperApiConfig.asset.example
// pour le gabarit à copier/renommer localement.
[CreateAssetMenu(fileName = "SerperApiConfig", menuName = "Gallery/Serper Api Config")]
public class SerperApiConfig : ScriptableObject
{
    public string apiKey;
}
