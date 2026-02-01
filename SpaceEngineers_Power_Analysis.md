# Space Engineers Power Yield Analysis
## Power Generation from 1 Voxel of Ore

This analysis calculates how much power (in kWh) can be generated from 1 voxel of each ore type through different conversion chains.

---

## Key Data Points

### Voxel Ore Yields (Actual MinedOreRatio values)
- **Uranium Voxel** → 0.3 Uranium Ore (MinedOreRatio = 0.3)
- **Nickel Voxel** → 3 Nickel Ore (MinedOreRatio = 3)
- **Ice Voxel** → 5 Ice Ore (MinedOreRatio = 5)

### Ore Processing Yields
- **Uranium Ore** → 0.01 Uranium Ingot (1 ore = 0.01 ingot)
- **Nickel Ore** → 0.4 Nickel Ingot (1 ore = 0.4 ingot)
- **Ice** → 20 Hydrogen (IceToGasRatio in OxygenGenerator) + 10 Oxygen

### Power Generation
- **Uranium Ingot** → 1 MWh (1000 kWh) per ingot in Reactor
- **Hydrogen** → 0.001556 MWh per liter (EnergyDensity in GasProperties)
- **Hydrogen Engine** → 5 MW output (Large) or 0.5 MW output (Small)
- **Prototech Fusion Reactor** → 400 MW output (runs on Hydrogen)

### Nickel → Power Cell → Battery Chain
- **Nickel Ingot** → Power Cell requires: 10 Iron + 1 Silicon + 2 Nickel ingots
- **Power Cell** → Battery component (used to build batteries)
- **Battery** → Stores 3 MWh (Large) or 1 MWh (Small) with 30% initial charge
- **Battery Cycle** → Create power cells → Weld battery → Battery has power → Discharge → Grind down → Lose power cells → Repeat

---

## Calculation Results

### Path 1: Uranium (Direct Power Generation)
```
1 Uranium Voxel
  → 0.3 Uranium Ore (MinedOreRatio = 0.3)
  → 0.3 × 0.01 = 0.003 Uranium Ingot
  → 0.003 MWh = 3 kWh per voxel
```

**Result: 3 kWh per Uranium voxel**

---

### Path 2: Nickel (Ore → Ingot → Power Cells → Batteries → Power)
```
1 Nickel Voxel
  → 3 Nickel Ore (MinedOreRatio = 3)
  → 3 × 0.4 = 1.2 Nickel Ingot
```

**Nickel Battery Cycle Analysis:**

A Large Battery requires:
- **80 Power Cells** (limiting factor)
- 20 Steel Plate
- 10 Construction
- 25 Computer
- 20 Construction
- 60 Steel Plate

Each Power Cell requires: 2 Nickel Ingots

**Resource Requirements per Battery:**
- 80 Power Cells × 2 Nickel per cell = **160 Nickel Ingots needed**
- 160 Nickel Ingots ÷ 1.2 per voxel = **~133 Nickel Voxels needed**

**Energy Yield per Battery:**
- 1 Large Battery = 3 MWh max capacity
- Initial charge = 3 × 0.3 = 0.9 MWh = 900 kWh

**Per Voxel Calculation:**
- 900 kWh ÷ 133 voxels = **~6.8 kWh per Nickel voxel**

**Result: ~6.8 kWh per Nickel voxel (via battery cycle)**

**Note:** While theoretically sustainable, this requires enormous amounts of nickel ore to build even a single battery. The battery cycle works, but it's not an efficient power source compared to ice.

---

### Path 3: Ice → Hydrogen → Power Generation
```
1 Ice Voxel
  → 5 Ice Ore (MinedOreRatio = 5)
  → 5 × 20 = 100 Liters of Hydrogen (IceToGasRatio = 20 in OxygenGenerator)
  → 100 × 0.001556 MWh = 0.1556 MWh = 155.6 kWh per voxel (Hydrogen Engine)
```

**Result: 155.6 kWh per Ice voxel (via Hydrogen Engine)**

---

### Path 4: Ice → Hydrogen → Fusion Reactor
```
1 Ice Voxel
  → 5 Ice Ore (MinedOreRatio = 5)
  → 5 × 20 = 100 Liters of Hydrogen (IceToGasRatio = 20 in OxygenGenerator)
  → Prototech Fusion Reactor: 400 MW output
```

**Note:** The Fusion Reactor consumes hydrogen at a rate determined by FuelProductionToCapacityMultiplier (0.4).
With 100 liters of hydrogen and the reactor's fuel consumption rate, this would provide continuous power output.

**Estimated result: 155.6 kWh per Ice voxel (same as Hydrogen Engine, but with 400 MW peak output)**

---

## Summary Table by Power Generation Method

### Ice Ore (5 ore per voxel → 100 L Hydrogen)
| Method | Peak Power Output | Consumption Rate | Runtime from 100L | Energy per Voxel | Notes |
|--------|-------------------|------------------|-------------------|------------------|-------|
| **Fusion Reactor** | 400 MW | 1000 L/s | 0.1 seconds | **11.11 kWh** ⭐ | (0.1 s × 400 MW) ÷ 3600 s/h |
| **Hydrogen Engine** | 5 MW | 500 L/s | 0.2 seconds | **0.2778 kWh** | (0.2 s × 5 MW) ÷ 3600 s/h |

### Uranium Ore (0.3 ore per voxel → 0.003 ingot)
| Method | Power Output per Voxel | Notes |
|--------|------------------------|-------|
| **Reactor** | 3 kWh | 1 MWh/ingot × 0.003 ingot |

### Nickel Ore (3 ore per voxel → 1.2 ingot)
| Method | Power Output per Voxel | Notes |
|--------|------------------------|-------|
| **Battery Cycle** | ~6.8 kWh | Requires 133 voxels to build one battery |

### Overall Efficiency Ranking (Energy per Voxel)
| Rank | Method | Ore Type | Energy per Voxel | Calculation |
|------|--------|----------|-----------------|-------------|
| 1 | **Fusion Reactor** | Ice | **11.11 kWh** ⭐ | (100 L ÷ 1000 L/s) × 400 MW ÷ 3600 s/h |
| 2 | **Battery Cycle** | Nickel | **~6.8 kWh** | 0.9 MWh ÷ 133 voxels |
| 3 | **Reactor** | Uranium | **3 kWh** | 1 MWh/ingot × 0.003 ingot |
| 4 | **Hydrogen Engine** | Ice | **0.2778 kWh** | (100 L ÷ 500 L/s) × 5 MW ÷ 3600 s/h |

---

## Important Notes

1. **Ice is the most efficient** at 155.6 kWh per voxel when converted to hydrogen
2. **Nickel is second** at ~6.8 kWh per voxel through the battery cycle (requires 133 voxels to build one battery)
3. **Uranium is the least efficient** at only 3 kWh per voxel
4. The "nickel as electricity" concept DOES work through a sustainable battery cycle:
   - Create power cells from nickel ingots (2 Ni per cell)
   - Weld batteries using power cells (80 cells per large battery)
   - Batteries spawn with 30% charge (0.9 MWh for large battery)
   - Discharge the battery to use the power
   - Grind down the battery to recover materials
   - Repeat the cycle - the power cells are consumed but the energy was extracted
   - **However:** Building a single large battery requires ~133 nickel voxels worth of ore, making it impractical as a primary power source

---

## Efficiency Ranking (Power per Voxel)

1. **Ice: 155.6 kWh** ⭐ Best renewable option (hydrogen conversion)
2. **Nickel: ~6.8 kWh** ✓ Works but requires massive resource investment
3. **Uranium: 3 kWh** ✗ Least efficient

---

## Additional Context

- **MinedOreRatio** values are: Uranium = 0.3, Nickel = 3, Ice = 5
- **Hydrogen efficiency** is 0.001556 MWh per liter
- **Prototech Fusion Reactor** outputs 400 MW and runs on hydrogen (FuelProductionToCapacityMultiplier = 0.4)
- **Standard Hydrogen Engine** outputs 5 MW (Large) or 0.5 MW (Small)
- **Battery initial charge** is 30% of max capacity (0.9 MWh for large battery = 900 kWh)
- The nickel battery cycle is sustainable because you're not losing the energy - you're extracting it from the battery's initial charge
