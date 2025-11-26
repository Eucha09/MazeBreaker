using System;
using UnityEngine;

public class QuestEvents
{
    public event Action<int> StartQuestAction;
    public void OnStartQuest(int id)
    {
        if (StartQuestAction != null)
        {
            StartQuestAction(id);
        }
    }

    public event Action<int> FinishQuestAction;
    public void OnFinishQuest(int id)
    {
        if (FinishQuestAction != null)
        {
            FinishQuestAction(id);
        }
    }

    public void Clear()
    {
        StartQuestAction = null;
        FinishQuestAction = null;
    }
}
