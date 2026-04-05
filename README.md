# 🍄 Smurfs Game - Documentation Complète

## 📋 Vue d'ensemble

**Smurfs Game** est un jeu 2D en temps réel développé en **C# avec Windows Forms** et **.NET 8**. Le joueur incarne un Schtroumpf qui doit naviguer dans un labyrinthe forestier, collecter des objets (potions et baies), et combattre des ennemis (araignées et mouches bourdonnantes).

### Objectif du jeu
- Survivre et collecter des objets pour remplir des objectifs
- Éviter/combattre les ennemis
- Gérer votre santé avec les potions
- Compléter le niveau

---

## 🏗️ Architecture du projet

Le projet est divisé en **3 couches** (Architecture 3-Tier) :

### 1️⃣ **SmurfsGame** (UI/Presentation Layer)
- **Responsabilité** : Interface utilisateur et affichage
- **Technologie** : Windows Forms (WinForms)
- **Fichiers clés** :
  - `Program.cs` : Point d'entrée de l'application
  - `Forms/` : Formulaires (MainMenuForm, CharacterSelectionForm, GameForm)
  - `UserControls/` : Composants visuels (sprites des personnages, barres de santé, items)
  - `Engine/` : Logique du moteur de jeu (GameEngine, Maze, GameState)
  - `CharacterType.cs` : Énumération des types de personnages

### 2️⃣ **SmurfsBL** (Business Logic Layer)
- **Responsabilité** : Entités métier et règles du jeu
- **Technologie** : Librairie C# .NET 8
- **Fichiers clés** (dans `entities/`) :
  - `GameConstants.cs` : Constantes du jeu (santé, dégâts, mouvements)
  - `Creature.cs` : Classe de base pour tous les créatures
  - `Schtroumpf.cs` : Le joueur principal (hérite de Creature)
  - `Spider.cs`, `BzzFly.cs` : Ennemis (héritent de Bug → Creature)
  - `Item.cs` : Classe de base pour les objets collectables
  - `RedPotion.cs`, `BluePotion.cs`, `Berry.cs` : Types d'objets
  - `Forest.cs` : Représente une forêt/niveau

### 3️⃣ **SmurfsDAL** (Data Access Layer)
- **Responsabilité** : Accès aux données via base de données
- **Technologie** : Entity Framework Core + SQL Server (LocalDB)
- **Fichiers clés** :
  - `SmurfsDbContext.cs` : Contexte Entity Framework Core
  - `GameRepository.cs` : Classe de dépôt pour accéder aux données
  - `Migrations/` : Migrations de base de données

---

## 🎮 Flux principal du jeu

```
Program.cs (Main)
    ↓
MainMenuForm (Menu principal)
    ↓
CharacterSelectionForm (Sélection du personnage)
    ↓
GameForm (Écran de jeu) ← GameEngine (Logique)
    ↓
Base de données (SmurfsDbContext)
```

### Cycle de jeu (80ms par frame)
1. **Input** : Récupère les touches clavier du joueur
2. **Update** : Déplace le joueur/ennemis, gère les collisions
3. **Render** : Affiche le labyrinthe, le joueur, les ennemis, les objets
4. **Events** : Déclenche les événements (collecte d'objet, dégâts, victoire)

---

## 🎯 Entités principales

### 📊 Hiérarchie des classes

```
Creature (classe de base)
├── Schtroumpf (le joueur) ✨
├── Bug (classe de base pour ennemis)
│   ├── Spider 🕷️
│   └── BzzFly 🐝

Item (classe de base)
├── RedPotion 🔴 (10 HP)
├── BluePotion 🔵 (20 HP)
└── Berry 🫐 (5 HP)

Forest (niveau/map)
```

### Creature (Créature)
- **Propriétés** :
  - `X, Y` : Position sur le labyrinthe
  - `Health` : Points de santé
  - `CreatureType` : Type discriminant pour TPH (Table Per Hierarchy)
- **Contrôle DB** : Stockée dans la table `Creatures` avec colonne `CreatureType`

### Schtroumpf (Joueur)
- **Propriété spéciale** :
  - `IsGrandSchtroumpf` : Booléen indiquant si c'est le Grand Schtroumpf (personnage spécial)
- **Capacités** :
  - Peut se déplacer dans le labyrinthe
  - Peut collecter des objets
  - Peut prendre des dégâts des ennemis
  - Peut mourir si santé = 0

### Bug (Ennemi de base)
- **Classe abstraite** pour ennemis
- **Comportement** :
  - Se déplacent dans les 4 directions cardinales
  - Infligent des dégâts au Schtroumpf
  - Ont un cooldown entre les attaques

### Spider 🕷️ & BzzFly 🐝
- **Spider** :
  - Dégâts : 20 HP
  - Vitesse : 4 pixels/frame
- **BzzFly** :
  - Dégâts : 10 HP
  - Vitesse : 4 pixels/frame

### Item (Objet collectable)
- **Propriétés** :
  - `X, Y` : Position
  - `ItemType` : Type discriminant (RedPotion, BluePotion, Berry)
- **Contrôle DB** : Table `Items` avec colonne `ItemType`

---

## ⚙️ GameConstants - Les chiffres clés

```csharp
// Santé des objets
BluePotionHealth = 20    // Potion bleue → +20 HP
RedPotionHealth  = 10    // Potion rouge → +10 HP
BerryHealth      = 5     // Baie → +5 HP

// Dégâts des ennemis
SpiderDamage     = 20    // Araignée inflige 20 dégâts
BzzFlyDamage     = 10    // Mouche inflige 10 dégâts

// Santé du joueur
MaxHealth        = 100   // Santé maximale
InitialHealth    = 100   // Santé initiale

// Mouvement (en pixels par frame)
SmurfStep        = 15    // Vitesse du Schtroumpf
EnemyStep        = 4     // Vitesse des ennemis (plus lent)

// Collision
CollisionSize    = 28    // Taille du hitbox (28 < 50 pour entrer dans corridors)

// Labyrinthe
ForestWidth      = 800   // Largeur en pixels
ForestHeight     = 560   // Hauteur en pixels
MazeCellSize     = 50    // Chaque cellule = 50×50 pixels

// Cooldown
EnemyHitCooldown = 800   // 800ms entre les dégâts (évite la mort instantanée)
```

---

## 🗄️ Base de données

### Schéma principal

#### Table `Forests`
- `Idf` (PK) : ID unique de la forêt
- Relations avec Creatures et Items

#### Table `Creatures` (TPH)
- `Id` (PK)
- `X, Y` : Coordonnées
- `Health` : Santé actuelle
- **`CreatureType`** (discriminant) : 
  - "Schtroumpf"
  - "Spider"
  - "BzzFly"
- `Idf` (FK) : Référence à la Forest
- `IsGrandSchtroumpf` : Spécifique à Schtroumpf

#### Table `Items` (TPH)
- `Idi` (PK)
- `X, Y` : Coordonnées
- **`ItemType`** (discriminant) :
  - "RedPotion"
  - "BluePotion"
  - "Berry"
- `Idf` (FK) : Référence à la Forest

### Configuration Entity Framework

**TPH (Table Per Hierarchy)** :
- Une seule table pour tous les types
- Colonne `CreatureType` pour différencier les types
- Colonne `ItemType` pour les items
- Avantage : Requêtes simples, relations faciles

---

## 🎮 Mécaniques de jeu

### Mouvement
- **Joueur** : 15 pixels/frame (touches ZQSD ou flèches)
- **Ennemis** : 4 pixels/frame (déplacement automatique dans les 4 directions)
- **Collision** : 28×28 pixels pour éviter de sortir des corridors

### Combat
- Les ennemis infligent des dégâts au toucher
- **Cooldown** : 800ms entre deux attaques du même ennemi
- Le joueur meurt si `Health <= 0`

### Collecte d'objets
- Le joueur collects les objets en les touchant
- Les potions et baies restaurent la santé
- Les objets disparaissent après collecte

### Conditions de victoire/défaite
- **Défaite** : Santé du joueur atteint 0
- **Victoire** : À définir (dépend du niveau)

---

## 🔄 Cycle de mise à jour (80ms)

### GameTimer_Tick (GameForm.cs)

1. **Input** : Lecture des touches (Haut, Bas, Gauche, Droite)
2. **Logique GameEngine** :
   - Déplacer le joueur
   - Déplacer les ennemis
   - Vérifier les collisions avec les items
   - Vérifier les collisions avec les ennemis
   - Gérer le cooldown des dégâts
3. **Mise à jour UI** :
   - Redessiner les sprites
   - Mettre à jour la barre de santé
   - Afficher les messages (bonus, dégâts)

---

## 🎨 Composants visuels (UserControls)

### SchtroumpfControl
- Affiche le sprite du Schtroumpf
- Position synchronisée avec GameState

### EnemyControl
- Affiche le sprite d'un ennemi
- Gère l'animation

### ItemControl
- Affiche le sprite d'un objet collectable

### HealthBarControl
- Barre de santé visuelle
- Mise à jour en temps réel

---

## 📱 Formes principales

### MainMenuForm
- Menu d'accueil
- Boutons : Jouer, Options, Quitter

### CharacterSelectionForm
- Sélection du type de personnage
- Peut-être : Schtroumpf régulier vs Grand Schtroumpf

### GameForm
- Écran principal du jeu
- Affiche le labyrinthe, joueur, ennemis, objets
- Barres de santé et score
- Boutons : Sauvegarder, Retour au menu

---

## 🚀 Comment lancer le jeu

1. **Prérequis** :
   - .NET 8 SDK
   - Visual Studio 2022+ ou VS Code
   - SQL Server (LocalDB)

2. **Étapes** :
   ```powershell
   # Cloner le repo
   git clone https://github.com/maysa-khalsi/SmurfsGame2
   cd SmurfsGame2

   # Restaurer les packages
   dotnet restore

   # Appliquer les migrations (créer la DB)
   dotnet ef database update --project SmurfsDAL

   # Lancer le jeu
   dotnet run --project SmurfsGame
   ```

3. **Ou via Visual Studio** :
   - Ouvrir la solution
   - Set `SmurfsGame` en projet de démarrage (Startup Project)
   - Appuyer sur F5

---

## 🐛 Débogage pendant la démo

### Points clés à vérifier

1. **Démarrage du jeu** :
   ```
   ✓ MainMenuForm s'ouvre
   ✓ Pas d'erreur de connexion DB
   ```

2. **Sélection du personnage** :
   ```
   ✓ CharacterSelectionForm apparaît
   ✓ Peut sélectionner un personnage
   ```

3. **Écran de jeu** :
   ```
   ✓ Labyrinthe s'affiche
   ✓ Joueur visible au centre
   ✓ Ennemis visibles
   ✓ Objets visibles
   ```

4. **Gameplay** :
   ```
   ✓ Mouvement du joueur fluide (ZQSD)
   ✓ Ennemis se déplacent
   ✓ Collecte d'objets fonctionne
   ✓ Barre de santé se met à jour
   ✓ Dégâts des ennemis fonctionnent
   ```

5. **Base de données** :
   ```
   ✓ Pas d'erreur EntityFramework
   ✓ Les données se sauvegardent
   ✓ Migrations appliquées correctement
   ```

### Raccourcis utiles
- **F5** : Lancer le débogage
- **Ctrl+Shift+B** : Compiler
- **Ctrl+Maj+Alt+I** : Fenêtre Immédiate (console débogage)
- **Ctrl+Alt+L** : Solution Explorer

---

## 📋 Checklist avant la démo

- [ ] Solution compile sans erreur
- [ ] Base de données créée (migrations appliquées)
- [ ] Jeu lance sans crash
- [ ] Tous les sprites s'affichent
- [ ] Mouvement du joueur fonctionne
- [ ] Ennemis se déplacent
- [ ] Collecte d'objets fonctionne
- [ ] Système de dégâts fonctionne
- [ ] Barre de santé se met à jour
- [ ] Pas d'erreurs dans la console d'erreur
- [ ] Performance fluide (60 FPS idéal)

---

## 🔧 Architecture technique résumée

| Couche | Rôle | Technologie |
|--------|------|-------------|
| **UI/Presentation** | Affichage, input utilisateur | Windows Forms (.NET 8) |
| **Business Logic** | Entités, règles du jeu | C# Classes |
| **Data Access** | Persistance, DB | Entity Framework Core 8 |
| **Database** | Stockage permanent | SQL Server (LocalDB) |

---

## 🎓 Concepts clés expliqués

### Labyrinthe (Maze)
- Grille de 50×50 pixels par cellule
- Le joueur et ennemis doivent rester dans les corridors
- Collision size (28px) < Cell size (50px) = 22px de marge

### Cooldown système
- **800ms** entre deux attaques du même ennemi
- Évite que le joueur meure en < 1 seconde

### Pattern TPH (Table Per Hierarchy)
- Toutes les Creatures dans une table
- Colonne discriminante pour le type
- Avantage : Requêtes efficaces

### Event-Driven
- `ItemCollected`, `EnemyHit`, `GameOver`, `GameWon`
- Permet une séparation clean UI ↔ Logique

---

## 📌 Notes supplémentaires

- **Performance** : 80ms par frame = ~12.5 FPS (normal pour WinForms)
- **Sprites** : Utilisent les UserControls WinForms
- **Audio** : À implémenter (pas encore dans le projet)
- **Score** : À implémenter complètement
- **Sauvegarde** : Structure en place mais à finir

---

## ✨ Résumé pour la démo

Tu as créé un **jeu complet avec architecture 3-tier** :
1. **UI modulaire** avec Windows Forms
2. **Logique métier** bien séparée
3. **Persistance DB** avec Entity Framework
4. **Système de combat** fluide
5. **Gestion d'énergie** (santé/potions)

**Le cœur du jeu** : Un Schtroumpf qui doit survivre dans un labyrinthe plein d'ennemis et collecter des ressources. Bonne chance pour ta démo ! 🍄

---

**Dernière mise à jour** : 2025  
**Branche** : main  
**Repo** : https://github.com/maysa-khalsi/SmurfsGame2
