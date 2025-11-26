using UnityEngine;

public class EnergySupplyController : StructureController
{
	[SerializeField]
	string _keyStr;


	protected override void OnDead()
	{
		if (IsDead)
			return;

		//_isDead = true;

		if (_keyStr != null && _keyStr.Length > 0)
			Managers.Game.SetStatus(_keyStr, 1);
		Managers.Object.Remove(gameObject);
	}
}
