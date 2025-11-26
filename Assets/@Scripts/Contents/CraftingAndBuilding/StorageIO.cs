using UnityEngine;

public class StorageIO : Storage
{
	int _inputSlotCount;
	int _inputStartSlot;
	int _inputEndSlot;

	int _outputSlotCount;
	int _outputStartSlot;
	int _outputEndSlot;

	int _fuelSlotCount;
	int _fuelStartSlot;
	int _fuelEndSlot;

	public int InputSlotCount { get { return _inputSlotCount; } }
	public int InputStartSlot { get { return _inputStartSlot; } }
	public int InputEndSlot { get { return _inputEndSlot; } }
	public int OutputSlotCount { get { return _outputSlotCount; } }
	public int OutputStartSlot { get { return _outputStartSlot; } }
	public int OutputEndSlot { get { return _outputEndSlot; } }
	public int FuelSlotCount { get { return _fuelSlotCount; } }
	public int FuelStartSlot { get { return _fuelStartSlot; } }
	public int FuelEndSlot { get { return _fuelEndSlot; } }

	#region ObjectInfoBinder
	public override void Bind(BuildingObjectInfo info)
	{
		base.Bind(info);
		if (_slotCount < 3)
			return;
		_inputSlotCount = SlotCount / 2;
		_fuelSlotCount = 1;
		_outputSlotCount = _slotCount - _inputSlotCount - _fuelSlotCount;

		_inputStartSlot = _startSlot;
		_inputEndSlot = _inputStartSlot + _inputSlotCount - 1;
		_fuelStartSlot = _inputEndSlot + 1;
		_fuelEndSlot = _fuelStartSlot + _fuelSlotCount - 1;
		_outputStartSlot = _fuelEndSlot + 1;
		_outputEndSlot = _outputStartSlot + _outputSlotCount - 1;
	}

	public override void Unbind()
	{
		Info = null;

		_slotCount = 0;
		_startSlot = 0;
		_endSlot = 0;

		_inputSlotCount = 0;
		_inputStartSlot = 0;
		_inputEndSlot = 0;

		_outputSlotCount = 0;
		_outputStartSlot = 0;
		_outputEndSlot = 0;

		_fuelSlotCount = 0;
		_fuelStartSlot = 0;
		_fuelEndSlot = 0;
	}
	#endregion

	void Start()
	{
		if (Info == null && _slotCount >= 3)
		{
			Managers.Inven.CreateStorage(_slotCount, out _startSlot, out _endSlot);

			_inputSlotCount = SlotCount / 2;
			_fuelSlotCount = 1;
			_outputSlotCount = _slotCount - _inputSlotCount - _fuelSlotCount;

			_inputStartSlot = _startSlot;
			_inputEndSlot = _inputStartSlot + _inputSlotCount - 1;
			_fuelStartSlot = _inputEndSlot + 1;
			_fuelEndSlot = _fuelStartSlot + _fuelSlotCount - 1;
			_outputStartSlot = _fuelEndSlot + 1;
			_outputEndSlot = _outputStartSlot + _outputSlotCount - 1;
		}
	}

	#region Input slots
	public int GetInputItemCount(int templateId)
	{
		if (_inputSlotCount == 0)
			return 0;
		return Managers.Inven.GetItemCount(templateId, _inputStartSlot, _inputEndSlot);
	}

	public int AddToInput(Item item)
	{
		if (_inputSlotCount == 0)
			return item.Count;
		return Managers.Inven.Add(item, _inputStartSlot, _inputEndSlot);
	}

	public void RemoveFromInput(int templateId, int count)
	{
		if (_inputSlotCount == 0)
			return;
		Managers.Inven.Remove(templateId, count, _inputStartSlot, _inputEndSlot);
	}
	#endregion

	#region Fuel slots
	public int GetFuelItemCount(int templateId)
	{
		if (_fuelSlotCount == 0)
			return 0;
		return Managers.Inven.GetItemCount(templateId, _fuelStartSlot, _fuelEndSlot);
	}

	public int AddToFuel(Item item)
	{
		if (_fuelSlotCount == 0)
			return item.Count;
		return Managers.Inven.Add(item, _fuelStartSlot, _fuelEndSlot);
	}

	public void RemoveFromFuel(int templateId, int count)
	{
		if (_fuelSlotCount == 0)
			return;
		Managers.Inven.Remove(templateId, count, _fuelStartSlot, _fuelEndSlot);
	}
	#endregion

	#region Output slots
	public int GetOutputItemCount(int templateId)
	{
		if (_outputSlotCount == 0)
			return 0;
		return Managers.Inven.GetItemCount(templateId, _outputStartSlot, _outputEndSlot);
	}

	public int AddToOutput(Item item)
	{
		if (_outputSlotCount == 0)
			return item.Count;
		return Managers.Inven.Add(item, _outputStartSlot, _outputEndSlot);
	}

	public void RemoveFromOutput(int templateId, int count)
	{
		if (_outputSlotCount == 0)
			return;
		Managers.Inven.Remove(templateId, count, _outputStartSlot, _outputEndSlot);
	}
	#endregion
}
