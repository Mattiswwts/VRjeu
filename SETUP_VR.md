# Guide Migration VR — Action! (Meta Quest 2/3)

## Vue d'ensemble

Ce guide transforme le prototype desktop en jeu VR pour 2x Meta Quest.
Les scripts VR sont déjà écrits — tu dois juste configurer Unity Editor.

---

## ÉTAPE 1 — Importer les packages XRI (déjà dans manifest.json)

À l'ouverture du projet, Unity va télécharger automatiquement :
- XR Interaction Toolkit 3.0.7
- OpenXR Plugin 1.13.0
- XR Management 4.5.0

**Si Unity affiche un popup "TMP Importer"** → clique "Import TMP Essentials"
**Si Unity affiche "XRI Default Input Actions"** → clique "Import" pour avoir les contrôles de base

⚠️ Si tu as des erreurs de version :
- Window → Package Manager → cherche "XR Interaction Toolkit" → mets à jour

---

## ÉTAPE 2 — Configurer OpenXR pour Meta Quest

1. **Edit → Project Settings → XR Plug-in Management**
2. Onglet **Android** (icône robot) → coche **OpenXR**
3. Onglet **PC** → coche aussi **OpenXR** (pour tester avec Link)
4. Clique sur l'icône ⚠️ jaune si elle apparaît → "Fix All"

5. Toujours dans XR Plug-in Management → clique **OpenXR** (sous Android)
6. Dans **Interaction Profiles** → clique **+** → ajoute :
   - **Meta Quest Touch Pro Controller Profile**
   - (ou "Oculus Touch Controller Profile" si le premier n'est pas là)
7. Dans **OpenXR Feature Groups** → active **Meta Quest Support**

---

## ÉTAPE 3 — Remplacer Player_Actor par un XR Rig

### Supprimer l'ancien Player_Actor
1. Dans la Hierarchy → clique sur **Player_Actor**
2. Note sa position (Transform) pour la réutiliser
3. **Ne supprime pas encore** — d'abord crée le nouveau

### Créer le XR Rig Acteur
1. Dans la Hierarchy → clic droit → **XR → XR Origin (VR)**
   - Unity crée automatiquement : XR Origin + Camera Offset + Main Camera + Left/Right Controller
2. Renomme-le `Player_Actor_XR`
3. Replace-le à la même position que l'ancien Player_Actor

### Ajouter les composants gameplay
4. Clique sur **Player_Actor_XR** (le parent)
5. **Add Component → ActorControllerXR**
6. **Add Component → Networked Player Sync** (s'il existe dans ta scène)
   - Vérifie que PlayerSync / NetworkedPlayerSync est aussi là

### Configurer la téléportation (locomotion Acteur)
7. Sur **Player_Actor_XR** → **Add Component → Locomotion System**
8. Sur **Player_Actor_XR** → **Add Component → Teleportation Provider**
   - Dans Locomotion System → assigne le **Locomotion System** que tu viens d'ajouter
9. Sur **Player_Actor_XR** → **Add Component → Snap Turn Provider (Action-based)**

### Configurer le Ray Interactor Acteur (main droite)
10. Déplie **Player_Actor_XR → Camera Offset → Right Controller**
11. Clique sur **Right Controller**
12. Il devrait déjà avoir **XR Ray Interactor** — sinon Add Component → XR Ray Interactor
13. **Add Component → XRActorInteractor** (notre script)

### Supprimer l'ancien Player_Actor
14. Maintenant supprime l'ancien **Player_Actor** de la Hierarchy

---

## ÉTAPE 4 — Remplacer Player_Director par un XR Rig

### Créer le XR Rig Réalisateur
1. Hierarchy → clic droit → **XR → XR Origin (VR)**
2. Renomme-le `Player_Director_XR`
3. Place-le dans la cabine réalisateur (même position que l'ancien Player_Director)

### Ajouter les composants
4. Sur **Player_Director_XR** → **Add Component → DirectorControllerXR**
5. Sur **Player_Director_XR** → **Add Component → Locomotion System**
6. Sur **Player_Director_XR** → **Add Component → Snap Turn Provider (Action-based)**
   - Le réalisateur ne se déplace PAS, il tourne juste sur lui-même

### Configurer le Ray Interactor Réalisateur
7. Déplie **Player_Director_XR → Camera Offset → Right Controller**
8. Sur **Right Controller** → **Add Component → XRDirectorInteractor** (notre script)

### Supprimer l'ancien Player_Director
9. Supprime l'ancien **Player_Director**

---

## ÉTAPE 5 — Configurer la téléportation dans la scène

L'acteur se téléporte pour se déplacer. Il faut marquer les zones où il peut aller.

1. Clique sur le sol de ta scène principale (**Floor test** ou similaire)
2. **Add Component → Teleportation Area**
3. Ça c'est tout — le raycast de la manette gauche affichera une courbe
   et l'acteur pourra se téléporter en appuyant sur le joystick gauche

⚠️ Si la téléportation ne marche pas : vérifie que le sol a un **Collider** (Box Collider ou Mesh Collider)

---

## ÉTAPE 6 — Rendre les boutons et leviers cliquables en VR

Les boutons du réalisateur et les leviers doivent être "sélectionnables" par le ray interactor.

**Pour chaque InteractableButton dans la cabine :**
1. Clique sur le GameObject du bouton
2. **Add Component → XR Simple Interactable**
3. C'est tout — XRDirectorInteractor détecte et appelle Press() automatiquement

**Pour chaque Lever :**
1. Clique sur le GameObject du levier
2. **Add Component → XR Simple Interactable**
3. C'est tout — XRDirectorInteractor détecte et appelle Toggle() automatiquement

---

## ÉTAPE 7 — Rendre les objets ramassables en VR

Pour chaque objet que l'acteur doit prendre (tasse café, CD, musicbox) :

1. Clique sur l'objet
2. **Add Component → XR Grab Interactable**
3. **Add Component → XR Pickup Helper** (notre script — envoie l'action réseau)
4. **Add Component → Network Object State** (notre script — synchro la disparition chez le réalisateur)
5. Sur XR Grab Interactable → décoche **Throw On Detach** (pour éviter que l'objet vole)
6. Assure-toi que l'objet a un **Rigidbody** (XR Grab Interactable en ajoute un automatiquement)

⚠️ Sans **Network Object State**, l'objet disparaît côté acteur mais reste visible côté réalisateur !

---

## ÉTAPE 8 — HUD Réalisateur en World Space (déjà fait)

L'écran réalisateur (`ecran_réalisateur`) est déjà en World Space Canvas — ça marche en VR directement.

Pour l'acteur : il n'a pas d'écran (game design intentionnel — il doit communiquer avec le réalisateur).

---

## ÉTAPE 9 — Build Settings pour Meta Quest

### Passer en Android
1. **File → Build Settings**
2. Plateforme → **Android** → **Switch Platform** (ça prend quelques minutes)

### Player Settings
3. Dans Build Settings → clique **Player Settings**
4. Onglet **Android** :
   - **Minimum API Level** : Android 10.0 (API level 29)
   - **Target API Level** : Automatic
   - **Scripting Backend** : IL2CPP
   - **Target Architectures** : coche seulement **ARM64**
5. **Other Settings → Color Space** : Linear (déjà en URP normalement)

### Activer le mode VR Android
6. **Edit → Project Settings → XR Plug-in Management → Android** : OpenXR doit être coché (fait à l'étape 2)

### Configurer pour Meta Quest spécifiquement
7. **Edit → Project Settings → OpenXR (Android)**
8. Vérifie que **Meta Quest Support** est activé dans les features

### Builder
9. **File → Build Settings → Build**
10. Connecte le Quest en USB → autorise le débogage sur le casque
11. Ou : Build And Run pour installer directement

---

## ÉTAPE 10 — Tester avec 2 Quests

### Sur les 2 casques :
- Les 2 doivent être sur le **même réseau WiFi**
- Lance le jeu sur le Quest 1 → crée une room → note le Join Code
- Lance sur le Quest 2 → rejoins avec le code

### Si la connexion ne marche pas :
- Vérifie que le serveur Ubiq `nexus.cs.ucl.ac.uk:8009` est accessible depuis le réseau
- En fallback : **Solo** pour tester le gameplay seul

---

## Résumé des composants ajoutés par objet

| GameObject | Composants à ajouter |
|---|---|
| Player_Actor_XR | ActorControllerXR, LocomotionSystem, TeleportationProvider, SnapTurnProvider |
| Player_Actor_XR → RightHand | XRActorInteractor |
| Player_Director_XR | DirectorControllerXR, LocomotionSystem, SnapTurnProvider |
| Player_Director_XR → RightHand | XRDirectorInteractor |
| Sol (scène acteur) | TeleportationArea |
| Chaque bouton cabine | XRSimpleInteractable |
| Chaque levier cabine | XRSimpleInteractable |
| Tasse café, CD, musicbox | XRGrabInteractable + XRPickupHelper + NetworkObjectState |

---

## En cas de problème

**"XRBaseInteractor introuvable" à la compilation :**
→ Window → Package Manager → XR Interaction Toolkit → vérifier version (doit être 3.0+)

**Les mains ne bougent pas dans le casque :**
→ Project Settings → XR Plug-in Management → OpenXR → vérifie les Interaction Profiles

**La téléportation ne fonctionne pas :**
→ Vérifie que TeleportationProvider est bien sur le même objet que LocomotionSystem
→ Le sol doit avoir un Collider et un TeleportationArea

**Le réalisateur voit la scène acteur (mauvaise position) :**
→ Vérifie que Player_Director_XR est bien positionné dans la cabine

**Ubiq ne synchro plus les positions :**
→ Vérifie que PlayerSync / NetworkedPlayerSync est sur les 2 XROrigins
→ Ou : réassigne le composant depuis Network Scene dans l'Inspector

---

## BONUS — Avatars Ubiq (requis pour l'examen)

Ubiq fournit un avatar "tête + mains flottantes" prêt à l'emploi.

### 1. Ajouter AvatarManager sur NetworkScene
1. Dans la Hierarchy → trouve **NetworkScene** (le parent Ubiq)
2. Clique dessus → **Add Component → Avatar Manager** (namespace Ubiq.Avatars)
3. Dans **Avatar Prefab** → assigne le prefab : `Packages/Ubiq/Runtime/ExampleAvatars/Floating/Ubiq Floating Avatar`
   - Via Project window : cherche "Ubiq Floating Avatar" dans la barre de recherche

### 2. Ajouter l'input XRI pour les avatars
Sur **chaque XR Rig** (Player_Actor_XR ET Player_Director_XR) :
1. Clique sur le XR Origin parent
2. **Add Component → Head And Hands Avatar Input XRI** (namespace Ubiq.XRI)
3. Le champ **Avatar Manager** se remplit automatiquement (si pas : glisse le NetworkScene)
4. Le champ **XR Origin Game Object** → glisse le XR Origin lui-même

### 3. Vérifier le define symbol
Le script `HeadAndHandsAvatarInputXRI` requiert `XRI_3_0_7_OR_NEWER` dans les defines.
Unity le définit normalement automatiquement avec XRI 3.0.7.

Pour vérifier/ajouter manuellement :
- **Edit → Project Settings → Player → Other Settings → Scripting Define Symbols**
- Ajoute `XRI_3_0_7_OR_NEWER` si absent

### 4. Résultat attendu
- Chaque joueur voit l'avatar de l'autre (tête + 2 mains) se déplacer en temps réel
- L'avatar local est invisible pour soi-même (normal — c'est géré par AvatarLocalRemoteSwitcher)
- La voix Ubiq (VoipPeer) ajoute l'indicateur de parole sur l'avatar automatiquement

### Dépannage avatar
**"HeadAndHandsAvatarInputXRI désactivé" dans la console :**
→ Pas d'XRInputModalityManager enfant du XR Rig — Add Component → XR Input Modality Manager sur Camera Offset

**L'avatar ne bouge pas :**
→ Vérifie que AvatarManager.avatarPrefab est assigné
→ Vérifie que les 2 clients sont dans la même room Ubiq
