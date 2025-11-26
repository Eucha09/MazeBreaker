using Data;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class NatureInfo : CreatureInfo
{
	public List<int> RewardList { get; set; }
	public int DropsGiven { get; set; }

	public override void Init(int objectId)
	{
		base.Init(objectId);

		Type = ObjectInfoType.Nature;
		RewardList = null;
		DropsGiven = 0;
	}
}