using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    List<GameObject> _battleMonsters = new List<GameObject>();

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            _audioSources[(int)Define.Sound.Bgm].loop = true;
            _audioSources[(int)Define.Sound.BattleBgm].loop = true;
        }
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        _audioClips.Clear();
        _battleMonsters.Clear();

	}

    public void Play(string path, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type, pitch);
    }

	public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
	{
        if (audioClip == null)
            return;

		if (type == Define.Sound.Bgm)
		{
			AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
			if (audioSource.isPlaying)
				audioSource.Stop();

			audioSource.pitch = pitch;
			audioSource.clip = audioClip;

            if (_audioSources[(int)Define.Sound.BattleBgm].isPlaying == false)
    			audioSource.Play();
		}
        else if (type == Define.Sound.BattleBgm)
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.BattleBgm];
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();

			audioSource = _audioSources[(int)Define.Sound.Bgm];
			if (audioSource.isPlaying)
				audioSource.Stop();
		}
        else
		{
			AudioSource audioSource = _audioSources[(int)Define.Sound.Effect];
			audioSource.pitch = pitch;
			audioSource.PlayOneShot(audioClip);
		}
    }

    public void Play3DSound(GameObject go, string path, float minDistance, float maxDistance)
    {
        AudioClip audioClip = GetOrAddAudioClip(path);
        Play3DSound(go, audioClip, minDistance, maxDistance);
    }

    public void Play3DSound(GameObject go, AudioClip audioClip, float minDistance, float maxDistance)
    {
        AudioSource audio = go.GetOrAddComponent<AudioSource>();
        audio.spatialBlend = 1;
        audio.rolloffMode = AudioRolloffMode.Linear;
        audio.minDistance = minDistance;
        audio.maxDistance = maxDistance;
        audio.PlayOneShot(audioClip);
    }

    public void PlayBattleBGM(GameObject go, string audioPath)
	{
		AudioClip audioClip = GetOrAddAudioClip(audioPath);
		PlayBattleBGM(go, audioClip);
	}

    public void PlayBattleBGM(GameObject go, AudioClip audioClip)
    {
        _battleMonsters.Add(go);

        Play(audioClip, Define.Sound.BattleBgm);
    }

    public void EndBattle(GameObject go)
    {
        _battleMonsters.Remove(go);

        if (_battleMonsters.Count == 0)
            StopBattleBGM();
    }

    public void StopBattleBGM()
    {
		if (_audioSources[(int)Define.Sound.BattleBgm].isPlaying)
			_audioSources[(int)Define.Sound.BattleBgm].Stop();

		_audioSources[(int)Define.Sound.Bgm].Play();
	}

    public AudioClip GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.Effect)
    {
		if (path.Contains("Sounds/") == false)
			path = $"Sounds/{path}";

		AudioClip audioClip = null;

		if (type == Define.Sound.Bgm)
		{
			audioClip = Managers.Resource.Load<AudioClip>(path);
		}
		else
		{
			if (_audioClips.TryGetValue(path, out audioClip) == false)
			{
				audioClip = Managers.Resource.Load<AudioClip>(path);
				_audioClips.Add(path, audioClip);
			}
		}

		if (audioClip == null)
			Debug.Log($"AudioClip Missing ! {path}");

		return audioClip;
    }


    //3d Loop 사운드 - By 창수-----------------------------------------------------------------------------------------------------------
    public AudioSource Play3DLoop(GameObject go, AudioClip audioClip, float minDistance = 0f, float maxDistance = 54f)
    {
        AudioSource audio = go.GetOrAddComponent<AudioSource>();
        audio.clip = audioClip;
        audio.spatialBlend = 1;
        audio.rolloffMode = AudioRolloffMode.Linear;
        audio.minDistance = minDistance;
        audio.maxDistance = maxDistance;
        audio.loop = true;
        audio.Play();
        return audio;
    }

    public void Stop3DLoop(AudioSource audio)
    {
        if (audio != null)
        {
            audio.Stop();
            GameObject.Destroy(audio);
        }
    }
    public void PlayRandomized3DSound(GameObject go, AudioClip audioClip, float minDistance, float maxDistance)
    {
        AudioSource audio = go.GetOrAddComponent<AudioSource>();
        audio.spatialBlend = 1;
        audio.rolloffMode = AudioRolloffMode.Linear;
        audio.minDistance = minDistance;
        audio.maxDistance = maxDistance;
        audio.pitch = Random.Range(0.8f, 1.2f); // 랜덤 피치
        audio.volume = Random.Range(0.7f, 1.0f); // 랜덤 볼륨
        audio.PlayOneShot(audioClip);
    }





}
