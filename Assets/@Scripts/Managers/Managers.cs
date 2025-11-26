using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance; // 유일성이 보장된다
    public static Managers Instance { get { Init(); return s_instance; } } // 유일한 매니저를 갖고온다

    #region Contents
    EventManager _event = new EventManager();
    GameManagerEx _game = new GameManagerEx();
    InteractionManager _interact = new InteractionManager();
    InventoryManager _inven = new InventoryManager();
    LayerManager _layer = new LayerManager();
    MapManager _map = new MapManager();
    MinimapManager _minimap = new MinimapManager();
    ObjectManager _object = new ObjectManager();
    QuestManager _quest = new QuestManager();
    StarManager _star = new StarManager();
    TimeManager _time = new TimeManager();
    //VideoManager _video = new VideoManager();

    public static EventManager Event { get { return Instance._event; } }
    public static GameManagerEx Game {  get { return Instance._game; } }
    public static InteractionManager Interact { get { return Instance._interact; } }
    public static InventoryManager Inven { get { return Instance._inven; } }
    public static LayerManager Layer { get { return Instance._layer; } }
    public static MapManager Map { get { return Instance._map; } }
    public static MinimapManager Minimap { get { return Instance._minimap; } }
    public static ObjectManager Object { get { return Instance._object; } }
    public static QuestManager Quest { get { return Instance._quest; } }
    public static StarManager Star { get { return Instance._star; } }
    public static TimeManager Time { get { return Instance._time; } }
    //public static VideoManager Video { get { return Instance._video; } }
    #endregion

    #region Core
    DataManager _data = new DataManager();
    InputManager _input = new InputManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SoundManager _sound = new SoundManager();
    UIManager _ui = new UIManager();

    public static DataManager Data { get { return Instance._data; } }
    public static InputManager Input { get { return Instance._input; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._ui; } }
	#endregion

	void Start()
    {
        Init();
	}

    void Update()
    {
        _input.OnUpdate();
        _event.OnUpdate();
    }

    static void Init()
    {
        if (s_instance == null)
        {
			GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            //s_instance._data.Init();
            s_instance._pool.Init();
            s_instance._sound.Init();

            s_instance._event.Init();
            s_instance._game.Init();
            s_instance._quest.Init();
            s_instance._layer.Init();
            s_instance._time.Init();
            //s_instance._video.Init();
        }		
	}

    public static void Clear()
    {
        Input.Clear();
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();

        Minimap.Clear();
        Event.Clear();
        Game.Clear();
        Object.Clear();
        //Video.Clear();
        Inven.Clear();
        Layer.Clear();
        Star.Clear();
        Quest.Clear();
        Time.Clear();
    }
}
