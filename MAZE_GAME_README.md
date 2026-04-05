# Smurfs Maze Adventure - Architecture et Modifications

## 📋 Vue d'ensemble des modifications

Le jeu a été transformé d'un jeu de forêt simple en un **jeu de labyrinthe** où le joueur doit naviguer d'un point de départ à un point de fin tout en évitant les ennemis et en collectant des objets.

---

## 🎮 Fonctionnalités principales

### 1. **Système de Labyrinthe** (`Maze.cs`)
- Génère un labyrinthe navigationable avec des murs et des chemins
- **Murs** (marron) : zones inaccessibles
- **Chemins** (vert clair) : zones navigables
- **Point de départ** : coin supérieur gauche
- **Point de fin** : zone indiquée par "END" en jaune

#### Méthodes clés :
```csharp
public bool IsWall(int x, int y)           // Vérifie si une position est un mur
public bool HasWallCollision(x, y, size)   // Détecte collision avec mur
public (int x, int y) StartPoint            // Position de départ
public (int x, int y) EndPoint              // Position d'arrivée
```

### 2. **État du Jeu** (`GameState.cs`)
Ajout de :
- `Maze` : référence au labyrinthe actuel
- `IsGameWon` : flag pour déterminer si le joueur a atteint la fin

### 3. **Moteur de Jeu** (`GameEngine.cs`)
Modifications majeures :

#### Initialisation du Maze
```csharp
public async Task LoadForestAsync(int forestId)
{
    // Crée un nouveau labyrinthe
    var maze = new Maze(GameConstants.ForestWidth, GameConstants.ForestHeight, 40);
    State.Maze = maze;
    
    // Place le Schtroumpf au point de départ
    State.Smurf.PositionX = maze.StartPoint.x;
    State.Smurf.PositionY = maze.StartPoint.y;
}
```

#### Mouvement avec détection de collision
```csharp
public void MoveSmurf(int dx, int dy)
{
    // Vérifier si on n'a pas déjà gagné ou perdu
    if (State.IsGameOver || State.IsGameWon) return;
    
    // Calculer la nouvelle position
    int newX = s.PositionX + dx;
    int newY = s.PositionY + dy;
    
    // Autoriser le mouvement UNIQUEMENT si pas de collision avec mur
    if (!State.Maze.HasWallCollision(newX, newY, GameConstants.CollisionSize))
    {
        s.PositionX = newX;
        s.PositionY = newY;
    }
}
```

#### Condition de victoire
```csharp
private void CheckWinCondition()
{
    var endPoint = State.Maze.EndPoint;
    
    // Vérifier si le joueur a atteint la zone "END"
    if (Overlaps(s.PositionX, s.PositionY, endPoint.x, endPoint.y))
    {
        State.IsGameWon = true;
        State.Score += 1000;
        GameWon?.Invoke();
    }
}
```

#### Placement des ennemis et objets
```csharp
private void PlaceEnemiesInMaze()
{
    // Place chaque ennemi sur un chemin valide du labyrinthe
    // Évite les murs grâce à HasWallCollision
}

private void PlaceItemsInMaze()
{
    // Place les objets (potions, baies) sur des chemins valides
}
```

### 4. **Interface Utilisateur** (`GameForm.cs`)
Modifications majeurs :

#### Affichage du Labyrinthe
```csharp
private void DrawMaze(Graphics g)
{
    // Parcourt la grille du labyrinthe
    // Dessine chaque cellule selon son type (mur/chemin)
}

private void DrawEndPoint(Graphics g)
{
    // Dessine la zone de fin en jaune doré avec "END"
}
```

#### Gestion de la victoire
```csharp
private void OnGameWon()
{
    // Affiche un message de félicitations
    // Affiche le score final
    // Ferme le jeu
}
```

---

## 🕹️ Commandes du jeu

| Touche | Action |
|--------|--------|
| **← →** | Déplacer gauche/droite |
| **↑ ↓** | Déplacer haut/bas |
| **ESC** | Quitter le jeu |

---

## 🎯 Objectif du jeu

1. **Naviguer le labyrinthe** : Du point de départ (haut-gauche) au point de fin (zone jaune "END")
2. **Éviter les ennemis** : Perdre de la vie en les touchant
   - Spider: -20 HP
   - BzzFly: -10 HP
3. **Collecter les objets** : Récupérer de la santé
   - Potion Bleue: +20 HP
   - Potion Rouge: +10 HP
   - Baie: +5 HP
4. **Atteindre la fin** : Gagner +1000 points et voir le message de victoire

---

## 🔧 Architecture du code

```
SmurfsGame/
├── Engine/
│   ├── GameEngine.cs       (Logique du jeu)
│   ├── GameState.cs        (État du jeu)
│   ├── Maze.cs            (Nouvelle: Génération et gestion du labyrinthe)
│   └── GameConstants.cs   (Constantes)
├── Forms/
│   ├── GameForm.cs        (Interface principale, rendue pour le maze)
│   └── MainMenuForm.cs    (Menu principal)
└── UserControls/
    ├── SchtroumpfControl.cs   (Le héros)
    ├── ItemControl.cs         (Objets collectables)
    ├── EnemyControl.cs        (Ennemis)
    └── HealthBarControl.cs    (Barre de santé)
```

---

## 🐛 Problèmes résolus

### Problème 1: Le joueur ne pouvait pas se déplacer
**Cause** : Le système de collision vérifiait les QUATRE coins du sprite, ce qui était trop strict.
**Solution** : Changement pour vérifier uniquement le **centre** du sprite, rendant la collision plus indulgente.

```csharp
// AVANT (trop strict) :
return IsWall(x, y) || IsWall(x + size, y) || 
       IsWall(x, y + size) || IsWall(x + size, y + size);

// APRÈS (indulgent) :
int centerX = x + size / 2;
int centerY = y + size / 2;
return IsWall(centerX, centerY);
```

### Problème 2: Labyrinthe pas assez navigationable
**Cause** : Génération à partir de murs → peu de chemins
**Solution** : Génération à partir de chemins → ajout contrôlé de murs, garantissant des zones accessibles autour du départ et de la fin.

---

## 📊 Variables et constantes clés

```csharp
GameConstants.SmurfStep = 20          // Pixels par mouvement du Schtroumpf
GameConstants.EnemyStep = 4           // Pixels par mouvement des ennemis
GameConstants.CollisionSize = 64      // Taille de la boîte de collision
GameConstants.ForestWidth = 800       // Largeur du labyrinthe
GameConstants.ForestHeight = 560      // Hauteur du labyrinthe
Maze.CellSize = 40                    // Taille d'une cellule de la grille
```

---

## 🎨 Couleurs utilisées

| Élément | Couleur | Code RGB |
|---------|---------|----------|
| Murs | Marron foncé | (60, 30, 30) |
| Chemins | Vert clair | (60, 80, 60) |
| Point END | Jaune doré | (255, 215, 0) |
| Arrière-plan | Vert très foncé | (20, 40, 20) |

---

## 🚀 Prochaines améliorations possibles

- [ ] Ajouter plusieurs niveaux de difficulté
- [ ] Générer des labyrinthes plus complexes avec l'algorithme de Prim/Kruskal
- [ ] Ajouter des bonus temps limité
- [ ] Ajouter des pièges et des portes
- [ ] Système de sauvegarde du meilleur score
- [ ] Animations de victoire/défaite plus élaborées
- [ ] Musique et effets sonores
- [ ] Minuscule minimique de la salle sur le côté

---

## 📝 Notes pour les développeurs

1. **Modification du labyrinthe** : Éditez `Maze.cs > GenerateMaze()` pour changer la génération
2. **Difficultés de collision** : Ajustez `Maze.HasWallCollision()` selon les besoins
3. **Placement des ennemis/objets** : Modifiez `GameEngine.cs > PlaceEnemiesInMaze()` et `PlaceItemsInMaze()`
4. **Visuels** : Modifiez `GameForm.cs > DrawMaze()` et `DrawEndPoint()`

---

**Créé pour l'expérience de jeu Smurfs Maze Adventure** 🍄
