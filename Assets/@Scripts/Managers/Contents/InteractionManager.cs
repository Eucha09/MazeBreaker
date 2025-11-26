using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionManager
{
	public void Push(PatternedWall wall, Vector3 dir, List<StarPiece> path)
	{
		PlayerController player = Managers.Object.GetPlayer();

		//foreach (StarPiece s in path)
		//	s.CurrentPositiveEnergy = Mathf.Max(0.0f, s.CurrentPositiveEnergy - 10.0f);

		player.Stats.CurrentPositiveEnergy = Mathf.Max(0.0f, player.Stats.CurrentPositiveEnergy - 150.0f);

		wall.Push(dir);
	}

	public void Fall(PatternedWall wall, Vector3 dir, List<StarPiece> path)
	{
		PlayerController player = Managers.Object.GetPlayer();

		//foreach (StarPiece s in path)
		//	s.CurrentPositiveEnergy = Mathf.Max(0.0f, s.CurrentPositiveEnergy - 10.0f);

		player.Stats.CurrentPositiveEnergy = Mathf.Max(0.0f, player.Stats.CurrentPositiveEnergy - 150.0f);

		wall.Fall(dir);
	}

	public void Up(PatternedWall wall, List<StarPiece> path)
	{
		PlayerController player = Managers.Object.GetPlayer();

		//foreach (StarPiece s in path)
		//	s.CurrentPositiveEnergy = Mathf.Max(0.0f, s.CurrentPositiveEnergy - 10.0f);

		player.Stats.CurrentPositiveEnergy = Mathf.Max(0.0f, player.Stats.CurrentPositiveEnergy - 150.0f);

		wall.Up();
	}

	public void Down(PatternedWall wall, List<StarPiece> path)
	{
		PlayerController player = Managers.Object.GetPlayer();

		//foreach (StarPiece s in path)
		//	s.CurrentPositiveEnergy = Mathf.Max(0.0f, s.CurrentPositiveEnergy - 10.0f);

		player.Stats.CurrentPositiveEnergy = Mathf.Max(0.0f, player.Stats.CurrentPositiveEnergy - 150.0f);

		wall.Down();
	}

	public void Exchange(GameObject obj1, GameObject obj2, List<StarPiece> path)
	{
		PlayerController player = Managers.Object.GetPlayer();

		//foreach (StarPiece s in path)
		//	s.CurrentPositiveEnergy = Mathf.Max(0.0f, s.CurrentPositiveEnergy - 10.0f);

		player.Stats.CurrentPositiveEnergy = Mathf.Max(0.0f, player.Stats.CurrentPositiveEnergy - 150.0f);

		Rigidbody rb1 = obj1.GetComponentInParent<Rigidbody>();
		Rigidbody rb2 = obj2.GetComponentInParent<Rigidbody>();
		Vector3 pos1 = rb1.gameObject.transform.position;
		Vector3 pos2 = rb2.gameObject.transform.position;

		rb1.position = pos2;
		rb1.transform.position = pos2;
		rb2.position = pos1;
		rb2.transform.position = pos1;

		if (obj1.GetComponent<MazeWall>() && obj2.GetComponent<MazeWall>())
		{
			//Managers.Map.ExchangedWall(obj1.GetComponent<MazeWall>(), obj2.GetComponent<MazeWall>());
		}
		if (obj1.GetComponent<StarPiece>())
		{
			obj1.GetComponent<StarPiece>().Disconnect();
			obj1.GetComponent<StarPiece>().Connect();
		}
		if (obj2.GetComponent<StarPiece>())
		{
			obj2.GetComponent<StarPiece>().Disconnect();
			obj2.GetComponent<StarPiece>().Connect();
		}
	}

	public void Teleport(Vector3 pos, List<StarPiece> path)
	{
		PlayerController player = Managers.Object.GetPlayer();

		//foreach (StarPiece s in path)
		//	s.CurrentPositiveEnergy = Mathf.Max(0.0f, s.CurrentPositiveEnergy - 10.0f);

		player.Stats.CurrentPositiveEnergy = Mathf.Max(0.0f, player.Stats.CurrentPositiveEnergy - 150.0f);

		player.GetComponent<Rigidbody>().position = pos;
		player.transform.position = pos;
	}
}
