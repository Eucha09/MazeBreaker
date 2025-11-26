using TreeMonster.Search;
using UnityEngine;

public class DummyController : MonoBehaviour
{
	public CreatureInfo Info { get; set; }
	public bool ChaseTrigger { get; set; }
	public Vector3 ChasingPos { get; set; }
	public Vector3 SpawnPoint { get; set; }

	NavHybridAgent _nma;

	public void Bind(CreatureInfo info)
	{
		Info = info;

		SpawnPoint = Info.SpawnPos;
		ChaseTrigger = Info.ChaseTrigger;
		ChasingPos = Info.ChasingPos;
	}

	public void Unbind()
	{
		Info.ChaseTrigger = ChaseTrigger;
		Info.ChasingPos = ChasingPos;
		Info = null;
	}

	void Start()
    {
        _nma = GetComponent<NavHybridAgent>();

		if (ChaseTrigger)
			SetChasingPos(ChasingPos);
		else
			ChasingEnd();
	}

    void Update()
	{
		if (Info != null)
			Managers.Map.ApplyMove(Info);
	}

	public void SetChasingPos(Vector3 pos, bool chaseTrigger = true)
	{
		ChasingPos = pos;
		ChaseTrigger = chaseTrigger;

		if (_nma != null)
		{
			_nma.SetActive(true);
			_nma.SetDestination(ChasingPos);
		}
	}

	public void ChasingEnd()
	{
		ChaseTrigger = false;

		if (Info != null)
			Info.ReturnSpawnPos();
	}
}
