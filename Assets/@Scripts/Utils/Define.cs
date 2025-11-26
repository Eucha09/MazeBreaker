using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum ObjectInfoType
    {
        None,
        Tile,
        Creature,
        Nature,
        RewardObject,
        BuildingObject,
    }

	public enum WorkState
	{
		Idle,
		Work,
		Pending,
    }

    public enum DayTimeType
    {
        None,
        Day,
        Night,
    }

	public enum DetectorType
	{
		None,
        Meat,
        Stone,
        Flower,
        Wood,
	}

	public enum GameObjectType
    {
        None,
        Player,
        Monster,
        MonsterGroup,
        Nature,
        Structure,
        ShieldBoss,
    }

    public enum MaterialType
    {
        None,
        Tree,
        Stone,
        Grass,
        EggHouse,
        Core,
    }

    public enum MonsterType
    {
        None,
    }

    public enum NatureType
    {
        None,
    }

    public enum StructureType
    {
        None,
        Building,
    }

    public enum SkillType
    {
        Default,
        Projectile,
        DamageOverTime
    }

	public enum ItemType
	{
		None,
        Equipment,
		Consumable,
		CraftingMaterial,
        SpecialItem,
	}

    public enum EquipmentType
    {
        Weapon,
        Armor,
        Tool,
    }

	public enum WeaponType
    {
        None,
        Hand,
        Sword,
        Bow,
        Axe,
        Mace,
        Hammer,
    }


    public enum ArmorType
    {
        None,
        Helmet,
        Armor,
        Boots,
    }

    public enum ToolType
    {
        None,
    }

    public enum ConsumableType
    {
        None,
        Potion,
        Food,
    }

    public enum FoodType
    { 
        None,
        Meat,
    }

    public enum CraftingMaterialType
    { 
        None,
    }

    public enum SpecialItemType
    {
        None,
    }

    public enum InteractionType
    {
        None,
        Interact,
        Push,
        Fall,
        Up,
        Down,
        Exchange,
        Teleport,
    }

    public enum QuestType
    { 
        None,
        CollectItem,
        KillMonster,
        UseFeature,
        UseItem,
        EquipItem,
        OutOfLocation,
        MoveToLocation,
		GameEvent,
        Building,
    }

    public enum Dir
    {
        Left,
        Right,
        Up,
        Down,
        Center,
    }

    public enum CellType
    {
        None,
        Seed,
        Forest,
        Glade3x3,
        ForestWall,
        OuterWall,
        GladeWall,
        GladeDoor,
        OuterDoor,
		SpecialAreaWall,
		SpecialAreaDoor,
		SectorA,
        SectorA_Wall,
        Rotten,
        Woodland3x3,
        SpiderHabitat3x3,
        SpiderHabitat2x2,
		OpenField2x2,
		GoldMine3x3,
        IronMine3x3,
        CopperMine3x3,
        SectorEnergySupply2x2,
        TutorialStartPoint,
        TutorialRaderEvent,
        TutorialClothChangeEvent,
		TutorialForest,
		TutorialNoteBookEvent,
		TutorialSoldierDead1,
		TutorialSoldierDead2,
		TutorialSoldierDead3,
		TutorialSoldierDead4,
		SectorA_Door,
		CopperMine2x2,
		IronMine2x2,
		MaxCount,
	}

    public enum TileType
    {
        None,
        Empty,
        Seed,
        Wall,
        Unchangeable,
    }

    public enum DistributionType
    {
        Uniform,
        Gaussian,
    }

    public enum AreaType
	{
		Local,
		Global,
    }

    public enum Layer
    {
        Default = 0,
        IgnoreRaycast = 2,
        Ground = 7,
        Monster = 8,
        Player = 9,
        Block = 10,
        DRM = 12,
        Obstacle = 14,
    }

    public enum Scene
    {
        Unknown,
        Title,
        Prologue1,
		Prologue2,
		Tutorial,
		Prologue3,
		Game,
        End,
    }

    public enum CreatureState
    {
        Idle,
        Moving,
        Run,
        Jump,
        Skill,
        Damaged,
        Stunned,
        Dead,
        Crazy,
        Start,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        BattleBgm,
        MaxCount,
    }

    public enum DeviceMode
    {
        KeyboardMouse,
        Gamepad,
    }

    public enum UIEvent
    {
        Click,
        Drag,
        PointerDown,
        PointerUp,
        PointerEnter,
        PointerExit,
        Scroll,
    }

    public enum MouseEvent
    {
        Press,
        PointerDown,
        PointerUp,
        Click,
    }

    // Old Version
    public enum CameraMode
    {
        QuarterView,
        WorldView,
    }

    // New Version
    public enum CameraType
	{
		Follow,
        TopDown,
    }
}
