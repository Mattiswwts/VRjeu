# VRMirrorWorld — Action! (Jeu VR HCI)

## Concept
Jeu VR collaboratif asymétrique 2 joueurs (examen HCI, framework Ubiq).

**ACTEUR** : dans la scène principale, reçoit des instructions HUD, exécute des actions physiques (marcher, prendre, poser).  
**RÉALISATEUR** : dans une cabine de régie, déclenche des effets (lumières, météo, sons) via leviers/boutons VR.

**Gameplay** : l'acteur fait une action → le réalisateur déclenche l'effet correspondant dans une fenêtre de synchro (2s). Raté = reset. Score = nb de prises.

## Stack
- **Unity 6** (6000.x), URP 17.3.0
- **Ubiq** (UCL, upm branch) — networking P2P, voice chat, avatars
- **XR Interaction Toolkit 3.0.7** + **OpenXR 1.13.0** + **XR Hands 1.4.0** — Meta Quest 2×
- **New Input System 1.18.0**

## Structure Scripts
```
Assets/Scripts/
├── Effects/     LightController, WeatherController, SoundController, DayNightController
├── Managers/    ScenarioManager (9 étapes), SyncChecker
├── Networking/  UbiqSetup, PlayerSync, NetworkedPlayerSync, PlayerSpawner,
│                RoleSelector, EffectsSync, ActorActionsSync, NetworkObjectState
├── Objects/     IInteractable, PickupItem, HoldableItem, MusicBoxController,
│                InteractableButton, Lever, DoorTeleport, ScenarioTrigger, XRPickupHelper
├── Player/      ActorController (desktop), ActorControllerXR (VR),
│                DirectorControllerXR (VR), XRActorInteractor, XRDirectorInteractor
└── UI/          HUDController
```

## Architecture réseau (Ubiq)
- `EffectsSync` : Réalisateur → tous — effets (Light, Weather, Sound, DayNight_On/Off, Music)
- `ActorActionsSync` : Acteur → tous — actions (nom objet/zone)
- `NetworkObjectState` : par objet ramassable — sync SetActive(false) quand l'objet est pris
- `PlayerSync` / `NetworkedPlayerSync` : positions joueurs (déjà fonctionnel)

## Scénario — 9 étapes
| # | Acteur attend | Directeur déclenche | Sync? |
|---|---------------|---------------------|-------|
| 0 | (réveil, auto) | - | auto (2s) |
| 1 | TV | Sound | ✅ |
| 2 | feuille | DayNight_On | - |
| 3 | doorM | - | - |
| 4 | tasse café | - | - |
| 5 | musicbox | Music | ✅ |
| 6 | doorJ | Weather | - |
| 7 | interrupteur | Light | ✅ |
| 8 | lit | DayNight_Off | ✅ |

## Conventions
- Namespaces : `ActionGame.Player`, `ActionGame.Networking`, `ActionGame.GameLogic`, `ActionGame.Effects`, `ActionGame.Objects`, `ActionGame.UI`
- Private fields : prefix `m_`
- Code en anglais, commentaires français OK
- Anti double-subscription : `RemoveListener` + `AddListener` dans `Start()` (jamais dans Inspector)
- Frame guard anti-double-toggle : flag `m_busy` + `StartCoroutine(ResetBusy())`
- Unity API Updater : XRBaseInteractor → `UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor`

## Points d'attention
- **Inspector vs code** : `[SerializeField]` garde les valeurs sérialisées même si le code change. Faire **Reset** sur le composant pour restaurer les défauts du code.
- **XRPickupHelper** : à ajouter sur chaque objet ramassable en VR (avec XRGrabInteractable). Envoie `ActorActionsSync.SendAction(gameObject.name)`.
- **NetworkObjectState** : à ajouter sur chaque objet ramassable (tasse café, CD). Synchro la disparition de l'objet sur les 2 clients.
- **Ubiq avatars** : non encore implémenté — requis pour l'examen.

## Setup VR (résumé)
Voir `SETUP_VR.md` pour le guide complet Unity Editor.
Rôles : F1 = Acteur, F2 = Réalisateur (clavier desktop) ou auto via RoleSelector en VR.
