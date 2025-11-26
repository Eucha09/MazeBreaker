using Data;
using JetBrains.Annotations;
using StylizedWater3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class QuestManager
{
    public List<QuestGroup> CurQuests { get; set; } = new List<QuestGroup>();
    public HashSet<int> FinishedQuest { get; set; } = new HashSet<int>();

    public void Init()
    {

    }

    public void StartQuest(int questId)
    {
        QuestData questData = null;
        Managers.Data.QuestDict.TryGetValue(questId, out questData);
        if (questData == null)
            return;

		foreach (GameStatus status in questData.statusToUpdate_Before)
			Managers.Game.SetStatus(status, true);
		foreach (int craftingId in questData.unlockCrafting_Before)
			Managers.Game.UnLockedCrafting.Add(craftingId);
		foreach (int craftingId in questData.unlockBuilding_Before)
			Managers.Game.UnLockedBuilding.Add(craftingId);

		Managers.Event.QuestEvents.OnStartQuest(questId);

        if (FinishedQuest.Contains(questId))
        {
            FinishQuest(questId);
            return;
        }

		//TODO
		Quest quest = null;
        if (questData.questType == QuestType.CollectItem)
            quest = new CollectItemQuest(questId);
        else if (questData.questType == QuestType.UseFeature)
            quest = new UseFeatureQuest(questId);
        else if (questData.questType == QuestType.UseItem)
            quest = new UseItemQuest(questId);
        else if (questData.questType == QuestType.EquipItem)
            quest = new EquipItemQuest(questId);
        else if (questData.questType == QuestType.OutOfLocation)
            quest = new OutOfLocationQuest(questId);
        else if (questData.questType == QuestType.MoveToLocation)
            quest = new MoveToLocationQuest(questId);
        else if (questData.questType == QuestType.GameEvent)
            quest = new GameEventQuest(questId);
		else if (questData.questType == QuestType.Building)
			quest = new BuildingQuest(questId);
		else
            quest = new Quest(questId);

        bool groupExists = false;
        foreach (QuestGroup qg in CurQuests)
        {
            if (qg.Title == questData.title)
            {
                qg.AddQuest(quest);
                groupExists = true;
            }
        }

        if (groupExists == false)
        {
            QuestGroup questGroup = new QuestGroup();
            questGroup.Title = questData.title;
            questGroup.AddQuest(quest);
            CurQuests.Add(questGroup);
            UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
            ui.AddQuestGroupUI(questGroup);
        }
    }

    public void FinishQuest(int questId)
	{
		QuestData questData = null;
		Managers.Data.QuestDict.TryGetValue(questId, out questData);

		foreach (GameStatus status in questData.statusToUpdate_After)
			Managers.Game.SetStatus(status, true);

		FinishedQuest.Add(questId);
        Managers.Event.QuestEvents.OnFinishQuest(questId);

        if (questData.nextQuestIds != null)
        {
            foreach (int nextId in questData.nextQuestIds)
            {
                QuestData nextQuestData = null;
                Managers.Data.QuestDict.TryGetValue(nextId, out nextQuestData);
                if (nextQuestData == null)
                    continue;
                bool possible = true;
                foreach (int priorId in nextQuestData.priorQuestIds)
                    if (FinishedQuest.Contains(priorId) == false)
                        possible = false;

                if (possible)
                    StartQuest(nextId);
            }
        }

        RefreshQuest();
    }

    public void RefreshQuest()
    {
        for (int i = CurQuests.Count - 1; i >= 0; i--)
            if (CurQuests[i].IsAllFinished())
                CurQuests.RemoveAt(i);
    }

    public void ReconnectingQuests()
	{
        UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
        if (ui == null)
            return;
		foreach (QuestGroup q in CurQuests)
		{
			q.ReconnectingEvents();
			ui.AddQuestGroupUI(q);
		}
	}

    public void Clear()
    {
        //TODO
        //CurQuests.Clear();
    }
}
