using System;
using UnityEngine;

public class PlayerEvents
{
    public Action DisablePlayerMovementAction;
    public void OnDisablePlayerMovement()
    {
        if (DisablePlayerMovementAction != null)
            DisablePlayerMovementAction();
    }

    public Action EnablePlayerMovementAction;
    public void OnEnablePlayerMovement()
    {
        if (EnablePlayerMovementAction != null)
            EnablePlayerMovementAction();
    }

    public Action EnablePlayerDashAction;
    public void OnEnablePlayerDash()
    {
        if (EnablePlayerDashAction != null)
            EnablePlayerDashAction();
    }

    public Action PlayerSprintAction;
    public void OnPlayerSprint()
    {
        if (PlayerSprintAction != null)
            PlayerSprintAction();
    }

	public Action ConnectStarPieceAction;
    public void OnConnectStarPiece()
    {
        if (ConnectStarPieceAction != null)
            ConnectStarPieceAction();
    }

    public Action ShowRadarAction;
	public void OnShowRadar()
	{
		if (ShowRadarAction != null)
			ShowRadarAction();
	}

    public Action<BaseController> AttackAction;
    public void OnAttack(BaseController Target)
    {
        if (AttackAction != null)
            AttackAction(Target);
	}

    public Action<BaseController> DetectedByMonsterAction;
    public void OnDetectedByMonster(BaseController monster)
    {
        if (DetectedByMonsterAction != null)
            DetectedByMonsterAction(monster);
	}

	public void Clear()
    {
        DisablePlayerMovementAction = null;
        EnablePlayerMovementAction = null;
        EnablePlayerDashAction = null;
        ConnectStarPieceAction = null;
        ShowRadarAction = null;
	}
}
