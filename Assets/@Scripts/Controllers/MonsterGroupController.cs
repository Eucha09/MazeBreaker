using Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI;

public class MonsterGroupController : BaseController
{
	public override bool IsDead
	{
		get 
        {
            foreach (MonsterController2 monster in _monsters)
                if (monster.IsDead)
                    return true;
            return false;
        }
		//set
		//{
  //          if (value == false)
  //          {
  //              foreach (MonsterController2 monster in _monsters)
		//		{
		//			monster.gameObject.SetActive(true);
		//			monster.IsDead = value;
  //              }
  //          }
		//}
	}

	[SerializeField]
    List<MonsterController2> _monsters = new List<MonsterController2>();
    bool _init;

    void Start()
    {
        Init();
    }

    void Init()
    {
        if (_init)
            return;
        _init = true;


        ObjectData objectData = null;
        Managers.Data.ObjectDict.TryGetValue(TemplateId, out objectData);
        MonsterGroupData mgData = objectData as MonsterGroupData;

        //if (_monsters.Count != mgData.monsterIds.Count)
        //    Debug.Log("Error! MonsterGroup Init()");

        //for (int i = 0; i < Mathf.Min(_monsters.Count, mgData.monsterIds.Count); i++)
        //{
        //    _monsters[i].TemplateId = mgData.monsterIds[i];
        //}
    }

    void OnEnable()
    {
        if (_init)
            Init();
	}

	#region Revive
	public override void Revive()
	{
		foreach (MonsterController2 monster in _monsters)
            monster.Revive();
	}
	#endregion
}
