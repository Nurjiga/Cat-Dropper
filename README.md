# Cat Dropper

A Suika-style cat merging game made in Unity for iPhone. AITU university project, group MT-2401, Assignment 4.

Drop cats into the jar: two identical cats merge into a bigger one. Reach the Legendary cat to double your score, and don't let the cats stack above the dashed line.

## How to download and run the project on another computer

### 1. What to install
- [Unity Hub](https://unity.com/download).
- **Unity Editor 6000.6.7f1** (this exact version). In Unity Hub: *Installs → Install Editor → Archive → Unity 6* and find `6000.6.7f1`. To build for iPhone, install it together with the **iOS Build Support** module.
- To run the game on an iPhone you also need a Mac and Xcode. To play inside Unity (the ▶ button) you do not need them.

### 2. Download the project
Option A, with a terminal (Git required):
```bash
git clone https://github.com/Nurjiga/Cat-Dropper.git
```
Option B, without Git: on the repository page click **Code → Download ZIP** and unzip the archive.

> Download the whole repository. The `Assets` folder alone is not enough: the `Packages` folder (Unity packages) and the `ProjectSettings` folder (project settings) are required next to it.

### 3. Open it in Unity
1. In Unity Hub click **Add → Add project from disk**.
2. Select the **`Cat Dropper`** folder inside the downloaded repository (it contains `Assets`, `Packages` and `ProjectSettings`).
3. Open the project with version **6000.6.7f1**.
4. Wait: the first time, Unity downloads the packages and builds its cache (`Library`). This takes a few minutes and requires an internet connection.

### 4. Run the game
1. In the Project window open the scene `Assets/Scenes/Main Menu.unity`.
2. Press the **▶ Play** button at the top. The first click on the *Game* window only gives it focus.
3. In the Game window choose a portrait screen size, for example **iPhone 12 Pro Max**.

## Project structure
| Folder | Contents |
|---|---|
| `Cat Dropper/Assets/Scenes` | Two scenes: `Main Menu` (first) and `SampleScene` (the game itself) |
| `Cat Dropper/Assets/Scripts` | Game scripts (`GameManager`, `Cat`, `DeadZone`, `SaveData`, `AudioManager`, `TutorialModal`, `SettingsPanel`, etc.) |
| `Cat Dropper/Assets/Editor` | Editor tools in the menu **Tools → Cat Dropper** (build the Settings screen and tutorial windows, button style, reset progress) |
| `Cat Dropper/Assets/Resources/Audio` | Sound effects and music |
| `Cat Dropper/Assets/Sprites`, `Prefabs` | Images and cats (`cat_0 … cat_7`) |
| `Cat Dropper/Tools` | `generate_sounds.py`, a script that synthesizes all sounds (needs Python and numpy) |

## Building for iPhone
1. In Unity: **File → Build Profiles → iOS → Build**, and choose an empty folder.
2. Open `Unity-iPhone.xcodeproj` in Xcode.
3. *Signing & Capabilities* → enable *Automatically manage signing* and choose your Team (Apple ID).
4. Connect the iPhone, select it at the top and press ▶. The first time, on the phone go to Settings → General → VPN & Device Management and trust the developer.

## Troubleshooting
- **Lots of red errors in the console after opening:** check the Unity version (it must be `6000.6.7f1`) and that you had an internet connection on the first open. Close Unity, delete the `Library` folder next to `Assets` and open the project again.
- **The project does not show up in Unity Hub:** add the `Cat Dropper` folder (inside the repository), not the repository root.
- **An empty scene opens:** double-click `Assets/Scenes/Main Menu.unity`.
- **Reset progress** (best score, unlocked cats, tutorial): **Tools → Cat Dropper → Reset Progress (PlayerPrefs)**.

## Saved data (PlayerPrefs)
`TutorialSeen`, `BestScore`, `CatUnlocked_0…7`, `SoundOn`, `MusicOn`.
