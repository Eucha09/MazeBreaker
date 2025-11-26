using AmazingAssets.DynamicRadialMasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using VolumetricFogAndMist2;

public class DayAndNight : MonoBehaviour
{
	[SerializeField]
    Texture2D _skyboxDay;
	[SerializeField]
	Texture2D _skyboxNight;

	Light _globalLight;
	[SerializeField]
	Gradient _gradientDayToNight;
	[SerializeField]
	Gradient _gradientNightToDay;

	[SerializeField]
	float _fogEndDistanceDay;
	[SerializeField]
	float _fogEndDistanceNight;

    VolumetricFog _volumetricFog;
	[SerializeField]
	float _volumetricFogAlphaDay;
	[SerializeField]
	float _volumetricFogAlphaNight;

	[SerializeField]
    Volume _globalVolume;

	public GameObject Fog { get; set; }

	[SerializeField]
	string _msgText;
	float _msgShowTime;
	bool _showMsg;

    public void Init()
	{
		Managers.Event.TimeEvents.NightAction += OnNight;
		Managers.Event.TimeEvents.DayAction += OnDay;

		Fog = Managers.Resource.Instantiate("Environment/FogVolume");
		_volumetricFog = Fog.GetComponent<VolumetricFog>();
		Fog.GetComponent<SynchronousPosition>().SetTarget(Managers.Object.GetPlayer().transform);

		_globalLight = GetComponent<Light>();

		// TEMP
		RenderSettings.skybox.SetTexture("_Texture1", _skyboxDay);
		RenderSettings.skybox.SetFloat("_Blend", 0);
		_globalLight.color = _gradientDayToNight.Evaluate(0.0f);
		RenderSettings.fogColor = _globalLight.color;
		_volumetricFog.profile.albedo.a = 0;

		_msgShowTime = Managers.Time.DayLength * 3.0f / 4.0f;

	}

    void Update()
    {
		if (Managers.Game.CheckStatus(GameStatus.TimePasses) == false)
			return;

		if (_showMsg == false && Managers.Time.IsNight == false && Managers.Time.SecondsToday > _msgShowTime)
		{
			UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
			if (ui != null)
			{
				ui.ShowSysMsg(_msgText, 5.0f);
				_showMsg = true;
			}
		}


		_globalLight.transform.Rotate(Vector3.up, (360f / Managers.Time.FullCycleLength) * Time.deltaTime, Space.World);
		Managers.Time.SecondsToday += Time.deltaTime;
	}

    public void OnNight()
	{
		Managers.Sound.Play("DaytoNight");
		StartCoroutine(CoLerpSkybox(_skyboxDay, _skyboxNight, 10f));
		StartCoroutine(CoLerpLight(_gradientDayToNight, 10f));
		StartCoroutine(CoLerpFogEndDistance(_fogEndDistanceDay, _fogEndDistanceNight, 10f));
		StartCoroutine(CoLerpFogVolume(_volumetricFogAlphaDay, _volumetricFogAlphaNight, 10f));
	}

	public void OnDay()
	{
        _showMsg = false;
        Managers.Sound.Play("NighttoDay");
		StartCoroutine(CoLerpSkybox(_skyboxNight, _skyboxDay, 10f));
		StartCoroutine(CoLerpLight(_gradientNightToDay, 10f));
		StartCoroutine(CoLerpFogEndDistance(_fogEndDistanceNight, _fogEndDistanceDay, 10f));
		StartCoroutine(CoLerpFogVolume(_volumetricFogAlphaNight, _volumetricFogAlphaDay, 10f));
	}

	IEnumerator CoLerpSkybox(Texture2D a, Texture2D b, float time)
	{
		RenderSettings.skybox.SetTexture("_Texture1", a);
		RenderSettings.skybox.SetTexture("_Texture2", b);
		RenderSettings.skybox.SetFloat("_Blend", 0);
		for (float i = 0; i < time; i += Time.deltaTime)
		{
			RenderSettings.skybox.SetFloat("_Blend", i / time);
			yield return null;
		}
		RenderSettings.skybox.SetTexture("_Texture1", b);
	}

	IEnumerator CoLerpLight(Gradient lightGradient, float time)
	{
		for (float i = 0; i < time; i += Time.deltaTime)
		{
			_globalLight.color = lightGradient.Evaluate(i / time);
			RenderSettings.fogColor = _globalLight.color;
			yield return null;
		}
	}

	IEnumerator CoLerpFogEndDistance(float endDistance1, float endDistance2, float time)
	{
		for (float i = 0; i < time; i += Time.deltaTime)
		{
			RenderSettings.fogEndDistance = Mathf.Lerp(endDistance1, endDistance2, i / time);
			yield return null;
		}
		RenderSettings.fogEndDistance = endDistance2;
	}

	IEnumerator CoLerpFogVolume(float fogAlpha1, float fogAlpha2, float time)
	{
		for (float i = 0; i < time; i += Time.deltaTime)
		{
			_volumetricFog.profile.albedo.a = Mathf.Lerp(fogAlpha1, fogAlpha2, i / time);
			yield return null;
		}
        _volumetricFog.profile.albedo.a = fogAlpha2;
	}

	/*
	void TransformMonsters()
	{
		List<GameObject> monsters = Managers.Object.FindAll((go) => { return go.GetComponent<MonsterController2>() != null; });

		foreach (GameObject monster in monsters)
		{
			monster.GetComponent<MonsterController2>().TransformByTime(Managers.Time.DayTimeType);
		}
	}
	*/
}
