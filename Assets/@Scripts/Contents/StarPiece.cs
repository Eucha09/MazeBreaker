using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class StarPiece : MonoBehaviour
{
	[SerializeField]
	private float _maxPositiveEnergy = 100.0f;
	[SerializeField]
	private float _currentPositiveEnergy = 100.0f;

	public float MaxPositiveEnergy { get { return _maxPositiveEnergy; } set { _maxPositiveEnergy = value; } }
	public float CurrentPositiveEnergy 
	{ 
		get { return _currentPositiveEnergy; }
		set 
		{ 
			_currentPositiveEnergy = value;
			RewardObject rewardObject = GetComponentInParent<RewardObject>();

			if (_currentPositiveEnergy == 0.0f)
			{
				Disconnect();
				Managers.Resource.Destroy(GetComponentInParent<Rigidbody>().gameObject);
			}

			if (rewardObject != null && _currentPositiveEnergy <= 10.0f)
				rewardObject.ItemId = 311;
			else if (rewardObject != null && _currentPositiveEnergy > 10.0f)
				rewardObject.ItemId = 307;
		}
	}

	public List<StarEdge> Edges { get; set; } = new List<StarEdge>();

    void Start()
    {
		RewardObject rewardObject = GetComponentInParent<RewardObject>();
		if (rewardObject != null && rewardObject.ItemId == 311)
			_currentPositiveEnergy = 10.0f;

		//Connect();
	}

	public void Connect()
	{
		transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);

		List<Minimap_Marker> objs = Managers.Minimap.Markers;
		foreach (Minimap_Marker obj in objs)
		{
			StarPiece target = obj.SyncPositionTarget.GetComponent<StarPiece>();
			if (target == null || target == this)
				continue;

			Vector3 dir = target.transform.position - transform.position;
			if (dir.magnitude > 18.0f * 5.0f) // 5칸
				continue;

			// 이미 연결되어 있으면
			if (GetEdgeConnectedTo(target) != null)
				continue;

			RaycastHit hit;
			if (Physics.Raycast(transform.position + Vector3.up, dir, out hit, dir.magnitude, 1 << (int)Define.Layer.Block))
				continue;

			// edge 생성
			// 일단 여기 sound here
			Managers.Sound.Play("UISound/StarSucceed");
			StarEdge starEdge = Managers.Resource.Instantiate("Nature/StarEdge").GetComponent<StarEdge>();
			starEdge.Init(gameObject, target.gameObject);
			Edges.Add(starEdge);
			target.Edges.Add(starEdge);
			Managers.Event.PlayerEvents.OnConnectStarPiece();
		}

		Managers.Star.DrawConstellation(transform.position);
	}

    public void Disconnect()
	{
		for (int i = Edges.Count - 1; i >= 0; i--)
			Edges[i].Disconnect();
		Edges.Clear();
	}

    public StarEdge GetEdgeConnectedTo(StarPiece star)
    {
        foreach (StarEdge edge in Edges)
        {
            if (edge.FirstObject == star.gameObject || edge.SecondObject == star.gameObject)
                return edge;
        }
        return null;
    }

    public List<StarPiece> GetConnectedStars()
    {
        List<StarPiece> stars = new List<StarPiece>();
        foreach (StarEdge edge in Edges)
        {
            if (edge.Active == false)
                continue;

            if (edge.FirstObject == gameObject)
                stars.Add(edge.SecondObject.GetComponent<StarPiece>());
            else if (edge.SecondObject == gameObject)
                stars.Add(edge.FirstObject.GetComponent<StarPiece>());
        }
        return stars;
    }
}
