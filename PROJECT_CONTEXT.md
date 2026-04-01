\# VR Mirror World Project



\## 🎯 Objectif

Projet d'examen HCI : expérience multijoueur inspirée de Stranger Things

\- Joueur 1 : dans un lab abandonné normal

\- Joueur 2 : dans une version sombre (monde miroir)

\- Les joueurs interagissent indirectement via des objets synchronisés

\- Phase 1 : Développement PC (clavier/souris)

\- Phase 2 : Migration VR (plus tard avec casque)



\## 🛠️ Technologies

\- \*\*Unity version\*\* : 2022.3 (ou version actuelle)

\- \*\*Render Pipeline\*\* : URP (Universal Render Pipeline) - OBLIGATOIRE

\- \*\*Networking\*\* : Ubiq framework (à installer)

\- \*\*Input\*\* : New Input System (pas le legacy)

\- \*\*Développement\*\* : PC standalone pour l'instant



\## 📁 Structure du projet

```

Assets/

├── Scenes/

│   ├── NormalWorld.unity      # Monde normal (éclairé)

│   └── MirrorWorld.unity       # Monde miroir (sombre)

├── Scripts/

│   ├── Player/                 # Contrôle joueur PC

│   ├── Networking/             # Ubiq sync

│   ├── Objects/                # Objets interactifs

│   └── Managers/               # Game/Scene managers

├── Prefabs/

│   ├── Player/

│   └── NetworkedObjects/

├── Materials/

│   └── Emissive/               # Matériaux émissifs pour monde sombre

└── Settings/

&#x20;   └── InputActions/           # New Input System actions

```



\## 🎮 Contrôles PC actuels

\- \*\*WASD\*\* : Déplacement

\- \*\*Souris\*\* : Rotation caméra

\- \*\*E\*\* : Interagir avec objets

\- \*\*Espace\*\* : Action secondaire

\- \*\*Échap\*\* : Menu pause



\## 🎨 Conventions de code

\- \*\*Namespaces\*\* : MirrorWorld.Player, MirrorWorld.Networking, MirrorWorld.Objects

\- \*\*Private fields\*\* : prefix m\_ (ex: m\_speed)

\- \*\*Commentaires\*\* : en français OK

\- \*\*Code\*\* : en anglais (noms de classes, méthodes, variables)

\- \*\*Networked objects\*\* : héritent de NetworkBehaviour (Ubiq)



\## 🔑 Concepts clés

\- \*\*Synchronisation indirecte\*\* : Les joueurs ne se voient pas, mais leurs actions affectent l'autre monde

\- \*\*Objets liés\*\* : Un levier dans le monde normal contrôle une porte dans le monde miroir

\- \*\*États partagés\*\* : Les objets ont un état synchronisé via Ubiq



\## ⚠️ Problèmes connus à anticiper

\- Shaders émissifs ne fonctionnent qu'avec URP (pas Built-in)

\- New Input System peut conflictuer avec du vieux code

\- Ubiq nécessite une configuration réseau spécifique



\## 📝 Roadmap

\### Phase 1 - PC Prototype (maintenant)

\- \[ ] Setup Ubiq networking

\- \[ ] PlayerController WASD + souris

\- \[ ] Système d'interaction avec objets

\- \[ ] Première synchro objet simple (levier/porte)

\- \[ ] 2 scènes reliées (Normal/Mirror)



\### Phase 2 - VR Migration (avec casque)

\- \[ ] Installer XR Toolkit

\- \[ ] Convertir contrôles en VR

\- \[ ] Adapter interactions pour manettes VR

\- \[ ] Tests sur casque



\### Phase 3 - Gameplay

\- \[ ] Puzzles collaboratifs

\- \[ ] Narratif environnemental

\- \[ ] Polish visuel/audio



\## 🚫 À ignorer par Claude

Library/, Temp/, Logs/, Packages/, UserSettings/, .vs/, .idea/

