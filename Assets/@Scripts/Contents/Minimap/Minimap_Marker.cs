using Data;
using UnityEngine;
using UnityEngine.Splines;

public class Minimap_Marker : MonoBehaviour
{
    [SerializeField]
    Sprite _icon;
    [SerializeField]
    float _iconSize;
    [SerializeField]
    bool _isIndicator;
    [SerializeField]
    Transform _syncPositionTarget;
    [SerializeField]
    Transform _syncRotationTarget;
    [SerializeField]
    bool _isPin;

    public int TemplateId { get; set; }
	public Sprite Icon { get { return _icon; } set { _icon = value; } }
    public float IconSize { get { return _iconSize; } set { _iconSize = value; } }
    public bool IsIndicator { get { return _isIndicator; } set { _isIndicator = value; } }
    public Transform SyncPositionTarget { get { return _syncPositionTarget; } set { _syncPositionTarget = value; } }
    public Transform SyncRotationTarget { get { return _syncRotationTarget; } set { _syncRotationTarget = value; } }
    public bool IsPin { get { return _isPin; } set { _isPin = value; } }

    SpriteRenderer _renderer;
    public bool IsStarPiece { get; set; }
    public Vector3 StarPiecePos { get; set; }

    public void SetTemplateId(int templateId, Vector3 pos)
	{
        TemplateId = templateId;
		MinimapMarkerData data = null;
		Managers.Data.MinimapMarkerDict.TryGetValue(templateId, out data);
		if (data == null)
			return;

		gameObject.name = $"MinimapObject_{data.name}";

		_icon = Managers.Resource.Load<Sprite>(data.iconPath);
		_renderer = GetComponent<SpriteRenderer>();
		_renderer.sprite = _icon;

        SetPosition(pos);

		_iconSize = data.size;
		Vector2 spriteSize = Icon.bounds.size;
		float scaleFactor = 0.2f * _iconSize / Mathf.Max(spriteSize.x, spriteSize.y);
		transform.localScale = Vector3.one * scaleFactor;

        if (data.syncToCameraRotation)
            SetSyncRotationTarget(Camera.main.transform);

		_isIndicator = data.isIndicator;
		_isPin = data.isPin;
	}

    public void SetPosition(Vector3 pos)
    {
        pos.y = -20.0f;
        transform.position = pos;
	}

	public void SetSyncPositionTarget(Transform target)
    {
        SyncPositionTarget = target;
        GetComponent<SynchronousPosition>().SetTarget(SyncPositionTarget);
    }

    public void SetSyncRotationTarget(Transform target)
    {
        SyncRotationTarget = target;
        GetComponent<SynchronousRotation>().SetSynchronizeTarget(SyncRotationTarget);
    }
}