using UnityEngine;

public class Minimap_Zone : MonoBehaviour
{
	[SerializeField]
	int _range;
	[SerializeField]
	Color _color;

	public int Range { get { return _range; } set { _range = value; } }
	public Color Color { get { return _color; } set { _color = value; } }

	SpriteRenderer _renderer;

	public void SetInfo(Vector3 centerPos, int range, Color color)
	{
		_range = range;
		_color = color;

		centerPos.y = -20.0f;
		transform.position = centerPos;

		_renderer = GetComponent<SpriteRenderer>();
		_renderer.color = _color;

		float scaleFactor = (_range * 2 + 1) * 3.0f;
		transform.localScale = new Vector3(scaleFactor, scaleFactor, 1.0f);
	}
}
