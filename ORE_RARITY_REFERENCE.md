# Space Engineers Ore Rarity & Yield Reference

## Quick Summary

This document contains comprehensive data about ore spawning, mining yields, and refining efficiency in Space Engineers.

---

## Asteroid Spawning System

Asteroids spawn based on **probability weights** defined in `AsteroidGenerators.sbc`. The most common generator (type 4) uses:

- **Asteroid:** 300 weight (base spawn)
- **AsteroidCluster:** 100 weight (3x less common)
- **EncounterAlone:** 15 weight (20x less common)

Each ore type has a **spawn probability multiplier** applied to these base weights.

---

## Complete Ore Rarity Chain

### Mining Yield → Refining Efficiency

| Ore | Asteroid Spawn Multiplier | Ore per Voxel Mined | Ingot per Ore | **Final Ingots per Voxel** | Refine Time (sec) | Rarity Tier |
|-----|---------------------------|-------------------|---------------|---------------------------|-------------------|-------------|
| **Iron** | 5x | 5.0 | 0.7 | **3.5** | 0.05 | Common |
| **Nickel** | 3x | 3.0 | 0.4 | **1.2** | 0.66 | Uncommon |
| **Silicon** | 3x | 3.0 | 0.7 | **2.1** | 0.6 | Uncommon |
| **Cobalt** | 2x | 3.0 | 0.3 | **0.9** | 3.0 | Rare |
| **Magnesium** | 2x | 3.0 | 0.007 | **0.021** | 0.5 | Rare (Useless) |
| **Silver** | 2x | 1.0 | 0.1 | **0.1** | 1.0 | Rare |
| **Gold** | 1x | 1.0 | 0.01 | **0.01** | 0.4 | Very Rare |
| **Platinum** | 1x | 1.0 | 0.005 | **0.005** | 3.0 | Very Rare |
| **Uranium** | 1x | 0.3 | 0.01 | **0.003** | 4.0 | Extremely Rare |

---

## Key Insights

### Most Abundant Resources
1. **Iron** - 3.5 ingots/voxel, 5x spawn rate = easiest to find and refine
2. **Silicon** - 2.1 ingots/voxel, 3x spawn rate = good early game
3. **Nickel** - 1.2 ingots/voxel, 3x spawn rate = essential for components

### Scarcity Tiers
- **Common:** Iron (5x multiplier)
- **Uncommon:** Nickel, Silicon (3x multiplier)
- **Rare:** Cobalt, Magnesium, Silver (2x multiplier)
- **Very Rare:** Gold, Platinum (1x multiplier)
- **Extremely Rare:** Uranium (1x multiplier, 0.3 ore/voxel)

### Uranium Reality Check
- **0.003 ingots per voxel mined**
- Need to mine **333 voxels** to get 1 uranium ingot
- Need to mine **3,330 voxels** to get 10 uranium ingots (1 reactor)
- **1,167x less efficient** than Iron mining

### Magnesium Problem
- Only **0.007 ingots per ore** (worst refining ratio)
- Rarely used in recipes
- Generally considered a waste of mining time

### Precious Metals (Gold/Platinum)
- **0.01 ingots per voxel** (Gold) / **0.005 ingots per voxel** (Platinum)
- Only 1x spawn multiplier (hardest to find)
- Used in specialized high-tech components
- Extremely time-consuming to accumulate

---

## Voxel Material Definitions

### Asteroid Voxel Materials (from VoxelMaterials_asteroids.sbc)

```
Iron_01:        MinedOreRatio=5, IsRare=true, SpawnMultiplier=5
Iron_02:        MinedOreRatio=5, IsRare=true, SpawnMultiplier=5
Nickel_01:      MinedOreRatio=3, IsRare=true, SpawnMultiplier=3
Cobalt_01:      MinedOreRatio=3, IsRare=true, SpawnMultiplier=2
Magnesium_01:   MinedOreRatio=3, IsRare=true, SpawnMultiplier=2
Silicon_01:     MinedOreRatio=3, IsRare=true, SpawnMultiplier=3
Silver_01:      MinedOreRatio=1, IsRare=true, SpawnMultiplier=2
Gold_01:        MinedOreRatio=1, IsRare=true, SpawnMultiplier=1
Platinum_01:    MinedOreRatio=1, IsRare=true, SpawnMultiplier=1
Uraninite_01:   MinedOreRatio=0.3, IsRare=true, SpawnMultiplier=1
```

---

## Refining Blueprints

### Basic Ore to Ingot Conversion

```
Iron Ore:       1 ore → 0.7 ingot (0.05 sec)
Nickel Ore:     1 ore → 0.4 ingot (0.66 sec)
Cobalt Ore:     1 ore → 0.3 ingot (3.0 sec)
Magnesium Ore:  1 ore → 0.007 ingot (0.5 sec)
Silicon Ore:    1 ore → 0.7 ingot (0.6 sec)
Silver Ore:     1 ore → 0.1 ingot (1.0 sec)
Gold Ore:       1 ore → 0.01 ingot (0.4 sec)
Platinum Ore:   1 ore → 0.005 ingot (3.0 sec)
Uranium Ore:    1 ore → 0.01 ingot (4.0 sec)
```

---

## Practical Mining Strategy

### Early Game (First Hour)
- Focus on **Iron** asteroids (easiest to find, fastest to refine)
- Collect **Nickel** for basic components
- Gather **Silicon** for glass and components

### Mid Game (First Day)
- Hunt for **Cobalt** asteroids (needed for advanced components)
- Accumulate **Silver** for better components
- Avoid **Magnesium** (poor yield)

### Late Game (Endgame)
- **Uranium** hunting becomes necessary for reactors
- **Gold/Platinum** for specialized tech
- Consider automated mining operations for Uranium

---

## File References

- **Voxel Materials:** `/Content/Data/VoxelMaterials_asteroids.sbc`
- **Asteroid Generators:** `/Content/Data/AsteroidGenerators.sbc`
- **Refining Blueprints:** `/Content/Data/Blueprints.sbc`

All paths relative to Space Engineers installation directory.

---

## Notes for Modders

If modifying ore yields or spawn rates:
1. Edit `VoxelMaterials_asteroids.sbc` to change `MinedOreRatio` values
2. Edit `AsteroidGenerators.sbc` to change `AsteroidGeneratorSpawnProbabilityMultiplier`
3. Edit `Blueprints.sbc` to change refining efficiency (`Result Amount`)

Changes take effect on world reload.
