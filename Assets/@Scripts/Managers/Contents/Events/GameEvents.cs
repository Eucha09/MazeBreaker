using System;
using UnityEngine;

public class GameEvents
{
	public event Action<GameStatus> GameEventAction;
	public void OnGameEvent(GameStatus status)
	{
		if (GameEventAction != null)
			GameEventAction(status);
	}

	public Action LoadOrUnloadContentAction;
	public void OnLoadOrUnloadContent()
	{
		if (LoadOrUnloadContentAction != null)
			LoadOrUnloadContentAction();
	}

	public Action MazeChangedAction;
	public void OnMazeChanged()
	{
		if (MazeChangedAction != null)
			MazeChangedAction();
	}

	public void Clear()
	{
		GameEventAction = null;
		LoadOrUnloadContentAction = null;
		MazeChangedAction = null;
	}
}
