using UnityEngine;
using static Define;

public class TimeManager
{
	// in seconds
	float _fullCycleLength = 1.0f;
	float _dayLength;
	float _nightLength;

	float _dayRatio;
	float _nightRatio;
	float _curTimeRatio;

	int _days;
	float _secondsToday;
	double _curtime;
	bool _isNight;

	public float FullCycleLength { get { return _fullCycleLength; } }
	public float DayLength { get { return _dayLength; } }
	public float NightLength { get { return _nightLength; } }

	public float DayRatio { get { return _dayRatio; } }
	public float NightRatio { get { return _nightRatio; } }
	public float CurTimeRatio { get { return _curTimeRatio; } }

	public bool IsNight { get { return _isNight; } }
	public DayTimeType DayTimeType { get { return _isNight ? DayTimeType.Night : DayTimeType.Day; } }

	public int Days { get { return _days; } }
	public double CurTime { get { return _curtime; } }
	public float SecondsToday
	{ 
		get { return _secondsToday; }
		set
		{
			_secondsToday = value;
			_curtime = (double)_days * _fullCycleLength + _secondsToday;
			if (_secondsToday >= _fullCycleLength)
			{
				_secondsToday -= _fullCycleLength;
				_days++;
				Managers.Event.TimeEvents.OnDaysChange();
			}
			_curTimeRatio = _secondsToday / _fullCycleLength;

			if (_isNight == false && _secondsToday >= _dayLength)
			{
				_isNight = true;
				Managers.Event.TimeEvents.OnNight();
			}
			else if (_isNight == true && _secondsToday < _dayLength)
			{
				_isNight = false;
				Managers.Event.TimeEvents.OnDay();
			}
		}
	}

	public void Init()
	{
		// load Data
		_days = 1;
		_secondsToday = 0.0f;
		_curtime = 0.0;
		_isNight = false;
		SetDayAndNightLength(0.5f, 0.5f);
	}

	public void SetDayAndNightLength(float dayLengthInMinutes, float nightLengthMinutes)
	{
		_dayLength = dayLengthInMinutes * 60.0f;
		_nightLength = nightLengthMinutes * 60.0f;
		_fullCycleLength = _dayLength + _nightLength;

		_dayRatio = _dayLength / _fullCycleLength;
		_nightRatio = _nightLength / _fullCycleLength;
	}


	public void Clear()
	{
		_days = 1;
		_secondsToday = 0.0f;
		_curtime = 0.0;
		_isNight = false;
	}
}
