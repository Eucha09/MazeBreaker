using System.Collections.Generic;
using UnityEngine;

public class SelectedOutline : MonoBehaviour
{
	[SerializeField]
	Material _outline;
	[SerializeField]
	Renderer _renderer;

	List<Material> _materialList = new List<Material>();

	public void DrawOutline()
	{

		_materialList.Clear();
		_materialList.AddRange(_renderer.sharedMaterials);
		_materialList.Add(_outline);

		_renderer.materials = _materialList.ToArray();
	}

	public void ClearOutline()
	{

		_materialList.Clear();
		_materialList.AddRange(_renderer.sharedMaterials);
		_materialList.Remove(_outline);

		_renderer.materials = _materialList.ToArray();
	}
}
