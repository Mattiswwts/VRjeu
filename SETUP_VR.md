# Guide VR + Avatars Ubiq — Action! (Meta Quest)

> Tous les scripts sont déjà écrits. Ce guide = clics dans Unity Editor uniquement.
> Temps estimé : 45 min.

---

## VUE D'ENSEMBLE

```
Scene
├── GameManager          ← RoleSelector + VRRoleMenu (ajouter)
├── NetworkScene (Ubiq)
│   └── AvatarManager   ← à ajouter ici
├── Player_Actor         (desktop, déjà là — laisser)
├── Player_Director      (desktop, déjà là — laisser)
├── Player_Actor_XR      (à créer — XROrigin Acteur)
└── Player_Director_XR   (à créer — XROrigin Réalisateur)
```

---

## ÉTAPE 1 — Créer le XR Rig Acteur

### 1a. Créer l'XR Rig
1. Hierarchy → clic droit → **XR → XR Origin (VR)**
2. Renommer : `Player_Actor_XR`
3. Positionner au même endroit que `Player_Actor` (le point de départ acteur)

### 1b. Composants sur Player_Actor_XR (racine)
Cliquer sur `Player_Actor_XR` → Add Component :
- **ActorControllerXR** (notre script)
- **Player Sync** (notre script Ubiq) — décocher "Is Local" pour l'instant
- **Head And Hands Avatar Input XRI** *(Ubiq.XRI)* — décocher **enabled** pour l'instant
  - Laisser les champs vides (auto-trouvés au runtime)

### 1c. Locomotion acteur (téléportation)
Toujours sur `Player_Actor_XR` → Add Component :
- **Locomotion System**
- **Teleportation Provider**
  - Dans le champ **Locomotion System** → glisse le Locomotion System
- **Snap Turn Provider (Action-based)**

### 1d. Interacteur main droite
1. Déplie `Player_Actor_XR → Camera Offset → Right Controller`
2. Sur `Right Controller` → Add Component → **XR Actor Interactor** (notre script)

### 1e. Sol téléportable
1. Clique sur le sol de la scène acteur (`Floor test` ou similaire)
2. Add Component → **Teleportation Area**
*(Le joueur peut alors se téléporter en appuyant sur le joystick gauche)*

### 1f. Désactiver Player_Actor_XR au départ
- Dans la Hierarchy, clique sur `Player_Actor_XR`
- **Décoche la case** à gauche du nom (SetActive false)

---

## ÉTAPE 2 — Créer le XR Rig Réalisateur

### 2a. Créer l'XR Rig
1. Hierarchy → clic droit → **XR → XR Origin (VR)**
2. Renommer : `Player_Director_XR`
3. Positionner dans la cabine réalisateur (même endroit que `Player_Director`)

### 2b. Composants sur Player_Director_XR (racine)
- **DirectorControllerXR** (notre script)
- **Player Sync** (notre script Ubiq) — décocher "Is Local"
- **Head And Hands Avatar Input XRI** *(Ubiq.XRI)* — décocher **enabled**
- **Snap Turn Provider (Action-based)** *(le réalisateur tourne sur place, pas de téléport)*

### 2c. Interacteur main droite
1. Déplie `Player_Director_XR → Camera Offset → Right Controller`
2. Sur `Right Controller` → Add Component → **XR Director Interactor** (notre script)

### 2d. Désactiver Player_Director_XR au départ
- Décoche la case de `Player_Director_XR` (SetActive false)

---

## ÉTAPE 3 — Avatars Ubiq

### 3a. AvatarManager sur NetworkScene
1. Dans la Hierarchy → trouve **NetworkScene** (le parent Ubiq)
2. Clique dessus → Add Component → **Avatar Manager** *(Ubiq.Avatars)*
3. Champ **Avatar Prefab** → dans le Project panel, cherche `Ubiq Floating Avatar`
   - Chemin : `Packages/Ubiq/Runtime/ExampleAvatars/Floating/Ubiq Floating Avatar`
   - Glisse-le dans le champ

### 3b. Vérifier le define symbol
*Normalement automatique avec XRI 3.0.7, mais si les avatars ne bougent pas :*
- **Edit → Project Settings → Player → Other Settings → Scripting Define Symbols**
- Ajouter : `XRI_3_0_7_OR_NEWER`

### 3c. Résultat attendu
- Chaque joueur voit l'avatar de l'autre (tête + 2 mains flottantes) bouger en temps réel
- L'avatar local est invisible pour soi-même (normal, géré par `AvatarLocalRemoteSwitcher`)

---

## ÉTAPE 4 — Brancher RoleSelector

1. Dans la Hierarchy → clique sur **GameManager**
2. Sur le composant **Role Selector** → remplir les 4 champs :
   - **Actor Player** → glisse `Player_Actor`
   - **Director Player** → glisse `Player_Director`
   - **Actor Player XR** → glisse `Player_Actor_XR`
   - **Director Player XR** → glisse `Player_Director_XR`

3. Toujours sur GameManager → Add Component → **VR Role Menu** (notre script)
   - Laisse les valeurs par défaut (distance=2, height=0)

*En VR : 2 cubes flottants (vert=Acteur, bleu=Réalisateur) apparaissent au démarrage.
En desktop : F1/F2 comme avant.*

---

## ÉTAPE 5 — Boutons et leviers VR

Pour chaque **InteractableButton** (lumière, son, météo, musique) dans la cabine :
1. Clique sur le bouton → Add Component → **XR Simple Interactable**

Pour chaque **Lever** (jour/nuit) :
1. Clique sur le levier → Add Component → **XR Simple Interactable**

*XRDirectorInteractor détecte automatiquement et appelle Press() ou Toggle().*

---

## ÉTAPE 6 — Objets ramassables VR

Pour **tasse café** et **CD** :
1. Clique sur l'objet
2. Add Component → **XR Grab Interactable**
   - Décoche **Throw On Detach**
3. Add Component → **XR Pickup Helper** (notre script)
4. Add Component → **Network Object State** (notre script — sync visibilité)

---

## ÉTAPE 7 — Voice Chat Ubiq

La voix est **déjà incluse** dans le prefab NetworkScene Ubiq (VoipPeer + VoipSpeakerOutput).
Rien à faire si tu utilises le prefab standard.

Si la voix ne marche pas :
- Vérifie que **VoIP Peer** est présent sur NetworkScene
- Sur Quest : Menu Quest → Paramètres → autoriser le micro pour l'app

---

## ÉTAPE 8 — Build Android (Meta Quest)

1. **File → Build Settings → Android → Switch Platform**
2. **Player Settings → Android :**
   - Minimum API Level : Android 10 (API 29)
   - Scripting Backend : IL2CPP
   - Target Architectures : ARM64 uniquement
3. **XR Plug-in Management → Android :** OpenXR coché
4. **OpenXR → Features :** Meta Quest Support activé
5. Brancher le Quest en USB → autoriser le débogage → **Build And Run**

---

## ÉTAPE 9 — Tester à 2 Quests

1. Les 2 Quests sur le **même WiFi**
2. Quest 1 : lancer → viser le cube vert → trigger → **ACTEUR**
3. Quest 2 : lancer → viser le cube bleu → trigger → **RÉALISATEUR**
4. Quest 1 : dans UbiqSetup → **Créer une room** → noter le code
5. Quest 2 : entrer le code → **Rejoindre**
6. Vérifier : les avatars (tête + mains) sont visibles entre les 2 joueurs

---

## TABLEAU RÉCAP — Composants par objet

| GameObject | Composants à ajouter |
|---|---|
| `Player_Actor_XR` | ActorControllerXR, PlayerSync, HeadAndHandsAvatarInputXRI, LocomotionSystem, TeleportationProvider, SnapTurnProvider |
| `Player_Actor_XR → RightHand` | XRActorInteractor |
| `Player_Director_XR` | DirectorControllerXR, PlayerSync, HeadAndHandsAvatarInputXRI, SnapTurnProvider |
| `Player_Director_XR → RightHand` | XRDirectorInteractor |
| `NetworkScene` | AvatarManager (avatarPrefab = Ubiq Floating Avatar) |
| `GameManager` | VRRoleMenu |
| Sol acteur | TeleportationArea |
| Chaque bouton cabine | XRSimpleInteractable |
| Chaque levier cabine | XRSimpleInteractable |
| `tasse café`, `CD` | XRGrabInteractable + XRPickupHelper + NetworkObjectState |

---

## DÉPANNAGE

**Avatar ne bouge pas :**
→ AvatarManager.avatarPrefab assigné ? + les 2 joueurs dans la même room ?

**HeadAndHandsAvatarInputXRI désactivé dans console :**
→ Add Component → XR Input Modality Manager sur `Camera Offset`

**Le rôle ne se sélectionne pas en VR :**
→ Vérifie que Player_Actor_XR et Player_Director_XR sont bien **SetActive false** au départ

**Téléportation ne fonctionne pas :**
→ Sol a un Collider + TeleportationArea + TeleportationProvider sur Player_Actor_XR ?

**Voix pas synchronisée :**
→ VoipPeer sur NetworkScene + permission micro sur Quest autorisée

**Objets (tasse, CD) toujours visibles côté réalisateur après pickup :**
→ NetworkObjectState manquant sur l'objet
