using AmazingAssets.DynamicRadialMasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using static Define;

public enum GameStatus
{
	//Enable_GladeDoor,
	Enable_Interaction,
	Enable_Inventory,

	TimePasses,
    //GoOutGlade,
	ChangingClothes,
    PickUpRadar,
	//GuardsNotebook,
	TutorialComplete,

	SectorA_EnergySupply_Found,
	SectorA_EnergySupply_1,
	SectorA_EnergySupply_2,

	DemoComplete,

	MaxCount,
}

public class GameManagerEx
{
    bool[] _statuses = new bool[(int)GameStatus.MaxCount];
	public HashSet<int> UnLockedCrafting { get; set; } = new HashSet<int>();
	public HashSet<int> UnLockedBuilding { get; set; } = new HashSet<int>();

	public int RespawnPeriod { get; private set; } = 1;

	public void Init()
    {

    }

    public void SetStatus(GameStatus status, bool value)
    {
        _statuses[(int)status] = true;
        Managers.Event.GameEvents.OnGameEvent(status);
    }

    public void SetStatus(String status, int cnt)
    {
        for (int i = 1; cnt > 0; i++)
        {
            GameStatus s;
			if (System.Enum.TryParse<GameStatus>(status + "_" + i, out s))
			{
				if (_statuses[(int)s] == false)
				{
					SetStatus(s, true);
					cnt--;
				}
			}
			else
                break;
		}
    }

    public bool CheckStatus(GameStatus status)
    {
        return _statuses[(int)status];
    }

    public bool CheckStatus(String status)
	{
		bool ret = false;
		for (int i = 1; ; i++)
		{
			GameStatus s;
			if (System.Enum.TryParse<GameStatus>(status + "_" + i, out s))
			{
				ret = true;
				if (_statuses[(int)s] == false)
					return false;
			}
			else
				break;
		}
		return ret;
	}

    public void Clear()
    {

    }
}
