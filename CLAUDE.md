# VRMirrorWorld — Action! (Jeu VR HCI)

## Concept
Jeu VR collaboratif asymétrique 2 joueurs (examen HCI, framework Ubiq).

**ACTEUR** : dans la scène principale, reçoit des instructions HUD, exécute des actions physiques (marcher, prendre, poser).  
**RÉALISATEUR** : dans une cabine de régie, déclenche des effets (lumières, météo, sons) via leviers/boutons VR.

**Gameplay** : l'acteur fait une action → le réalisateur déclenche l'effet correspondant dans une fenêtre de synchro. Raté = reset. Score = nb de prises.

## Stack
- Unity 2022.3 LTS, URP
- Ubiq (networking + voice chat)
- XR Interaction Toolkit (VR)
- New Input System

## Structure Scripts
```
Assets/Scripts/
├── Effects/      LightController, WeatherController, SoundController, DayNightController
├── Managers/     ScenarioManager, SyncChecker
├── Networking/   UbiqSetup, PlayerSync, NetworkedPlayerSync, PlayerSpawner, RoleSelector, EffectsSync
├── Objects/      HoldableItem, DoorTeleport
├── Player/       ActorController, DirectorController
└── UI/           ActorHUD, DirectorHUD
```

## Architecture réseau (Ubiq)
- `ActionExecuted` : Acteur → SyncChecker
- `EffectTriggered` : Réalisateur → SyncChecker
- `SyncResult` : SyncChecker → Tous
- `ScenarioUpdate` : ScenarioManager → Tous

## Conventions
- Namespaces : `ActionGame.Player`, `ActionGame.Networking`, `ActionGame.GameLogic`, `ActionGame.Effects`
- Private fields : prefix `m_`
- Code en anglais, commentaires français OK
- Networked objects héritent de `NetworkBehaviour` (Ubiq)

## Phase actuelle
Phase 1 — Prototype Desktop (Ubiq, scène basique, SyncChecker, ScenarioManager).  
Référence complète : `PROJECT_CONTEXT.md`
