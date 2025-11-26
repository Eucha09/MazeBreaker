using System;
using UnityEngine;
using static Define;

public class BuildingEvents
{
	public Action<int> BuildingCompletedAction;
	public void OnBuildingCompleted(int templateId)
	{
		if (BuildingCompletedAction != null)
			BuildingCompletedAction(templateId);
	}

	public void Clear()
	{
		BuildingCompletedAction = null;
	}
}
