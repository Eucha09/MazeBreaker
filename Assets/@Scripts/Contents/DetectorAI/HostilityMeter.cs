using UnityEngine;
using static Define;

public class HostilityMeter
{
	public DetectorType Type { get; private set; }
	public float Value { get; private set; }
	public float MaxValue { get; private set; } = 100f;
	public bool IsTriggered => Value >= MaxValue;
	public bool IsActive { get; set; } = false;

	private float _decaySpeed = 2f; // 초당 감소량

	public HostilityMeter(DetectorType type)
	{
		Type = type;
		Value = 0f;
	}

	public void Increase(float amount)
	{
		Value = Mathf.Min(Value + amount, MaxValue);

		(Managers.UI.SceneUI as UI_GameScene)?.HostilityMetersUI?.UpdateHostilityMeters(this);
	}

	public void Decrease(float deltaTime)
	{
		if (Value > 0)
			Value = Mathf.Max(Value - _decaySpeed * deltaTime, 0f);

		(Managers.UI.SceneUI as UI_GameScene)?.HostilityMetersUI?.UpdateHostilityMeters(this);
	}

	public void Reset()
	{
		Value = 0f;

		(Managers.UI.SceneUI as UI_GameScene)?.HostilityMetersUI?.UpdateHostilityMeters(this);
	}
}
