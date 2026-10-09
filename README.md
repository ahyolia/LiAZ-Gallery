# LiAZ Gallery

Galerie d'images virtuelle en VR, pilotée à la voix. On dit un mot-clé, on prononce sa recherche, et les images trouvées sur le web s'accrochent aux murs de la galerie. On s'y déplace aux commandes d'un drone.

Projet réalisé avec Unity dans le cadre du cours *Dispositifs interactifs*.

## Fonctionnalités

- **Recherche vocale** : dire « recherche », énoncer sa requête, puis dire « terminé » pour la valider (reconnaissance vocale Windows).
- **Galerie dynamique** : les résultats de recherche d'images (via l'API [Serper](https://serper.dev)) sont affichés sous forme de tableaux sur les murs.
- **Détails d'une image** : un clic (souris ou contrôleur VR) sur un tableau ouvre un panneau avec le titre, la source et un bouton pour ouvrir le lien dans le navigateur.
- **Déplacement en drone** : vol libre avec collisions contre les murs et inclinaison du drone selon le mouvement.
- **Mode VR et mode bureau** : fonctionne avec un casque OpenXR ou au clavier/souris.
- **Aide permanente** : un encart en coin d'écran rappelle les commandes.

## Commandes

| Action | Commande |
|---|---|
| Se déplacer | `ZQSD` / `WASD` |
| Monter / descendre | `Espace` / `Ctrl gauche` |
| Tourner | `Q` / `E` ou souris |
| Regarder (mode bureau) | Souris |
| Libérer / reprendre la souris | `Échap` |
| Lancer une recherche | Dire « recherche », puis la requête, puis « terminé » |
| Voir les détails d'une image | Clic gauche ou sélection au contrôleur VR sur un tableau |

## Lancer le jeu (build Windows)

1. Télécharger l'archive du build dans l'onglet [Releases](https://github.com/ahyolia/liaz-gallery/releases).
2. Dézipper et lancer `LiAZ Gallery.exe`.
3. Pour la recherche vocale, activer **Paramètres Windows › Confidentialité et sécurité › Voix › Reconnaissance vocale en ligne**.

## Ouvrir le projet dans Unity

### Prérequis

- **Unity 6000.4.7f1** (installer cette version via Unity Hub)
- Windows (la reconnaissance vocale utilise `UnityEngine.Windows.Speech`)
- Une clé API gratuite [Serper](https://serper.dev)
- Optionnel : un casque VR compatible OpenXR (Meta Quest via Link, etc.)

### Installation

1. Cloner le dépôt :
   ```bash
   git clone https://github.com/ahyolia/liaz-gallery.git
   ```
2. Dans Unity Hub, **Add › Add project from disk** et choisir le dossier `projet_unity/`. Le premier lancement prend quelques minutes (Unity regénère le dossier `Library/`).
3. **Configurer la clé API** : dans `Assets/Resources/`, copier `SerperApiConfig.asset.example`, renommer la copie en `SerperApiConfig.asset`, puis remplacer `COLLE_TA_CLE_SERPER_ICI` par sa clé Serper. Ce fichier est ignoré par Git : la clé ne sera jamais publiée.
4. Ouvrir la scène `Assets/Scenes/Main VR scene.unity` et lancer le mode Play.

### Raccourcis de test (éditeur)

Le script `TestSearchTrigger` permet de tester sans micro :

- `T` : lance une recherche prédéfinie (nécessite la clé API) ;
- `G` : génère une fausse galerie, sans appel réseau, pour vérifier le placement des tableaux.

## Architecture

Les scripts se trouvent dans `projet_unity/Assets/Scripts/`.

```
VoiceSearchInput ──requête──▶ SearchManager ──résultats──▶ GalleryBuilder ──▶ ResultPanel (×N)
  (mot-clé + dictée)           (API Serper)                (un tableau par slot mural)      │
                                                                                            ▼ clic
                                                                               DetailPanelController
```

| Script | Rôle |
|---|---|
| `VoiceSearchInput` | Écoute le mot-clé déclencheur (`KeywordRecognizer`), puis passe en dictée (`DictationRecognizer`) jusqu'au mot de fin. |
| `SearchManager` | Envoie la requête à l'API Serper Images et convertit la réponse JSON. |
| `SerperApiConfig` | ScriptableObject qui contient la clé API, chargé depuis `Resources`. |
| `SerpApiModels` | Classes de données pour la requête et la réponse de l'API. |
| `GalleryBuilder` | Instancie un `ResultPanel` par emplacement mural et télécharge les miniatures. |
| `ResultPanel` | Tableau mural : affiche l'image (ou une couleur de repli) et ouvre les détails au clic. |
| `DetailPanelController` | Panneau de détails unique (titre, source, lien, bouton « Ouvrir »). |
| `MouseClickInteractor` | Raycast souris pour cliquer sur les tableaux en mode bureau. |
| `DroneController` | Déplacement du drone via `CharacterController`, regard souris hors VR. |
| `AnimateHandOnInput` | Animation des mains VR selon la gâchette et la poignée. |
| `HelpOverlay` | Encart d'aide affiché en permanence. |
| `TestSearchTrigger` | Raccourcis de debug (`T` / `G`). |

## Technologies

- Unity 6 (6000.4) avec Universal Render Pipeline
- XR Interaction Toolkit 2.5 et OpenXR
- Input System
- TextMeshPro
- API [Serper](https://serper.dev) (Google Images), choisie à la place de Google Custom Search à cause de ses quotas
- Reconnaissance vocale Windows (`UnityEngine.Windows.Speech`)

## Assets tiers

- [Gogo Casual Pack](https://assetstore.unity.com/) (décor, végétation)
- PolyKebap (décor)
- Sci-fi Drone (modèle du drone)
- Oculus Hands (mains VR)
- Starter Assets du XR Interaction Toolkit (Unity)

Ces assets restent la propriété de leurs auteurs respectifs et sont soumis à leurs propres licences.

## Auteur

Camélia AMIN HANDOYO
