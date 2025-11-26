using System;
using UnityEngine;

public class EventManager
{
    BuildingEvents _buildingEvents = new BuildingEvents();
    GameEvents _gameEvents = new GameEvents();
    ItemEvents _itemEvents = new ItemEvents();
    PlayerEvents _playerEvents = new PlayerEvents();
    QuestEvents _questEvents = new QuestEvents();
    SpawnEvents _spawnEvents = new SpawnEvents();
    TimeEvents _timeEvents = new TimeEvents();

	public BuildingEvents BuildingEvents { get { return _buildingEvents; } }
	public GameEvents GameEvents { get { return _gameEvents; } }
	public ItemEvents ItemEvents { get { return _itemEvents; } }
    public PlayerEvents PlayerEvents { get { return _playerEvents; } }
    public QuestEvents QuestEvents { get { return _questEvents; } }
	public SpawnEvents SpawnEvents { get { return _spawnEvents; } }
    public TimeEvents TimeEvents { get { return _timeEvents; } }

	public Action UpdateAction;
    public void OnUpdate()
    {
        if (UpdateAction != null)
        {
            UpdateAction();
        }
    }

    public void Init()
    {

    }

    public void Clear()
    {
        BuildingEvents.Clear();
		GameEvents.Clear();
        ItemEvents.Clear();
        PlayerEvents.Clear();
        QuestEvents.Clear();
        SpawnEvents.Clear();
        TimeEvents.Clear();
	}
}
