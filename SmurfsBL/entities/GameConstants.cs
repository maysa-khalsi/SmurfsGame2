namespace Smurfs.SmurfsBL.entities;

public static class GameConstants
{
    // ── Item health values ────────────────────────────────────────────────────
    public const int BluePotionHealth = 20;
    public const int RedPotionHealth  = 10;
    public const int BerryHealth      = 5;

    // ── Enemy damage ──────────────────────────────────────────────────────────
    public const int SpiderDamage  = 20;
    public const int BzzFlyDamage  = 10;

    // ── Player health ─────────────────────────────────────────────────────────
    public const int MaxHealth     = 100;
    public const int InitialHealth = 100;

    // ── Movement ──────────────────────────────────────────────────────────────
    // SmurfStep MUST be a divisor of MazeCellSize so movement aligns with corridors
    public const int SmurfStep    = 15;    // 50 / 5 = 10 key-presses to cross one cell
    public const int EnemyStep    = 4;    // slower than smurf, fair challenge

    // ── Collision ─────────────────────────────────────────────────────────────
    // Must be SMALLER than MazeCellSize so the sprite fits in a corridor
    public const int CollisionSize = 28;  // 28 < 50 → 22px of clearance in corridors

    // ── Forest / Maze ─────────────────────────────────────────────────────────
    public const int ForestWidth   = 800;
    public const int ForestHeight  = 560;
    public const int MazeCellSize  = 50;  // 50×50 px per cell → wide corridors

    // ── Enemy hit cooldown (ms) ───────────────────────────────────────────────
    // Prevents continuous damage every 80 ms — player would die in <1 second otherwise
    public const int EnemyHitCooldown = 800; // ms between successive damage from same enemy
}
