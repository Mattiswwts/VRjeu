# Action! - VR Collaborative Game

## 🎯 Concept du projet
Jeu VR collaboratif asymétrique à 2 joueurs pour examen HCI avec Ubiq.

### Les deux rôles (TOUS DEUX EN VR) :

**L'ACTEUR** :
- Dans la scène principale (appartement, rue, scène de vie quotidienne)
- Reçoit des instructions en HUD (ex: "Va à la cuisine", "Prends l'objet")
- Voit la cabine de régie au loin mais ne peut pas y accéder
- Exécute les actions physiquement en VR (marcher, prendre, poser)
- Dépend du réalisateur pour que les effets se déclenchent au bon moment

**LE RÉALISATEUR** :
- Dans une cabine de régie physique (tour de contrôle, salle technique) dans le même monde
- Voit toute la scène + l'acteur depuis sa cabine
- A un prompteur qui défile à chaque action synchronisée réussie
- Manipule physiquement des leviers/boutons en VR pour déclencher :
  - Lumières (allumer/éteindre, changer couleurs)
  - Météo (pluie, vent, neige)
  - Sons (musique, bruitages)
  - Objets animés (portes, tiroirs, effets visuels)
- Doit déclencher le BON effet au BON moment selon le scénario

### Gameplay :
- Les deux joueurs suivent un **scénario commun** affiché dans leurs HUD respectifs
- L'acteur fait une action → le réalisateur déclenche l'effet correspondant
- **Fenêtre de synchronisation** : si l'effet arrive trop tôt/trop tard → RATÉ
- Si raté → la scène recommence depuis le début
- **Difficulté progressive** : fenêtre de synchro de plus en plus stricte
- **Score final** : nombre de "prises" nécessaires pour finir la scène

### Communication :
- **Voice chat Ubiq intégré** pour que les joueurs se parlent ("je suis prêt", "action!", etc.)

## 🛠️ Technologies
- **Unity version** : 2022.3+ LTS
- **Render Pipeline** : URP recommandé (pour effets visuels)
- **Networking** : Ubiq framework
- **VR** : XR Interaction Toolkit (pour les deux joueurs)
- **Input** : New Input System (compatibilité desktop → VR)
- **Voice** : Ubiq Voice Chat intégré

## 📁 Structure du projet
```
Assets/
├── Scenes/
│   ├── MainScene.unity         # Scène principale (acteur + cabine réalisateur)
│   └── TestScene.unity          # Scène de test networking
├── Scripts/
│   ├── Player/
│   │   ├── ActorController.cs   # Contrôle acteur VR
│   │   └── DirectorController.cs # Contrôle réalisateur VR
│   ├── Networking/
│   │   ├── UbiqSetup.cs         # Init Ubiq, rooms
│   │   └── SyncManager.cs       # Sync actions/effets
│   ├── GameLogic/
│   │   ├── ScenarioManager.cs   # Gère le scénario, prompteur
│   │   ├── SyncChecker.cs       # Vérifie fenêtre de synchro
│   │   └── ScoreManager.cs      # Compte les prises
│   ├── Effects/
│   │   ├── LightController.cs   # Contrôle lumières
│   │   ├── WeatherController.cs # Météo (pluie, vent)
│   │   └── SoundController.cs   # Sons/musique
│   └── UI/
│       ├── ActorHUD.cs          # Instructions acteur
│       └── DirectorHUD.cs       # Prompteur réalisateur
├── Prefabs/
│   ├── NetworkedPlayer/         # Prefabs joueurs Ubiq
│   ├── DirectorCabin/           # Cabine de régie
│   └── InteractableObjects/     # Objets scène
├── Materials/
│   └── Effects/                 # Matériaux effets visuels
└── Settings/
    └── InputActions/            # New Input System
```

## 🎮 Contrôles VR (pour les deux joueurs)

### Acteur :
- **Joystick gauche** : Déplacement / téléportation
- **Grip trigger** : Grab objets
- **Trigger** : Interagir
- **Menu** : Pause

### Réalisateur :
- **Joystick gauche** : Se déplacer dans la cabine (optionnel)
- **Grip trigger** : Grab leviers
- **Trigger** : Appuyer boutons, activer leviers
- **Menu** : Pause

## 🎨 Conventions de code
- **Namespaces** : ActionGame.Player, ActionGame.Networking, ActionGame.GameLogic, ActionGame.Effects
- **Private fields** : prefix m_ (ex: m_syncWindow)
- **Commentaires** : français OK
- **Code** : anglais (classes, méthodes, variables)
- **Networked objects** : héritent de NetworkBehaviour (Ubiq)
- **Events** : utiliser UnityEvents pour découpler

## 🔑 Architecture réseau Ubiq
```
ACTEUR (VR)                    RÉALISATEUR (VR)
    ↓                               ↓
[Exécute action]            [Déclenche effet]
    ↓                               ↓
    ↓────→ UBIQ SYNC ←──────────────↓
              ↓
    [SyncChecker vérifie timing]
              ↓
         ✅ ou ❌
              ↓
    [ScenarioManager avance OU reset]
```

### Messages Ubiq à implémenter :
- `ActionExecuted` : Acteur → SyncChecker (j'ai fait l'action X)
- `EffectTriggered` : Réalisateur → SyncChecker (j'ai déclenché l'effet Y)
- `SyncResult` : SyncChecker → Tous (✅ ou ❌)
- `ScenarioUpdate` : ScenarioManager → Tous (prochaine étape OU reset)

## ⚠️ Défis techniques
- **Latence réseau** : Fenêtre de synchro doit tolérer ~100-200ms
- **VR locomotion** : Téléportation pour éviter motion sickness
- **Testing** : Besoin de 2 casques OU 1 casque + 1 desktop pour tester
- **Voice chat** : Ubiq voice peut avoir du lag, prévoir fallback Discord
- **Physics** : Leviers doivent être satisfaisants à manipuler en VR

## 📝 Roadmap

### Phase 1 - Prototype DESKTOP (2-3 semaines)
- [ ] Setup Ubiq : 2 instances se connectent dans une room
- [ ] Scène basique : 1 pièce + 1 cabine simple
- [ ] Acteur desktop : WASD + clic souris pour actions
- [ ] Réalisateur desktop : Boutons UI pour effets
- [ ] SyncChecker : détecte si action + effet sont synchro
- [ ] ScenarioManager : 3 étapes simples (marcher → prendre → poser)
- [ ] Test avec 2 instances Unity sur localhost

### Phase 2 - Migration VR (1 semaine)
- [ ] Installer XR Toolkit + Ubiq XR Rig
- [ ] Convertir contrôles acteur en VR (teleport + grab)
- [ ] Convertir cabine réalisateur en espace 3D avec leviers physiques
- [ ] Adapter UI pour être dans l'espace 3D (world space canvas)
- [ ] Test avec 2 casques (ou 1 casque + 1 desktop)

### Phase 3 - Gameplay et polish (1-2 semaines)
- [ ] 2-3 scénarios différents (cuisine, rue, bureau)
- [ ] 5-10 effets variés (lumières, sons, météo, objets)
- [ ] Difficulté progressive (fenêtre de synchro réduite)
- [ ] Score et UI finale (nombre de prises)
- [ ] Voice chat Ubiq activé et testé
- [ ] Polish visuel et audio

### Phase 4 - Exam (dernière semaine)
- [ ] Documentation technique
- [ ] Vidéo démo du gameplay
- [ ] Préparation présentation
- [ ] Tests finaux 2 joueurs

## 🎯 Objectif exam (MVP)
- 1 scénario complet avec 5 étapes
- 3-5 effets fonctionnels
- Networking stable 2 joueurs (localhost minimum)
- Voice chat opérationnel
- Gameplay asymétrique clair et fun

## 🚫 À ignorer par Claude
Library/, Temp/, Logs/, Packages/, UserSettings/, .vs/, .idea/