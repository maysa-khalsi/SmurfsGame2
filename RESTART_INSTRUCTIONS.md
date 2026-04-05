# Étapes pour relancer le jeu avec le nouveau Maze

## 🛑 Avant de continuer

1. **Arrêtez l'application Visual Studio** (Shift+F5 ou le bouton Stop)
2. **Relancez le jeu** (F5) pour appliquer les changements

## 🎯 Qu'est-ce qui a changé ?

### ✅ Améliorations principales

1. **Algorithme de génération du Maze**
   - **Avant** : Placement aléatoire de murs → Labyrinthe chaotique et bloqué
   - **Après** : Algorithme "Recursive Backtracking" → Vrai labyrinthe avec chemin clair du départ à la fin

2. **Taille des cellules**
   - **Avant** : cellSize = 40px → peu de précision
   - **Après** : cellSize = 20px → meilleure résolution et navigation

3. **Taille de collision**
   - **Avant** : CollisionSize = 64px → trop gros, bloque partout
   - **Après** : CollisionSize = 32px → taille idéale pour naviguer

4. **Vitesse de mouvement**
   - SmurfStep : 20 → 10 (plus de contrôle)
   - EnemyStep : 4 → 3 (moins d'agressivité)

### 📐 Algorithme Recursive Backtracking expliqué

```
1. Remplir la grille de MURS
2. Commencer à la cellule (1, 1)
3. Marquer cette cellule comme CHEMIN
4. Pour chaque direction aléatoire (haut, bas, gauche, droite):
   - Si la cellule voisine est un mur et non visitée:
     a. Creuser un passage entre les deux cellules
     b. Creuser la cellule voisine (récursif)
     c. Continuer vers la cellule voisine
5. Résultat : Labyrinthe connexe avec un chemin unique
```

### 🔍 Résultats attendus

- ✅ Vous pouvez naviguer librement dans le labyrinthe
- ✅ Pas de zones bloquées
- ✅ Un chemin clair du départ (coin haut-gauche) à la fin (zone END)
- ✅ Les ennemis peuvent naviguer sans être bloqués
- ✅ Les items sont placés sur des chemins accessibles

## 🚀 Comment redémarrer

### Option 1 : Redémarrage normal
```
Appuyez sur: Ctrl+Shift+F5  (Arrêter et redémarrer le débogage)
```

### Option 2 : Arrêter puis relancer
```
1. Appuyez sur: Shift+F5   (Arrêter)
2. Appuyez sur: F5         (Relancer)
```

### Option 3 : Fermer Visual Studio
```
1. Fermer le jeu
2. Fermer VS
3. Rouvrir le projet
4. Appuyez sur: F5
```

---

## 📊 Paramètres clés du Maze

```csharp
// Dans GameConstants.cs
public const int MazeCellSize  = 20;       // Taille d'une cellule (20x20 pixels)
public const int ForestWidth   = 800;      // Grille = 40x28 cellules
public const int ForestHeight  = 560;      // Grille = 40x28 cellules

// Résolution du Maze
Colonnes = 800 / 20 = 40 (toujours impair)
Lignes   = 560 / 20 = 28 → 29 (forcé à impair pour algo)
```

## 🎮 Test rapide

Après redémarrage, testez ceci :

1. **Navigation** : Appuyez sur les flèches - vous devriez pouvoir vous déplacer librement
2. **Items** : Collectez les baies rouges/bleues pour gagner de la santé
3. **Ennemis** : Évitez les araignées et les mouches
4. **Objectif** : Atteindrez la zone "END" jaune en bas-droite
5. **Victoire** : Un message de félicitations s'affiche

---

## ❓ Problèmes possibles

### "L'écran se bloque encore"
→ Assurez-vous d'avoir complètement fermé l'application avant F5

### "Le labyrinthe est vide"
→ Relancez - la génération est aléatoire à chaque démarrage

### "Je ne peux pas atteindre la fin"
→ C'est impossible - l'algorithme garantit un chemin connexe
→ Explorez plus loin, il y a toujours une route

---

**Bon jeu ! 🎮**
