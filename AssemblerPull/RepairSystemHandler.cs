using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
        /// <summary> 
///    Class to handle the RepairSystems 
/// </summary> 
public class RepairSystemHandler : EntityHandler<IMyShipWelder>
{
    private Program _Program;
    public void SetProgram(Program program)
    {
        _Program = program;
    }


    private Func<IEnumerable<long>, VRage.Game.MyDefinitionId, int, int> _EnsureQueued;
    private Func<IMyProjector, Dictionary<VRage.Game.MyDefinitionId, VRage.MyFixedPoint>, int> _NeededComponents4Blueprint;
    /// <summary> 
    /// The block clases the system distinguish 
    /// </summary> 
    public enum BlockClass
    {
        AutoRepairSystem = 1,
        ShipController,
        Thruster,
        Gyroscope,
        CargoContainer,
        Conveyor,
        ControllableGun,
        PowerBlock,
        ProgrammableBlock,
        Projector,
        FunctionalBlock,
        ProductionBlock,
        Door,
        ArmorBlock
    }

    /// <summary> 
    /// The componet classes the system distinguish 
    /// </summary> 
    public enum ComponentClass
    {
        Material = 1,
        Ingot,
        Ore,
        Stone,
        Gravel
    }

    /// <summary> 
    /// The search modes supported by the block 
    /// </summary> 
    public enum SearchModes
    {
        Grids = 0x0001,
        BoundingBox = 0x0002
    }

    /// <summary> 
    /// The work modes supported by the block 
    /// </summary> 
    public enum WorkModes
    {
        /// <summary> 
        /// Grind only if nothing to weld 
        /// </summary> 
        WeldBeforeGrind = 0x0001,

        /// <summary> 
        /// Weld onyl if nothing to grind 
        /// </summary> 
        GrindBeforeWeld = 0x0002,

        /// <summary> 
        /// Grind only if nothing to weld or 
        /// build waiting for missing items 
        /// </summary> 
        GrindIfWeldGetStuck = 0x0004,

        /// <summary> 
        /// Only welding is allowed 
        /// </summary> 
        WeldOnly = 0x0008,

        /// <summary> 
        /// Only grinding is allowed 
        /// </summary> 
        GrindOnly = 0x0010
    }

    /// <summary> 
    /// Block/Component class and it's state 
    /// </summary> 
    public class ClassState<T> where T : struct
    {
        public T ItemClass { get; }
        public bool Enabled { get; }
        public ClassState(T itemClass, bool enabled)
        {
            ItemClass = itemClass;
            Enabled = enabled;
        }
    }

    /// <summary> 
    /// Set the Help Others state 
    /// </summary> 
    public bool HelpOther
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].HelpOthers : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.HelpOthers = value;
        }
    }

    /// <summary> 
    /// Set AllowBuild (projected blocks) 
    /// </summary> 
    public bool AllowBuild
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.AllowBuild") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.AllowBuild", value);
        }
    }

    /// <summary> 
    /// Set the search mode of the block 
    /// </summary> 
    public SearchModes SearchMode
    {
        get
        {
            return _Entities.Count > 0 ? (SearchModes)GetValue<long>("BuildAndRepair.Mode") : SearchModes.Grids;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValue<long>("BuildAndRepair.Mode", (long)value);
        }
    }

    /// <summary> 
    /// Set the search mode of the block 
    /// </summary> 
    public WorkModes WorkMode
    {
        get
        {
            return _Entities.Count > 0 ? (WorkModes)GetValue<long>("BuildAndRepair.WorkMode") : WorkModes.WeldBeforeGrind;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValue<long>("BuildAndRepair.WorkMode", (long)value);
        }
    }

    /// <summary> 
    /// Enable/Disable the use of the Ignore Color 
    /// If enabled block's with with color 'IgnoreColor' 
    /// will be ignored. 
    /// You could use this do have intentionally unweldet block's 
    /// and still use autorepair of the rest. 
    /// </summary> 
    public bool UseIgnoreColor
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.UseIgnoreColor") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.UseIgnoreColor", value);
        }
    }

    /// <summary> 
    /// Set the ignore color 
    /// X=Hue         0 .. 1 -> * 360 -> Displayed value 
    /// Y=Saturation -1 .. 1 -> * 100 -> Displayed value 
    /// Z=Value      -1 .. 1 -> * 100 -> Displayed value 
    /// </summary> 
    public Vector3 IgnoreColor
    {
        get
        {
            return _Entities.Count > 0 ? GetValue<Vector3>("BuildAndRepair.IgnoreColor") : Vector3.Zero;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValue<Vector3>("BuildAndRepair.IgnoreColor", value);
        }
    }

    /// <summary> 
    /// Enable/Disable the use of the Grind Color 
    /// If enabled block's with with color 'GrindColor' 
    /// will be grinded. 
    /// </summary> 
    public bool UseGrindColor
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.UseGrindColor") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.UseGrindColor", value);
        }
    }

    /// <summary> 
    /// Set the grind color 
    /// X=Hue         0 .. 1 -> * 360 -> Displayed value 
    /// Y=Saturation -1 .. 1 -> * 100 -> Displayed value 
    /// Z=Value      -1 .. 1 -> * 100 -> Displayed value 
    /// </summary> 
    public Vector3 GrindColor
    {
        get
        {
            return _Entities.Count > 0 ? GetValue<Vector3>("BuildAndRepair.GrindColor") : Vector3.Zero;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValue<Vector3>("BuildAndRepair.GrindColor", value);
        }
    }

    /// <summary> 
    /// If set autogrind enemy blocks in range 
    /// </summary> 
    public bool GrindJanitorEnemies
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.GrindJanitorEnemies") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.GrindJanitorEnemies", value);
        }
    }

    /// <summary> 
    /// If set autogrind not owned blocks in range 
    /// </summary> 
    public bool GrindJanitorNotOwned
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.GrindJanitorNotOwned") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.GrindJanitorNotOwned", value);
        }
    }

    /// <summary> 
    /// If set autogrind blocks owned by neutrals in range 
    /// </summary> 
    public bool GrindJanitorNeutrals
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.GrindJanitorNeutrals") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.GrindJanitorNeutrals", value);
        }
    }

    /// <summary> 
    /// If set autogrind grinds blocks only down to the 'Out of order' level 
    /// </summary> 
    public bool GrindJanitorOptionDisableOnly
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.GrindJanitorOptionDisableOnly") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.GrindJanitorOptionDisableOnly", value);
        }
    }

    /// <summary> 
    /// If set autogrind grinds blocks only down to the 'Hack' level 
    /// </summary> 
    public bool GrindJanitorOptionHackOnly
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.GrindJanitorOptionHackOnly") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.GrindJanitorOptionHackOnly", value);
        }
    }

    /// <summary> 
    /// If set block are only weldet to functional level 
    /// </summary> 
    public bool WeldOptionFunctionalOnly
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.WeldOptionFunctionalOnly") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.WeldOptionFunctionalOnly", value);
        }
    }


    /// <summary> 
    /// Set the with of the working area 
    /// </summary> 
    public float AreaWidth
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueFloat("BuildAndRepair.AreaWidth") : 0;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueFloat("BuildAndRepair.AreaWidth", value);
        }
    }

    /// <summary> 
    /// Set the left/right offset of the working area from block center 
    /// </summary> 
    public float AreaOffsetLeftRight
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueFloat("BuildAndRepair.AreaOffsetLeftRight") : 0;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueFloat("BuildAndRepair.AreaOffsetLeftRight", value);
        }
    }

    /// <summary> 
    /// Set the height of the working area 
    /// </summary> 
    public float AreaHeight
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueFloat("BuildAndRepair.AreaHeight") : 0;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueFloat("BuildAndRepair.AreaHeight", value);
        }
    }

    /// <summary> 
    /// Set the up/down offset of the working area from block center 
    /// </summary> 
    public float AreaOffsetUpDown
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueFloat("BuildAndRepair.AreaOffsetUpDown") : 0;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueFloat("BuildAndRepair.AreaOffsetUpDown", value);
        }
    }

    /// <summary> 
    /// Set the depth of the working area 
    /// </summary> 
    public float AreaDepth
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueFloat("BuildAndRepair.AreaDepth") : 0;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueFloat("BuildAndRepair.AreaDepth", value);
        }
    }

    /// <summary> 
    /// Set the depth of the working area 
    /// </summary> 
    public float AreaOffsetFrontBack
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueFloat("BuildAndRepair.AreaOffsetFrontBack") : 0;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueFloat("BuildAndRepair.AreaOffsetFrontBack", value);
        }
    }

    /// <summary> 
    /// Get a list with all known block classes and there 
    /// weld enabled state in descending order of priority. 
    /// </summary> 
    public List<ClassState<BlockClass>> WeldPriorityList()
    {
        if (_Entities.Count > 0)
        {
            var list = GetValue<List<string>>("BuildAndRepair.WeldPriorityList");
            var blockList = new List<ClassState<BlockClass>>();
            foreach (var item in list)
            {
                var values = item.Split(';');
                BlockClass blockClass;
                bool enabled;
                if (Enum.TryParse<BlockClass>(values[0], out blockClass) &&
                   bool.TryParse(values[1], out enabled))
                {
                    blockList.Add(new ClassState<BlockClass>(blockClass, enabled));
                }
            }
            return blockList;
        }
        return null;
    }

    /// <summary> 
    /// Get the weld priority of the given block class 
    /// </summary> 
    public int GetWeldPriority(BlockClass blockClass)
    {
        if (_Entities.Count > 0)
        {
            var getPriority = GetValue<Func<int, int>>("BuildAndRepair.GetWeldPriority");
            return getPriority((int)blockClass);
        }
        else return int.MaxValue;
    }

    /// <summary> 
    /// Set the weld priority of the given block class 
    /// (lower number higher priority) 
    /// </summary> 
    public void SetWeldPriority(BlockClass blockClass, int prio)
    {
        foreach (var entity in _Entities)
        {
            var setPriority = entity.GetValue<Action<int, int>>("BuildAndRepair.SetWeldPriority");
            setPriority((int)blockClass, prio);
        }
    }

    /// <summary> 
    /// Get the weld enabled state of the given block class 
    /// Enabled=True Block of that class will be repaired/build 
    /// Enabled=False Block's of that class will be ignored 
    /// </summary> 
    public bool GetWeldEnabled(BlockClass blockClass)
    {
        if (_Entities.Count > 0)
        {
            var getEnabled = GetValue<Func<int, bool>>("BuildAndRepair.GetWeldEnabled");
            return getEnabled((int)blockClass);
        }
        else return false;
    }

    /// <summary> 
    /// Set the weld enabled state of the given block class 
    /// (see GetEnabled) 
    /// </summary> 
    public void SetWeldEnabled(BlockClass blockClass, bool enabled)
    {
        foreach (var entity in _Entities)
        {
            var setEnabled = entity.GetValue<Action<int, bool>>("BuildAndRepair.SetWeldEnabled");
            setEnabled((int)blockClass, enabled);
        }
    }

    /// <summary> 
    /// Get a list with all known block classes and there 
    /// grind enabled state in descending order of priority. 
    /// </summary> 
    public List<ClassState<BlockClass>> GrindPriorityList()
    {
        if (_Entities.Count > 0)
        {
            var list = GetValue<List<string>>("BuildAndRepair.GrindPriorityList");
            var blockList = new List<ClassState<BlockClass>>();
            foreach (var item in list)
            {
                var values = item.Split(';');
                BlockClass blockClass;
                bool enabled;
                if (Enum.TryParse<BlockClass>(values[0], out blockClass) &&
                   bool.TryParse(values[1], out enabled))
                {
                    blockList.Add(new ClassState<BlockClass>(blockClass, enabled));
                }
            }
            return blockList;
        }
        return null;
    }

    /// <summary> 
    /// Get the grind priority of the given block class 
    /// </summary> 
    public int GetGrindPriority(BlockClass blockClass)
    {
        if (_Entities.Count > 0)
        {
            var getPriority = GetValue<Func<int, int>>("BuildAndRepair.GetGrindPriority");
            return getPriority((int)blockClass);
        }
        else return int.MaxValue;
    }

    /// <summary> 
    /// Set the grind priority of the given block class 
    /// (lower number higher priority) 
    /// </summary> 
    public void SetGrindPriority(BlockClass blockClass, int prio)
    {
        foreach (var entity in _Entities)
        {
            var setPriority = entity.GetValue<Action<int, int>>("BuildAndRepair.SetGrindPriority");
            setPriority((int)blockClass, prio);
        }
    }

    /// <summary> 
    /// Get the grind enabled state of the given block class 
    /// Enabled=True Block of that class will be grinded 
    /// Enabled=False Block's of that class will be ignored 
    /// </summary> 
    public bool GetGrindEnabled(BlockClass blockClass)
    {
        if (_Entities.Count > 0)
        {
            var getEnabled = GetValue<Func<int, bool>>("BuildAndRepair.GetGrindEnabled");
            return getEnabled((int)blockClass);
        }
        else return false;
    }

    /// <summary> 
    /// Set the grind enabled state of the given block class 
    /// (see GetEnabled) 
    /// </summary> 
    public void SetGrindEnabled(BlockClass blockClass, bool enabled)
    {
        foreach (var entity in _Entities)
        {
            var setEnabled = entity.GetValue<Action<int, bool>>("BuildAndRepair.SetGrindEnabled");
            setEnabled((int)blockClass, enabled);
        }
    }

    /// <summary> 
    /// Get a list with all known component classes and there 
    /// enabeld state in descending order of priority. 
    /// </summary> 
    public List<ClassState<ComponentClass>> ComponentClassList()
    {
        if (_Entities.Count > 0)
        {
            var list = GetValue<List<string>>("BuildAndRepair.ComponentClassList");
            var compList = new List<ClassState<ComponentClass>>();
            foreach (var item in list)
            {
                var values = item.Split(';');
                ComponentClass compClass;
                bool enabled;
                if (Enum.TryParse<ComponentClass>(values[0], out compClass) &&
                   bool.TryParse(values[1], out enabled))
                {
                    compList.Add(new ClassState<ComponentClass>(compClass, enabled));
                }
            }
            return compList;
        }
        return null;
    }

    /// <summary> 
    /// Get the priority of the given component class 
    /// </summary> 
    public int GetCollectPriority(ComponentClass compClass)
    {
        if (_Entities.Count > 0)
        {
            var getPriority = GetValue<Func<int, int>>("BuildAndRepair.GetCollectPriority");
            return getPriority((int)compClass);
        }
        else return int.MaxValue;
    }

    /// <summary> 
    /// Set the priority of the given component class 
    /// (lower number higher priority) 
    /// </summary> 
    public void SetCollectPriority(ComponentClass compClass, int prio)
    {
        foreach (var entity in _Entities)
        {
            var setPriority = entity.GetValue<Action<int, int>>("BuildAndRepair.SetCollectPriority");
            setPriority((int)compClass, prio);
        }
    }

    /// <summary> 
    /// Get the enabled state of the given component class 
    /// Enabled=True Component of that class will be collected 
    /// Enabled=False Component's of that class will be ignored 
    /// </summary> 
    public bool GetCollectEnabled(ComponentClass compClass)
    {
        if (_Entities.Count > 0)
        {
            var getEnabled = GetValue<Func<int, bool>>("BuildAndRepair.GetCollectEnabled");
            return getEnabled((int)compClass);
        }
        else return false;
    }

    /// <summary> 
    /// Set the enabled state of the given component class 
    /// (see GetEnabled) 
    /// </summary> 
    public void SetCollectEnabled(ComponentClass compClass, bool enabled)
    {
        foreach (var entity in _Entities)
        {
            var setEnabled = entity.GetValue<Action<int, bool>>("BuildAndRepair.SetCollectEnabled");
            setEnabled((int)compClass, enabled);
        }
    }

    /// <summary> 
    /// Set if the Block should only collect floating items (ore/ingot/material) 
    /// if nothing else to do (no welding, no grinding, no material for welding) 
    /// </summary> 
    public bool CollectIfIdle
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.CollectIfIdle") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.CollectIfIdle", value);
        }
    }

    /// <summary> 
    /// Set if the Block should push all ore/ingot imemediately out of its inventory, 
    /// else this will happen only if no more room to store the next items to be picked. 
    /// </summary> 
    public bool PushIngotOreImmediately
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.PushIngotOreImmediately") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.PushIngotOreImmediately", value);
        }
    }

    /// <summary> 
    /// Get the block that is currently being repaired/build. 
    /// </summary> 
    public IMySlimBlock CurrentTarget
    {
        get
        {
            return _Entities.Count > 0 ? GetValue<IMySlimBlock>("BuildAndRepair.CurrentTarget") : null;
        }
    }

    /// <summary> 
    /// Get the block that is currently being grinded. 
    /// </summary> 
    public IMySlimBlock CurrentGrindTarget
    {
        get
        {
            return _Entities.Count > 0 ? GetValue<IMySlimBlock>("BuildAndRepair.CurrentGrindTarget") : null;
        }
    }

    /// <summary> 
    /// Set if the Block if controlled by script. 
    /// (If controlled by script use PossibleTargets and CurrentPickedTarget  
    /// to set the block that should be build/repaired) 
    /// </summary> 
    public bool ScriptControlled
    {
        get
        {
            return _Entities.Count > 0 ? _Entities[0].GetValueBool("BuildAndRepair.ScriptControlled") : false;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValueBool("BuildAndRepair.ScriptControlled", value);
        }
    }

    /// <summary> 
    /// Get a list of missing components. 
    /// </summary> 
    public Dictionary<VRage.Game.MyDefinitionId, int> MissingComponents()
    {
        var missingItems = new Dictionary<VRage.Game.MyDefinitionId, int>();
        foreach (var entity in _Entities)
        {
            var dict = entity.GetValue<Dictionary<VRage.Game.MyDefinitionId, int>>("BuildAndRepair.MissingComponents");
            //Merge dictionaries but only first report of an item or higher amount 
            //(do not add up the missings, as overlapping systems report same missing items) 
            if (dict != null && dict.Count > 0)
            {
                int value;
                foreach (var newItem in dict)
                {
                    if (missingItems.TryGetValue(newItem.Key, out value))
                    {
                        if (newItem.Value > value) missingItems[newItem.Key] = newItem.Value;
                    }
                    else
                    {
                        missingItems.Add(newItem.Key, newItem.Value);
                    }
                }
            }
        }
        return missingItems;
    }

    /// <summary> 
    /// Get a list of possible repair/build targets. 
    /// (Contains only damaged/deformed/new block's in range of the system) 
    /// </summary> 
    public List<IMySlimBlock> PossibleTargets()
    {
        if (_Entities.Count > 0)
        {
            return GetValue<List<IMySlimBlock>>("BuildAndRepair.PossibleTargets");
        }
        return null;
    }

    public T GetValue<T>(string propertyName)
    {
        if (_Entities.Count > 0)
        {
            try
            {
                return _Entities[0].GetValue<T>(propertyName);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        return default(T);
    }

    /// <summary> 
    /// Get the Block that should currently repaired/build. 
    /// In order to build the given block the property 'ScriptControlled' has to be true and 
    /// the block has to be in the list of 'PossibleTargets'. 
    /// If 'ScriptControlled' is true and the block is not in the 'PossibleTargets' 
    /// the system will do nothing. 
    /// </summary> 
    public IMySlimBlock CurrentPickedTarget
    {
        get
        {
            return _Entities.Count > 0 ? GetValue<IMySlimBlock>("BuildAndRepair.CurrentPickedTarget") : null;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValue("BuildAndRepair.CurrentPickedTarget", value);
        }
    }

    /// <summary> 
    /// Get a list of possible grind targets. 
    /// </summary> 
    public HashSet<IMySlimBlock> PossibleGrindTargets()
    {
        if (_Entities.Count > 0)
        {

            var hashedList = new HashSet<IMySlimBlock>();
            foreach (var entity in _Entities)
            {
                var list = entity.GetValue<List<IMySlimBlock>>("BuildAndRepair.PossibleGrindTargets");
                hashedList.UnionWith(list);
            }

            return hashedList;
        }
        return null;
    }

    /// <summary> 
    /// Get the Block that should currently Grinded. 
    /// In order to grind the given block the property 'ScriptControlled' has to be true and 
    /// the block has to be in the list of 'PossibleGrindTargets'. 
    /// If 'ScriptControlled' is true and the block is not in the 'PossibleGrindTargets' 
    /// the system will do nothing. 
    /// </summary> 
    public IMySlimBlock CurrentPickedGrindTarget
    {
        get
        {
            return _Entities.Count > 0 ? GetValue<IMySlimBlock>("BuildAndRepair.CurrentPickedGrindTarget") : null;
        }
        set
        {
            foreach (var entity in _Entities) entity.SetValue("BuildAndRepair.CurrentPickedGrindTarget", value);
        }
    }

    /// <summary> 
    /// Get a list of possible grind targets. 
    /// </summary> 
    public List<IMyEntity> PossibleCollectTargets()
    {
        if (_Entities.Count > 0)
        {
            return GetValue<List<IMyEntity>>("BuildAndRepair.PossibleCollectTargets");
        }
        return null;
    }

    /// <summary> 
    /// Ensures that the given amount is either in inventory or the production 
    /// queue of the given production blocks 
    /// </summary> 
    public int EnsureQueued(IEnumerable<long> productionBlockIds, VRage.Game.MyDefinitionId materialId, int amount)
    {
        if (_Entities.Count > 0)
        {
            if (_EnsureQueued == null)
            {
                _EnsureQueued = GetValue<Func<IEnumerable<long>, VRage.Game.MyDefinitionId, int, int>>("BuildAndRepair.ProductionBlock.EnsureQueued");
            }

            if (_EnsureQueued != null)
            {
                return _EnsureQueued(productionBlockIds, materialId, amount);
            }
            return -3;
        }
        return -2;
    }

    /// <summary> 
    /// Retrieve the total components amount needed to build the projected 
    /// blueprint 
    /// </summary> 
    /// <param name="projector"></param> 
    /// <param name="componentList"></param> 
    /// <returns></returns> 
    public int NeededComponents4Blueprint(IMyProjector projector, Dictionary<VRage.Game.MyDefinitionId, VRage.MyFixedPoint> componentList)
    {
        if (_Entities.Count > 0)
        {
            if (_NeededComponents4Blueprint == null)
            {
                _NeededComponents4Blueprint = GetValue<Func<IMyProjector, Dictionary<VRage.Game.MyDefinitionId, VRage.MyFixedPoint>, int>>("BuildAndRepair.Inventory.NeededComponents4Blueprint");
            }

            if (_NeededComponents4Blueprint != null)
            {
                return _NeededComponents4Blueprint(projector, componentList);
            }
            return -3;
        }
        return -2;
    }
}
/// <summary>
/// Minimal memory-safe wrappers compatible with programmable block environment.
/// If mod-provided types exist, these stand-ins provide equivalent API used by this script.
/// </summary>
// NOTE: Rely on mod-provided MemorySafeList/MemorySafeDictionary types exposed to PB; do not redefine here to avoid conflicts.

/// <summary> 
///    Class to handle Entities 
/// </summary> 
public class EntityHandler<T> : EntityHandler where T : class, IMyTerminalBlock
{
    protected readonly List<T> _Entities = new List<T>();
    protected readonly HashSet<MyDefinitionId> _DefinitionIdsInclude = new HashSet<MyDefinitionId>();
    protected readonly HashSet<MyDefinitionId> _DefinitionIdsExclude = new HashSet<MyDefinitionId>();

    /// <summary> 
    ///  
    /// </summary> 
    public IEnumerable<T> Entities
    {
        get
        {
            return _Entities;
        }
    }

    public HashSet<MyDefinitionId> DefinitionIdsInclude
    {
        get
        {
            return _DefinitionIdsInclude;
        }
    }

    public HashSet<MyDefinitionId> DefinitionIdsExclude
    {
        get
        {
            return _DefinitionIdsExclude;
        }
    }

    /// <summary> 
    ///  
    /// </summary> 
    public bool AreEnabled { get; private set; }

    /// <summary> 
    /// Count of Working Enties (on and functional) 
    /// </summary> 
    public int CountOfWorking
    {
        get
        {
            var res = 0;
            foreach (var entity in _Entities) if (entity.IsWorking && entity.IsFunctional) res++;
            return res;
        }
    }

    /// <summary> 
    /// Get total count 
    /// </summary> 
    protected override int GetCount()
    {
        return _Entities.Count;
    }

    /// <summary> 
    /// Load entities from group 
    /// </summary> 
    public override void Init(IMyBlockGroup group, bool add = false)
    {
        if (!add) { _Entities.Clear(); AreEnabled = false; }
        var entities = new List<T>();
        group.GetBlocksOfType(entities);
        foreach (var entity in entities)
        {
            AddEntity(entity);
        }
        CheckEnabled();
    }

    /// <summary> 
    /// Load entity by name 
    /// </summary> 
    /// <param name="blocks"></param> 
    /// <param name="name"></param> 
    public override void Init(VRage.Game.ModAPI.Ingame.IMyEntity newEntity, bool add = false)
    {
        if (!add) { _Entities.Clear(); AreEnabled = false; }
        var entity = newEntity as T;
        if (AddEntity(entity))
        {
            CheckEnabled();
        }
    }

    /// <summary> 
    /// Load entity filtered by given collect function 
    /// </summary> 
    /// <param name="blocks"></param> 
    /// <param name="name"></param> 
    public void Init(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> collect = null, bool add = false)
    {
        if (!add) { _Entities.Clear(); AreEnabled = false; }
        if (gridTerminalSystem != null)
        {
            var entities = new List<T>();
            gridTerminalSystem.GetBlocksOfType<T>(entities, collect);
            foreach (var entity in entities)
            {
                AddEntity(entity);
            }
            CheckEnabled();
        }
    }

    /// <summary> 
    ///    Starten/Stoppen 
    /// </summary> 
    protected virtual bool AddEntity(T entity)
    {
        if (entity == null || _Entities.IndexOf(entity) >= 0) return false;
        var newDefId = entity.BlockDefinition;
        var allowed = DefinitionIdsInclude.Count <= 0;
        foreach (var defId in DefinitionIdsInclude)
        {
            if (defId.TypeId == newDefId.TypeId && (string.IsNullOrEmpty(defId.SubtypeName) || defId.SubtypeName.Equals(newDefId.SubtypeName)))
            {
                allowed = true;
                break;
            }
        }
        if (!allowed) return false;

        foreach (var defId in DefinitionIdsExclude)
        {
            if (defId.TypeId == newDefId.TypeId && (string.IsNullOrEmpty(defId.SubtypeName) || defId.SubtypeName.Equals(newDefId.SubtypeName)))
            {
                return false;
            }
        }

        _Entities.Add(entity);
        return true;
    }

    /// <summary> 
    ///    Starten/Stoppen 
    /// </summary> 
    public void Enabled(bool enabled)
    {
        foreach (var entity in _Entities)
        {
            var funcBlock = entity as IMyFunctionalBlock;
            if (funcBlock != null && funcBlock.Enabled != enabled) funcBlock.Enabled = enabled;
        }
        AreEnabled = enabled;
    }

    /// <summary> 
    ///  
    /// </summary> 
    private void CheckEnabled()
    {
        foreach (var entity in _Entities)
        {
            if (entity.IsWorking && entity.IsFunctional)
            {
                AreEnabled = true;
                break;
            }
        }
    }
}
/// <summary> 
///  
/// </summary> 
public abstract class EntityHandler
{
    public int Count { get { return GetCount(); } }
    public abstract void Init(IMyBlockGroup group, bool add = false);
    public abstract void Init(VRage.Game.ModAPI.Ingame.IMyEntity entity, bool add = false);
    protected abstract int GetCount();

    /// <summary> 
    ///  
    /// </summary> 
    public static string GetCustomData(string customData, string startTag, string endTag)
    {
        var start = customData.IndexOf(startTag);
        var end = customData.LastIndexOf(endTag);
        if (start < 0 || end < 0 || end < start) return null;
        return customData.Substring(start + startTag.Length, end - start - startTag.Length);
    }

    /// <summary> 
    ///  
    /// </summary> 
    public static string GetCustomValue(string customData, string name)
    {
        var tag = "<" + name + "=";
        var start = customData.IndexOf(tag);
        if (start < 0) return null;
        var end = customData.IndexOf("/>", start + tag.Length);
        if (end < 0) return null;
        return customData.Substring(start + tag.Length, end - start - tag.Length);
    }
}
}
