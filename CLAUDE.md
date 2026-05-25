# VRMirrorWorld — Action! (Jeu VR HCI)

## Concept
Jeu VR collaboratif asymétrique 2 joueurs (examen HCI, framework Ubiq).

**ACTEUR** : dans la scène principale, reçoit des instructions HUD, exécute des actions physiques (marcher, prendre, poser).  
**RÉALISATEUR** : dans une cabine de régie, déclenche des effets (lumières, météo, sons) via leviers/boutons.

**Gameplay** : l'acteur fait une action → le réalisateur déclenche l'effet correspondant dans une fenêtre de synchro (2s). Raté = reset. Score = nb de prises.

## Stack
- **Unity 6** (6000.x), URP 17.3.0
- **Ubiq** (UCL, upm branch) — networking P2P, voice chat, avatars
- **XR Interaction Toolkit 3.0.7** + **OpenXR 1.13.0** + **XR Hands 1.4.0** — Meta Quest 2×
- **New Input System 1.18.0**

## État actuel — SCÉNARIO DESKTOP COMPLET ✅
Tout fonctionne en desktop (clavier/souris). Prochaine étape : migration VR (SETUP_VR.md).

## Structure Scripts
```
Assets/Scripts/
├── Effects/     LightController, WeatherController, SoundController, DayNightController
├── Managers/    ScenarioManager (9 étapes ✅), SyncChecker
├── Networking/  UbiqSetup, PlayerSync, NetworkedPlayerSync, PlayerSpawner,
│                RoleSelector, EffectsSync, ActorActionsSync, NetworkObjectState
├── Objects/     IInteractable, PickupItem, HoldableItem, MusicBoxController,
│                InteractableButton, Lever, DoorTeleport, ScenarioTrigger, XRPickupHelper
├── Player/      ActorController (desktop ✅), ActorControllerXR (VR, à brancher),
│                DirectorControllerXR (VR, à brancher), XRActorInteractor, XRDirectorInteractor
└── UI/          HUDController
```

## Scénario — 9 étapes (toutes validées ✅)
| Étape | Acteur | Réalisateur | Sync | Notes |
|-------|--------|-------------|------|-------|
| 1 | (réveil auto 2s) | — | non | auto |
| 2 | TV | Sound | oui | bouton effectName="Sound" |
| 3 | feuille | DayNight_On | non | levier jour/nuit |
| 4 | doorM | — | non | DoorTeleport, trigger+clic |
| 5 | tasse café | — | non | PickupItem, NetworkObjectState |
| 6 | musicbox | Music | oui | CD (HoldableItem) → musicbox |
| 7 | doorJ | Weather | non | bouton effectName="Weather" |
| 8 | interrupteur | Light | oui | bouton effectName="Light" |
| 9 | lit | DayNight_Off | oui | levier nuit |

## Architecture réseau (Ubiq)
- `EffectsSync` : Réalisateur → tous — effets (Light, Weather, Sound, DayNight_On/Off, Music)
- `ActorActionsSync` : Acteur → tous — actions (nom objet/zone)
- `NetworkObjectState` : par objet ramassable — sync SetActive(false) sur les 2 clients
- `PlayerSync` / `NetworkedPlayerSync` : positions joueurs ✅

## Conventions
- Namespaces : `ActionGame.Player`, `ActionGame.Networking`, `ActionGame.GameLogic`, `ActionGame.Effects`, `ActionGame.Objects`, `ActionGame.UI`
- Private fields : prefix `m_`
- Code en anglais, commentaires français OK
- Anti double-subscription : `RemoveListener` + `AddListener` dans `Start()`
- Frame guard anti-double-toggle : flag `m_busy` + `StartCoroutine(ResetBusy())`
- Comparaisons actions/effets : **OrdinalIgnoreCase** (insensible à la casse)
- Debug en jeu : **F8** = forcer l'avancement d'une étape (#if UNITY_EDITOR uniquement)

## Bugs corrigés dans cette session
- Double subscription LightController → m_busy guard
- Weather/DayNight/Sound pas connectés → auto-subscribe dans Start()
- Lever pas networké → auto-connect à EffectsSync.SendDayNight
- HUD prompteur figé → RefreshHUD() + OnActorInstructionChanged subscription
- ScenarioManager Steps=3 sérialisé → Reset composant → 9 étapes
- Objet tasse/CD visible côté réalisateur après pickup → NetworkObjectState.SyncHide()
- DoorTeleport m_used flag bloquait les clics → supprimé
- DoorTeleport bypasse ActorActionsSync → corrigé
- effectName casse ("music" vs "Music") → OrdinalIgnoreCase dans RegisterDirectorEffect

## Points d'attention Inspector Unity
- `[SerializeField]` garde les valeurs sérialisées même si le code change → **Reset** pour restaurer
- Boutons directeur : `effectName` doit correspondre à `expectedDirectorEffect` (casse ignorée maintenant)
- `tasse café` et `CD` : ajouter **NetworkObjectState** pour sync visibilité
- `CD` : doit avoir **HoldableItem** (pas PickupItem)
- `doorM` / `doorJ` : **DoorTeleport** + **Target Point** assigné + **Is Trigger = true**

## Prochaine étape — Migration VR
Voir `SETUP_VR.md` (guide complet Unity Editor, 10 étapes + section avatars Ubiq).
Rôles en VR : RoleSelector automatique ou F1/F2 en desktop.
