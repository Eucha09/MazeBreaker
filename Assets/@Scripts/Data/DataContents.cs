using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// json 형식의 파일 내용을 저장할 데이터 class들 정의
namespace Data
{
	/*ex)
    #region Stat

    [Serializable]
    public class Stat
    {
       public int level;
       public int maxHp;
       public int attack;
       public int totalExp;
    }

    [Serializable]
    public class StatData : ILoader<int, Stat>
    {
       public List<Stat> stats = new List<Stat>();

       public Dictionary<int, Stat> MakeDict()
       {
           Dictionary<int, Stat> dict = new Dictionary<int, Stat>();
           foreach (Stat stat in stats)
               dict.Add(stat.level, stat);
           return dict;
       }
    }

    #endregion
	*/


	#region StatDescription
	[Serializable]
	public class StatDescriptionData
	{
		public int id;
		public string name;
		public string description;
	}

	[Serializable]
	public class StatDescriptionLoader : ILoader<int, StatDescriptionData>
	{
		public List<StatDescriptionData> stats = new List<StatDescriptionData>();

		public Dictionary<int, StatDescriptionData> MakeDict()
		{
			Dictionary<int, StatDescriptionData> dict = new Dictionary<int, StatDescriptionData>();
			foreach (StatDescriptionData stat in stats)
				dict.Add(stat.id, stat);
			return dict;
		}
	}
	#endregion

	#region Skill
	[Serializable]
	public class SkillData
    {
        public int id;
        public string name;
		public string iconPath;
        public string description;
		public string skillType;
		public int costCount;
	}

    [Serializable]
    public class SkillLoader : ILoader<int, SkillData>
    {
        public List<SkillData> skills = new List<SkillData>();

        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skill in skills)
                dict.Add(skill.id, skill);
            return dict;
        }
    }
	#endregion

	#region Item
	[Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public ItemType itemType;
        public string iconPath;
        public string prefabPath;
        public string description;
        public string detailedDescription;
        public bool dropable = true;
    }

    [Serializable]
    public class EquipmentData : ItemData
    {
        public EquipmentType equipmentType;
		public WeaponType weaponType;
		public ArmorType armorType;
		public int damage;
		public int defence;
		public int maxDurability;
        public int qSkillId;
        public string qSkillNote;
        public int eSkillId;
        public string eSkillNote;
	}

    //[Serializable]
    //public class WeaponData : ItemData
    //{
    //    public WeaponType weaponType;
    //    public int damage;
    //    public int maxDurability;
    //}

    //[Serializable]
    //public class ArmorData : ItemData
    //{
    //    public ArmorType armorType;
    //    public int defence;
    //    public int maxDurability;
    //}

    [Serializable]
    public class ConsumableData : ItemData
    {
        public ConsumableType consumableType;
        public FoodType foodType;
		public int maxCount;
		public float hp;
        public float mental;
        public float hunger;
        public float positiveEnergy;
	}

    [Serializable]
    public class CraftingMaterialData : ItemData
    {
        public CraftingMaterialType craftingMaterialType;
		public int maxCount;
		public bool fuelable;
		public float fuelEfficiency;
	}

    [Serializable]
    public class SpecialItemData : ItemData
    {
        public SpecialItemType specialItemType;
        public int maxCount;
    }

    //[Serializable]
    //public class ResourceData : ItemData
    //{
    //    public ResourceType resourceType;
    //    public int maxCount;
    //    public bool fuelable;
    //    public float fuelEfficiency;
    //}

    [Serializable]
    public class ItemLoader : ILoader<int, ItemData>
    {
        public List<EquipmentData> equipments = new List<EquipmentData>();
        public List<ConsumableData> consumables = new List<ConsumableData>();
        public List<CraftingMaterialData> craftingMaterials = new List<CraftingMaterialData>();
		public List<SpecialItemData> specialItems = new List<SpecialItemData>();

		public Dictionary<int, ItemData> MakeDict()
        {
            Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
            foreach (ItemData item in equipments)
            {
                item.itemType = ItemType.Equipment;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in consumables)
            {
                item.itemType = ItemType.Consumable;
                dict.Add(item.id, item);
            }
            foreach (ItemData item in craftingMaterials)
            {
                item.itemType = ItemType.CraftingMaterial;
                dict.Add(item.id, item);
			}
			foreach (ItemData item in specialItems)
			{
				item.itemType = ItemType.SpecialItem;
				dict.Add(item.id, item);
			}
			return dict;
        }
    }
    #endregion

    #region Crafting
    [Serializable]
    public class RequiredItem
    {
        public int id;
		public string note;
		public int count;
    }

    [Serializable]
    public class CraftingData
    {
        public int itemId;
        public string note;
        public List<RequiredItem> requiredItems;
		public float timeRequired;
		public int stationId;
        public bool locked;
	}

    [Serializable]
    public class CraftingLoader : ILoader<int, CraftingData>
    {
        public List<CraftingData> craftingItems = new List<CraftingData>();

        public Dictionary<int, CraftingData> MakeDict()
        {
            Dictionary<int, CraftingData> dict = new Dictionary<int, CraftingData>();
            foreach (CraftingData crafting in craftingItems)
            {
                dict.Add(crafting.itemId, crafting);
            }
            return dict;
        }
    }
    #endregion

    #region Building
    [Serializable]
    public class BuildingData
    {
        public int objectId;
		public string note;
		public string prefabPath;
        public List<RequiredItem> requiredItems;
        public bool locked;
	}

    [Serializable]
    public class BuildingLoader : ILoader<int, BuildingData>
    {
        public List<BuildingData> buildingObjects = new List<BuildingData>();

        public Dictionary<int, BuildingData> MakeDict()
        {
            Dictionary<int, BuildingData> dict = new Dictionary<int, BuildingData>();
            foreach (BuildingData building in buildingObjects)
            {
                dict.Add(building.objectId, building);
            }
            return dict;
        }
    }
	#endregion

	#region Refinement
	[Serializable]
	public class RefinementData
	{
		public int refinementId;
		public List<RequiredItem> inputItems;
		public List<RequiredItem> outputItems;
        public float timeRequired;
        public int stationId;
	}

	[Serializable]
	public class RefinementLoader : ILoader<int, RefinementData>
	{
		public List<RefinementData> refinementData = new List<RefinementData>();

		public Dictionary<int, RefinementData> MakeDict()
		{
			Dictionary<int, RefinementData> dict = new Dictionary<int, RefinementData>();
			foreach (RefinementData data in refinementData)
			{
				dict.Add(data.refinementId, data);
			}
			return dict;
		}
	}
	#endregion

	#region Stat
	[Serializable]
    public class StatInfo
    {
        public float hp;
        public float maxHp;
        public float attack;
        public float defense;
        public float speed;
        public float currentSpeed;

        // player stat
        public float mental;
        public float maxMental;
        public float hunger;
        public float maxHunger;
        public float stamina;
        public float maxStamina;
        public float specialValue;

        // Deep Copy
        public void MergeFrom(StatInfo other)
        {
            if (other == null)
                return;
            if (other.hp != 0)
                hp = other.hp;
            if (other.maxHp != 0)
                maxHp = other.maxHp;
            if (other.attack != 0)
                attack = other.attack;
            if (other.defense != 0)
                defense = other.defense;
            if (other.speed != 0.0f)
                speed = other.speed;
            if (other.mental != 0)
                mental = other.mental;
            if (other.maxMental != 0)
                maxMental = other.maxMental;
            if (other.hunger != 0)
                hunger = other.hunger;
            if (other.maxHunger != 0)
                maxHunger = other.maxHunger;
            if (other.stamina != 0.0f)
                stamina = other.stamina;
            if (other.maxStamina != 0.0f)
                maxStamina = other.maxStamina;
            if (other.specialValue != 0.0f)
                specialValue = other.specialValue;
        }
    }
    #endregion

    #region Objects
    [Serializable]
    public class RewardData
    {
        public int probability; // 100분율
        public int itemId;
        public int minCount;
        public int maxCount;
		public DayTimeType dayTimeType;
    }

    [Serializable]
    public class ObjectData
    {
        public int id;
        public string name;
        public GameObjectType objectType;
        public MaterialType materialType;
        public float colliderRadius = 1.0f;
        public string iconPath;
        public string description;
        public int minimapMarkerId;
        public bool isAwaysVisibleOnMinimap;
		public StatInfo stat;
        public List<RewardData> rewards;
    }

    [Serializable]
    public class MonsterData : ObjectData
    {
        public MonsterType monsterType;
        public DetectorType detectorType;
        public string prefabPath;
    }

    [Serializable]
    public class MonsterGroupData : ObjectData
    {
        public List<int> monsterIds;
        public string prefabPath;
    }

    [Serializable]
    public class NatureData : ObjectData
    {
        public NatureType natureType;
        public List<string> prefabPaths;
    }

    [Serializable]
    public class StructureData : ObjectData
    {
        public StructureType structureType;
        public string prefabPath;
        public int slotCount;
    }

    [Serializable]
    public class ObjectLoader : ILoader<int, ObjectData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();
        public List<MonsterGroupData> monsterGroups = new List<MonsterGroupData>();
        public List<NatureData> natureObjects = new List<NatureData>();
        public List<StructureData> structureObjects = new List<StructureData>();

        public Dictionary<int, ObjectData> MakeDict()
        {
            Dictionary<int, ObjectData> dict = new Dictionary<int, ObjectData>();
            foreach (MonsterData monster in monsters)
            {
                monster.objectType = GameObjectType.Monster;
                dict.Add(monster.id, monster);
            }
            foreach (MonsterGroupData monsterGroup in monsterGroups)
            {
                monsterGroup.objectType = GameObjectType.MonsterGroup;
                dict.Add(monsterGroup.id, monsterGroup);
            }
            foreach (NatureData nature in natureObjects)
            {
                nature.objectType = GameObjectType.Nature;
                dict.Add(nature.id, nature);
            }
            foreach (StructureData structure in structureObjects)
            {
                structure.objectType = GameObjectType.Structure;
                dict.Add(structure.id, structure);
            }
            return dict;
        }
    }
    #endregion

    #region MazeObjects
    [Serializable]
    public class MazeObjectData
    {
        public int id;
        public CellType cellType;
        public int cellSizeX = 1;
        public int cellSizeY = 1;
        public float sizeX;
        public float sizeY;
		public AreaType areaType;
		public int minimapMarkerId;
		public bool isAwaysVisibleOnMinimap;
		public List<string> prefabPaths;
    }

    [Serializable]
    public class MazeObjectLoader : ILoader<int, MazeObjectData>
    {
        public List<MazeObjectData> mazeObjects = new List<MazeObjectData>();

        public Dictionary<int, MazeObjectData> MakeDict()
        {
            Dictionary<int, MazeObjectData> dict = new Dictionary<int, MazeObjectData>();
            foreach (MazeObjectData mazeObject in mazeObjects)
            {
                dict.Add(mazeObject.id, mazeObject);
            }
            return dict;
        }
    }
	#endregion

	#region SpawnRule
	[Serializable]
	public class SpawnRuleData
	{
		public int objectTemplateId;
        public string objectName;
		public float frequency;
		public List<CellType> cellTypes;
	}

	[Serializable]
	public class SpawnRuleLoader : ILoader<int, SpawnRuleData>
	{
		public List<SpawnRuleData> spawnRules = new List<SpawnRuleData>();

		public Dictionary<int, SpawnRuleData> MakeDict()
		{
			Dictionary<int, SpawnRuleData> dict = new Dictionary<int, SpawnRuleData>();
			foreach (SpawnRuleData rule in spawnRules)
			{
				dict.Add(rule.objectTemplateId, rule);
			}
			return dict;
		}
	}
	#endregion

	#region Quest
	[Serializable]
    public class QuestData
    {
        public int id;
        public string title;
        public List<int> priorQuestIds;
        public List<int> nextQuestIds;
        public string context;
        public string context_xbox;
        public string toolTip;
        public List<GameStatus> statusToUpdate_Before = new List<GameStatus>();
        public List<GameStatus> statusToUpdate_After = new List<GameStatus>();
		public List<int> unlockCrafting_Before = new List<int>();
		public List<int> unlockBuilding_Before = new List<int>();
		public QuestType questType;
        public DayTimeType dayTimeType;
        public string str1;
        public GameStatus status1;
        public int arg1;
        public int arg2;
        public int arg3;
    }

    [Serializable]
    public class QuestLoader : ILoader<int, QuestData>
    {
        public List<QuestData> quests = new List<QuestData>();

        public Dictionary<int, QuestData> MakeDict()
        {
            Dictionary<int, QuestData> dict = new Dictionary<int, QuestData>();
            foreach (QuestData q in quests)
                dict.Add(q.id, q);
            foreach (QuestData q in quests)
            {
                foreach (int priorId in q.priorQuestIds)
                {
                    QuestData priorQuestData = null;
                    dict.TryGetValue(priorId, out priorQuestData);
                    if (priorQuestData != null)
                    {
                        if (priorQuestData.nextQuestIds == null)
                            priorQuestData.nextQuestIds = new List<int>();
                        priorQuestData.nextQuestIds.Add(q.id);
                    }
                }
            }
            return dict;
        }
    }
	#endregion

	#region MinimapMarker
    [Serializable]
    public class MinimapMarkerData
    {
        public int id;
        public string name;
        public string iconPath;
        public float size;
		public bool syncToCameraRotation;
		public bool isIndicator;
        public bool isPin;
	}

    [Serializable]
    public class MinimapMarkerLoader : ILoader<int, MinimapMarkerData>
    {
        public List<MinimapMarkerData> markers = new List<MinimapMarkerData>();
       
        public Dictionary<int, MinimapMarkerData> MakeDict()
        {
            Dictionary<int, MinimapMarkerData> dict = new Dictionary<int, MinimapMarkerData>();
            foreach (MinimapMarkerData marker in markers)
                dict.Add(marker.id, marker);
            return dict;
        }
	}
	#endregion

	// Save

	#region Player
	[Serializable]
    public class ItemDb
    {
        public int templateId;
        public int count;
        public int slot;
        public bool equipped;
    }

    [Serializable]
    public class PlayerDb
    {
        public int id;
        public string name;
        public int hp;
        public int maxHp;
        public int attack;
        public float speed;
        public List<ItemDb> items;
    }

    [Serializable]
    public class PlayerDbLoader : ILoader<int, PlayerDb>
    {
        List<PlayerDb> players = new List<PlayerDb>();

        public Dictionary<int, PlayerDb> MakeDict()
        {
            Dictionary<int, PlayerDb> dict = new Dictionary<int, PlayerDb>();
            foreach (PlayerDb player in players)
                dict.Add(player.id, player);
            return dict;
        }
    }
    #endregion

    #region Maze

    public class MazeCellData
    {
        public int mazeObjectId;
        public Vector2Int gridPos;
        public List<SpawnData> spawnObjects;
    }

    public class SpawnData
    {
        public int templateId;
        public bool exist;
    }
    #endregion


}