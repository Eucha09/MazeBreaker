using Data;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class Quest
{
    public int QuestId { get; private set; }
    public bool IsFinished { get; private set; }
    public string Context { get; private set; }
	public string Context_Xbox { get; private set; }
	public string ToolTip { get; private set; }
    public QuestType QuestType { get; private set; }
    public DayTimeType DayTimeType { get; private set; }

    public int TargetId { get; protected set; }
    public int TargetValue { get; protected set; }
    public int CurValue { get; protected set; }

    public Quest(int id)
    {
        QuestId = id;

        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(id, out questData);
        if (questData != null)
        {
            QuestType = questData.questType;
            DayTimeType = questData.dayTimeType;
            Context = questData.context;
            Context_Xbox = questData.context_xbox;
			ToolTip = questData.toolTip;
        }
    }

    public virtual void ReconnectingEvent() { }

    public void FinishQuest()
    {
        if (IsFinished)
            return;

        IsFinished = true;
        Managers.Quest.FinishQuest(QuestId);

    }
}

public class CollectItemQuest : Quest
{
    public CollectItemQuest(int id) : base(id)
    {
        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(id, out questData);
        if (questData != null)
        {
            TargetId = questData.arg1;
			TargetValue = questData.arg2;
			CurValue = Mathf.Min(Managers.Inven.GetItemCount(TargetId), TargetValue);
		}

        if (CurValue >= TargetValue)
            FinishQuest();
        else
        {
            Managers.Event.ItemEvents.GainItemAction += CheckQuest;
            Managers.Event.ItemEvents.RemoveItemAction += CheckQuest;
        }
    }

    public void CheckQuest(int itemId, int count, int startSlot, int endSlot)
    {
        if (TargetId == itemId)
        {
            int cnt = Managers.Inven.GetItemCount(itemId);
            CurValue = Mathf.Min(cnt, TargetValue);
        }

        if (CurValue >= TargetValue)
        {
            Managers.Event.ItemEvents.GainItemAction -= CheckQuest;
			Managers.Event.ItemEvents.RemoveItemAction -= CheckQuest;
			FinishQuest();
        }
    }

    public override void ReconnectingEvent()
    {
        Managers.Event.ItemEvents.GainItemAction -= CheckQuest;
		Managers.Event.ItemEvents.RemoveItemAction -= CheckQuest;
		Managers.Event.ItemEvents.GainItemAction += CheckQuest;
		Managers.Event.ItemEvents.RemoveItemAction += CheckQuest;
	}
}

public class KillMonsterQuest : Quest
{
    public KillMonsterQuest(int id) : base(id)
    {
        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(id, out questData);
        if (questData != null)
        {
            TargetId = questData.arg1;
            CurValue = 0;
            TargetValue = questData.arg2;
        }

        //Managers.Event.GainItemEvent += CheckQuest;
    }

    public void CheckQuest(int ObjectId)
    {
        if (TargetId == 0 || TargetId == ObjectId)
        {
            CurValue = Mathf.Min(CurValue + 1, TargetValue);
        }

        if (CurValue >= TargetValue)
        {
            //Managers.Event.GainItemEvent -= CheckQuest;
            FinishQuest();
        }
    }

    public override void ReconnectingEvent()
    {

    }
}

public class UseFeatureQuest : Quest
{
    string _feature;

    public UseFeatureQuest(int id) : base(id)
    {
        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(id, out questData);
        if (questData != null)
        {
            CurValue = 0;
            TargetValue = questData.arg2;
            _feature = questData.str1;
        }

        if (_feature == "Move")
            Managers.Event.PlayerEvents.EnablePlayerMovementAction += CheckQuest;
        else if (_feature == "Dash")
            Managers.Event.PlayerEvents.EnablePlayerDashAction += CheckQuest;
        else if (_feature == "ConnectStarPiece")
            Managers.Event.PlayerEvents.ConnectStarPieceAction += CheckQuest;
        else if (_feature == "Radar")
            Managers.Event.PlayerEvents.ShowRadarAction += CheckQuest;
        else if (_feature == "Sprint")
            Managers.Event.PlayerEvents.PlayerSprintAction += CheckQuest;
	}

    public void CheckQuest()
    {
        CurValue ++;
        CurValue = Mathf.Min(CurValue, TargetValue);

        if (CurValue >= TargetValue)
        {
            if (_feature == "Move")
                Managers.Event.PlayerEvents.EnablePlayerMovementAction -= CheckQuest;
            else if (_feature == "Dash")
                Managers.Event.PlayerEvents.EnablePlayerDashAction -= CheckQuest;
            else if (_feature == "ConnectStarPiece")
                Managers.Event.PlayerEvents.ConnectStarPieceAction -= CheckQuest;
            else if (_feature == "Radar")
                Managers.Event.PlayerEvents.ShowRadarAction -= CheckQuest;
			else if (_feature == "Sprint")
				Managers.Event.PlayerEvents.PlayerSprintAction -= CheckQuest;
			FinishQuest();
        }
    }

    public override void ReconnectingEvent()
    {
        if (_feature == "Move")
        {
            Managers.Event.PlayerEvents.EnablePlayerMovementAction -= CheckQuest;
            Managers.Event.PlayerEvents.EnablePlayerMovementAction += CheckQuest;
        }
        else if (_feature == "Dash")
        {
            Managers.Event.PlayerEvents.EnablePlayerDashAction -= CheckQuest;
            Managers.Event.PlayerEvents.EnablePlayerDashAction += CheckQuest;
        }
        else if (_feature == "ConnectStarPiece")
        {
            Managers.Event.PlayerEvents.ConnectStarPieceAction -= CheckQuest;
            Managers.Event.PlayerEvents.ConnectStarPieceAction += CheckQuest;
        }
        else if (_feature == "Radar")
        {
            Managers.Event.PlayerEvents.ShowRadarAction -= CheckQuest;
			Managers.Event.PlayerEvents.ShowRadarAction += CheckQuest;
		}
		else if (_feature == "Sprint")
		{
			Managers.Event.PlayerEvents.PlayerSprintAction -= CheckQuest;
			Managers.Event.PlayerEvents.PlayerSprintAction += CheckQuest;
		}
	}
}

public class OutOfLocationQuest : Quest
{
    Vector3 _pos;
    float _range;

    public OutOfLocationQuest(int id) : base(id)
    {
        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(id, out questData);
        if (questData != null)
        {
            _pos = new Vector3(questData.arg1, 0.0f, questData.arg2);
            _range = (float)questData.arg3;
        }

        Managers.Event.UpdateAction += CheckQuest;
    }

    public void CheckQuest()
    {
        PlayerController player = Managers.Object.GetPlayer();
        if (player == null)
            return;

        Vector3 dir = player.transform.position - _pos;
        if (Mathf.Abs(dir.x) > _range || Mathf.Abs(dir.z) > _range)
        {
            Managers.Event.UpdateAction -= CheckQuest;
            FinishQuest();
        }
    }

    public override void ReconnectingEvent()
    {
        Managers.Event.UpdateAction -= CheckQuest;
        Managers.Event.UpdateAction += CheckQuest;
    }
}

public class MoveToLocationQuest : Quest
{
    Vector3 _pos;
    float _range;

    public MoveToLocationQuest(int id) : base(id)
    {
        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(id, out questData);
        if (questData != null)
        {
            _pos = new Vector3(questData.arg1, 0.0f, questData.arg2);
            _range = (float)questData.arg3;
        }

        Managers.Event.UpdateAction += CheckQuest;
    }

    public void CheckQuest()
    {
        if (DayTimeType != DayTimeType.None)
        {
            if (DayTimeType == DayTimeType.Night && Managers.Time.IsNight == false)
                return;
			if (DayTimeType == DayTimeType.Day && Managers.Time.IsNight == true)
				return;
		}

        PlayerController player = Managers.Object.GetPlayer();
        if (player == null)
            return;

        Vector3 dir = player.transform.position - _pos;
        if (Mathf.Abs(dir.x) <= _range && Mathf.Abs(dir.z) <= _range)
        {
            Managers.Event.UpdateAction -= CheckQuest;
            FinishQuest();
        }
    }

    public override void ReconnectingEvent()
    {
        Managers.Event.UpdateAction -= CheckQuest;
        Managers.Event.UpdateAction += CheckQuest;
    }
}

public class EquipItemQuest : Quest
{
    public EquipItemQuest(int id) : base(id)
    {
        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(id, out questData);
        if (questData != null)
        {
            TargetId = questData.arg1;
        }

        Managers.Event.ItemEvents.EquipItemAction += CheckQuest;
    }

    public void CheckQuest(int itemId, Define.EquipmentType equipmentType, bool equipped)
    {
        if (equipped == false)
            return;
        if (TargetId == 0 || TargetId == itemId)
        {
            Managers.Event.ItemEvents.EquipItemAction -= CheckQuest;
            FinishQuest();
        }
    }

    public override void ReconnectingEvent()
    {
        Managers.Event.ItemEvents.EquipItemAction -= CheckQuest;
        Managers.Event.ItemEvents.EquipItemAction += CheckQuest;
    }
}

public class UseItemQuest : Quest
{
    public UseItemQuest(int id) : base(id)
    {
        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(id, out questData);
        if (questData != null)
        {
            TargetId = questData.arg1;
            CurValue = 0;
            TargetValue = questData.arg2;
        }

        Managers.Event.ItemEvents.UseItemAction += CheckQuest;
    }

    public void CheckQuest(int itemId)
    {
        if (TargetId == 0 || TargetId == itemId)
        {
            CurValue++;
            CurValue = Mathf.Min(CurValue, TargetValue);
        }

        if (CurValue >= TargetValue)
        {
            Managers.Event.ItemEvents.UseItemAction -= CheckQuest;
            FinishQuest();
        }
    }

    public override void ReconnectingEvent()
    {
        Managers.Event.ItemEvents.UseItemAction -= CheckQuest;
        Managers.Event.ItemEvents.UseItemAction += CheckQuest;
    }
}

public class GameEventQuest : Quest
{
    GameStatus _targetStatus;

	public GameEventQuest(int id) : base(id)
	{
		QuestData questData = null;
		Managers.Data.QuestDict.TryGetValue(id, out questData);
		if (questData != null)
		{
			_targetStatus = questData.status1;
		}

        if (Managers.Game.CheckStatus(_targetStatus))
            FinishQuest();
        else
            Managers.Event.GameEvents.GameEventAction += CheckQuest;

    }

	public void CheckQuest(GameStatus status)
	{
        if (Managers.Game.CheckStatus(_targetStatus))
		{
			Managers.Event.GameEvents.GameEventAction -= CheckQuest;
			FinishQuest();
		}
    }

    public override void ReconnectingEvent()
    {
        Managers.Event.GameEvents.GameEventAction -= CheckQuest;
        Managers.Event.GameEvents.GameEventAction += CheckQuest;
    }
}

public class QuestGroup
{
    public string Title { get; set; }

    public List<Quest> Quests = new List<Quest>();

    public void AddQuest(Quest quest)
    {
        foreach (Quest q in Quests)
            if (q.QuestId == quest.QuestId)
                return;
        Quests.Add(quest);
    }

    public bool IsAllFinished()
    {
        bool finished = true;
        foreach (Quest q in Quests)
            if (q.IsFinished == false)
                finished = false;
        return finished;
    }

    public void ReconnectingEvents()
    {
        foreach (Quest q in Quests)
            if (!q.IsFinished)
                q.ReconnectingEvent();
    }
}

public class BuildingQuest : Quest
{
	public BuildingQuest(int id) : base(id)
	{
		QuestData questData = null;
		Managers.Data.QuestDict.TryGetValue(id, out questData);
		if (questData != null)
		{
			TargetId = questData.arg1;
			CurValue = 0;
			TargetValue = questData.arg2;
		}

        Managers.Event.BuildingEvents.BuildingCompletedAction += CheckQuest;
	}

	public void CheckQuest(int itemId)
	{
		if (TargetId == itemId)
		{
			CurValue += 1;
			CurValue = Mathf.Min(CurValue, TargetValue);
		}

		if (CurValue >= TargetValue)
		{
			Managers.Event.BuildingEvents.BuildingCompletedAction -= CheckQuest;
			FinishQuest();
		}
	}

	public override void ReconnectingEvent()
	{
		Managers.Event.BuildingEvents.BuildingCompletedAction -= CheckQuest;
		Managers.Event.BuildingEvents.BuildingCompletedAction += CheckQuest;
	}
}

