using Monster.Search;
using System;
using UnityEngine;
using UnityEngine.InputSystem;


[System.Serializable]
public class PlayerInputBufferManager
{
    public InputData _bufferedInputData;

    // 입력을 버퍼에 저장 (1개의 매개변수)
    public void BufferInput(InputData inputData)
    {
        ClearBuffer();
        _bufferedInputData = inputData;
    }

    // 버퍼된 입력 처리
    public void ProcessBufferedInput()
    {
        //현재 플레이어가 입력을 받을 수 있는 상태인지 체크 if(CurrentState.IsActionPossible())
        _bufferedInputData?.ProcessInputData();
        ClearBuffer();
    }

    public void ClearBuffer()
    {
        _bufferedInputData = null;
    }

    public bool IsBufferEmpty()
    {
        if (_bufferedInputData == null) return true;
        else return false;
    }
}

public class InputData
{
    public Action _action;

    public InputData(Action action)
    {
        _action = action;
    }

    public void ProcessInputData()
    {
        _action?.Invoke();
    }
}