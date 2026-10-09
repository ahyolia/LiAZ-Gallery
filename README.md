# LiAZ Gallery

A voice-controlled virtual image gallery in VR. Say a keyword, speak your search, and the images found on the web are hung on the gallery walls. You explore the space by flying a drone.

Built with Unity for the *Dispositifs interactifs* (Interactive Devices) course.

## Features

- **Voice search**: say "recherche", speak your query, then say "terminé" to confirm (Windows speech recognition, French keywords).
- **Dynamic gallery**: image search results (from the [Serper](https://serper.dev) API) are displayed as paintings on the walls.
- **Image details**: clicking a painting (mouse or VR controller) opens a panel with the title, the source and a button that opens the link in your browser.
- **Drone movement**: free flight with wall collisions, and the drone tilts as it moves.
- **VR and desktop modes**: works with an OpenXR headset or with keyboard and mouse.
- **Always-visible help**: a small panel in the corner of the screen lists the controls.

## Controls

| Action | Control |
|---|---|
| Move | `ZQSD` / `WASD` |
| Go up / down | `Space` / `Left Ctrl` |
| Turn | `Q` / `E` or mouse |
| Look around (desktop mode) | Mouse |
| Release / capture the mouse | `Esc` |
| Start a search | Say "recherche", then your query, then "terminé" |
| Show image details | Left click or VR controller select on a painting |

## Running the game (Windows build)

1. Download the build archive from the [Releases](https://github.com/ahyolia/liaz-gallery/releases) page.
2. Unzip it and run `LiAZ Gallery.exe`.
3. For voice search, enable **Windows Settings › Privacy & security › Speech › Online speech recognition**.

## Opening the project in Unity

### Requirements

- **Unity 6000.4.7f1** (install this version through Unity Hub)
- Windows (speech recognition uses `UnityEngine.Windows.Speech`)
- A free [Serper](https://serper.dev) API key
- Optional: an OpenXR-compatible VR headset (Meta Quest via Link, etc.)

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/ahyolia/liaz-gallery.git
   ```
2. In Unity Hub, click **Add › Add project from disk** and select the `projet_unity/` folder. The first launch takes a few minutes while Unity rebuilds the `Library/` folder.
3. **Set up the API key**: in `Assets/Resources/`, duplicate `SerperApiConfig.asset.example`, rename the copy to `SerperApiConfig.asset`, and replace `COLLE_TA_CLE_SERPER_ICI` with your Serper key. Git ignores this file, so your key is never published.
4. Open the `Assets/Scenes/Main VR scene.unity` scene and press Play.

### Test shortcuts (editor)

The `TestSearchTrigger` script lets you test without a microphone:

- `T`: runs a predefined search (requires the API key);
- `G`: builds a fake gallery with no network call, to check where the paintings are placed.

## Architecture

The scripts are in `projet_unity/Assets/Scripts/`.

```
VoiceSearchInput ──query──▶ SearchManager ──results──▶ GalleryBuilder ──▶ ResultPanel (×N)
 (keyword + dictation)       (Serper API)              (one painting per wall slot)     │
                                                                                        ▼ click
                                                                           DetailPanelController
```

| Script | Role |
|---|---|
| `VoiceSearchInput` | Listens for the trigger keyword (`KeywordRecognizer`), then switches to dictation (`DictationRecognizer`) until the stop word. |
| `SearchManager` | Sends the query to the Serper Images API and converts the JSON response. |
| `SerperApiConfig` | ScriptableObject holding the API key, loaded from `Resources`. |
| `SerpApiModels` | Data classes for the API request and response. |
| `GalleryBuilder` | Creates one `ResultPanel` per wall slot and downloads the thumbnails. |
| `ResultPanel` | Wall painting: shows the image (or a fallback color) and opens the details on click. |
| `DetailPanelController` | Single details panel (title, source, link, "Open" button). |
| `MouseClickInteractor` | Mouse raycast for clicking paintings in desktop mode. |
| `DroneController` | Moves the drone with a `CharacterController`, mouse look outside VR. |
| `AnimateHandOnInput` | Animates the VR hands from the trigger and grip inputs. |
| `HelpOverlay` | Always-visible help panel. |
| `TestSearchTrigger` | Debug shortcuts (`T` / `G`). |

## Tech stack

- Unity 6 (6000.4) with the Universal Render Pipeline
- XR Interaction Toolkit 2.5 and OpenXR
- Input System
- TextMeshPro
- [Serper](https://serper.dev) API (Google Images), used instead of Google Custom Search because of its quotas
- Windows speech recognition (`UnityEngine.Windows.Speech`)

## Third-party assets

- [Gogo Casual Pack](https://assetstore.unity.com/) (scenery, plants)
- PolyKebap (scenery)
- Sci-fi Drone (drone model)
- Oculus Hands (VR hands)
- XR Interaction Toolkit Starter Assets (Unity)

These assets belong to their respective authors and are subject to their own licenses.

## Author

Camélia AMIN HANDOYO
