using System;
using UnityEngine;

public class TimeEvents
{
	public event Action DayAction;
	public void OnDay()
	{
		if (DayAction != null)
			DayAction();
	}

	public event Action NightAction;
	public void OnNight()
	{
		if (NightAction != null)
			NightAction();
	}

	public event Action DaysChangeAction;
	public void OnDaysChange()
	{
		if (DaysChangeAction != null)
			DaysChangeAction();
	}

	public void Clear()
	{
		DayAction = null;
		NightAction = null;
		DaysChangeAction = null;
	}
}
