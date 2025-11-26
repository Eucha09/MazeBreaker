using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
	// 데이터들을 가지고 있을 Dictionary 선언
	// ex) public Dictionary<int, Data.Stat> StatDict { get; private set; } = new Dictionary<int, Data.Stat>();
	public Dictionary<int, Data.StatDescriptionData> StatDict { get; private set; } = new Dictionary<int, Data.StatDescriptionData>();
	public Dictionary<int, Data.SkillData> SkillDict { get; private set; } = new Dictionary<int, Data.SkillData>();
    public Dictionary<int, Data.ItemData> ItemDict { get; private set; } = new Dictionary<int, Data.ItemData>();
    public Dictionary<int, Data.CraftingData> CraftingDict { get; private set; } = new Dictionary<int, Data.CraftingData>();
    public Dictionary<int, Data.BuildingData> BuildingDict { get; private set; } = new Dictionary<int, Data.BuildingData>();
    public Dictionary<int, Data.RefinementData> RefinementDict { get; private set; } = new Dictionary<int, RefinementData>();
    public Dictionary<int, Data.ObjectData> ObjectDict { get; private set; } = new Dictionary<int, Data.ObjectData>();
    public Dictionary<int, Data.MazeObjectData> MazeObjectDict { get; private set; } = new Dictionary<int, Data.MazeObjectData>();
	public Dictionary<int, Data.SpawnRuleData> SpawnRuleDict { get; private set; } = new Dictionary<int, Data.SpawnRuleData>();
	public Dictionary<int, Data.QuestData> QuestDict { get; private set; } = new Dictionary<int, QuestData>();
    public Dictionary<int, Data.MinimapMarkerData> MinimapMarkerDict { get; private set; } = new Dictionary<int, MinimapMarkerData>();

	public PlayerDb Player { get; private set; } = new PlayerDb();
    string _path;

    public void Init()
    {
        _path = Application.persistentDataPath + "/";

		// json 형식의 데이터 파일을 읽어들여 Dictionary 형태로 저장
		// ex) StatDict = LoadJson<Data.StatData, int, Data.Stat>("StatData").MakeDict();
		StatDict = LoadJson<Data.StatDescriptionLoader, int, Data.StatDescriptionData>("StatDescriptionData").MakeDict();
		SkillDict = LoadJson<Data.SkillLoader, int, Data.SkillData>("SkillData").MakeDict();
        ItemDict = LoadJson<Data.ItemLoader, int, Data.ItemData>("ItemData").MakeDict();
        CraftingDict = LoadJson<Data.CraftingLoader, int, Data.CraftingData>("CraftingData").MakeDict();
        BuildingDict = LoadJson<Data.BuildingLoader, int, Data.BuildingData>("BuildingData").MakeDict();
        RefinementDict = LoadJson<Data.RefinementLoader, int, Data.RefinementData>("RefinementData").MakeDict();
        ObjectDict = LoadJson<Data.ObjectLoader, int, Data.ObjectData>("ObjectData").MakeDict();
        MazeObjectDict = LoadJson<Data.MazeObjectLoader, int, Data.MazeObjectData>("MazeObjectData").MakeDict();
		SpawnRuleDict = LoadJson<Data.SpawnRuleLoader, int, Data.SpawnRuleData>("SpawnRuleData").MakeDict();
		QuestDict = LoadJson<QuestLoader, int, QuestData>("QuestData").MakeDict();
        MinimapMarkerDict = LoadJson<MinimapMarkerLoader, int, MinimapMarkerData>("MinimapMarkerData").MakeDict();
        
	}

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
		TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(textAsset.text);
	}

    void SaveJson<Loader, Key, Value>(Loader value) where Loader : ILoader<Key, Value>
    {
        string data = Newtonsoft.Json.JsonConvert.SerializeObject(value);

    }

    void SaveData()
    {
        string data = Newtonsoft.Json.JsonConvert.SerializeObject(Player);
        File.WriteAllText(_path + "save", data);
    }

    void LoadData()
    {
        string data = File.ReadAllText(_path + "save");
        Player = JsonUtility.FromJson<PlayerDb>(data);
    }
}
