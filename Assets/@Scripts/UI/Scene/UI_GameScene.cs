using Data;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Define;

public class UI_GameScene : UI_Scene
{
    enum Images
    {
        FadeImage,
        HurtImage,
        SystemMsgBg,
    }

    enum GameObjects
    {
        Indicators,
        Quests, 
        KeyGuides,
        UI_Minimap,
    }

    enum TMPs
    {
        SystemMsgText,
    }

    public UI_InventoryAndCrafting InvenCraftingUI { get; private set; }
    public UI_QuickSlots QuickSlotsUI { get; private set; }
    public UI_PlayerBar PlayerBar { get; private set; }
    //public UI_StaminaBar StaminaBar { get; private set; }
    public UI_HostilityMeters HostilityMetersUI { get; private set; }
	public UI_TargetBar TargetBar { get; private set; }
    public UI_Clock ClockUI { get; private set; }
    public UI_TimeScaleControl TimeScaleControlUI { get; private set; }
    public UI_Building BuildingUI { get; private set; }
	public UI_Crafting CraftingStationUI { get; private set; }
	public UI_Refinement RefinementStationUI { get; private set; }
    public UI_Storage StorageUI { get; private set; }
    public UI_Resting RestingUI { get; private set; }
    public UI_DetailedDescription DetailedDescriptionUI { get; private set; }

	//StatInfo _playerStat;
	//Transform _player;
	//PlayerDefaultState _pds;
	//PlayerHookMoveState _phs;

	//public Transform Target { get; set; }
	//public Stat MonsterStat { get; set; }

	//float shakeAmount = 5f;
	//Transform _compassTarget;

	public float fadeDuration = 1.0f;

    GameObject _keyGuideUI;
    UI_Dialogue _dialogueUI;

    Coroutine _coDialog;
    Coroutine _coSysMsg;
    Coroutine _coHurt;
    Coroutine _coFade;
    
    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
        Bind<TextMeshProUGUI>(typeof(TMPs));

        InvenCraftingUI = GetComponentInChildren<UI_InventoryAndCrafting>(true);
        QuickSlotsUI = GetComponentInChildren<UI_QuickSlots>(true);
        PlayerBar = GetComponentInChildren<UI_PlayerBar>(true);
		//StaminaBar = GetComponentInChildren<UI_StaminaBar>();
		HostilityMetersUI = GetComponentInChildren<UI_HostilityMeters>(true);
		TargetBar = GetComponentInChildren<UI_TargetBar>(true);
        ClockUI = GetComponentInChildren<UI_Clock>(true);
        TimeScaleControlUI = Util.FindChild<UI_TimeScaleControl>(gameObject);
        BuildingUI = Util.FindChild<UI_Building>(gameObject, "UI_Building");
		CraftingStationUI = Util.FindChild<UI_Crafting>(gameObject, "UI_CraftingStation");
        RefinementStationUI = Util.FindChild<UI_Refinement>(gameObject, "UI_RefinementStation");
        StorageUI = Util.FindChild<UI_Storage>(gameObject, "UI_Storage");
        RestingUI = Util.FindChild<UI_Resting>(gameObject, "UI_Resting");
        DetailedDescriptionUI = Util.FindChild<UI_DetailedDescription>(gameObject, "UI_DetailedDescription");

        TimeScaleControlUI.Close();
		InvenCraftingUI.gameObject.SetActive(false);
        BuildingUI.gameObject.SetActive(false);
		CraftingStationUI.gameObject.SetActive(false);
        RefinementStationUI.gameObject.SetActive(false);
        StorageUI.gameObject.SetActive(false);
        RestingUI.gameObject.SetActive(false);
        DetailedDescriptionUI.gameObject.SetActive(false);
		if (Managers.Game.CheckStatus(GameStatus.TimePasses) == false)
			ClockUI.gameObject.SetActive(false);
		if (Managers.Game.CheckStatus(GameStatus.PickUpRadar) == false)
			GetObject((int)GameObjects.UI_Minimap).SetActive(false);
        GetImage((int)Images.SystemMsgBg).gameObject.SetActive(false);

        Managers.Event.GameEvents.GameEventAction += OnGameEvent;

		Image fadeImage = GetImage((int)Images.FadeImage);
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 1.0f;
        fadeImage.color = color;
        StartCoroutine(CoFadeIn());

        //Managers.Event.QuestEvents.StartQuestAction += ShowKeyGuide;
        //Managers.Event.QuestEvents.FinishQuestAction += CloseKeyGuide;

        if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Tutorial)
            ShowKeyGuide(1);
		if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Game)
			ShowKeyGuide(4);
		Managers.Event.ItemEvents.EquipItemAction += (id, equipmentType, equipped) =>
        {
            if (equipmentType == Define.EquipmentType.Weapon)
            {
                ItemData itemData = null;
                Managers.Data.ItemDict.TryGetValue(id, out itemData);
                EquipmentData equipmentData = itemData as EquipmentData;
                if (equipped && equipmentData.weaponType != Define.WeaponType.Hammer)
                    ShowKeyGuide(2);
                else if (!equipped)
                    ShowKeyGuide(4);
                else
                    CloseKeyGuide();
            }
        };

		Managers.Input.ControlsChangedHandler -= OnControlsChanged;
        Managers.Input.ControlsChangedHandler += OnControlsChanged;
	}

    public void OnGameEvent(GameStatus status)
	{
		if (Managers.Game.CheckStatus(GameStatus.TimePasses))
			ClockUI.gameObject.SetActive(true);
		if (Managers.Game.CheckStatus(GameStatus.PickUpRadar))
			GetObject((int)GameObjects.UI_Minimap).SetActive(true);
	}

    public void OnControlsChanged(DeviceMode deviceMode)
    {
        if (deviceMode == DeviceMode.KeyboardMouse)
        {
        }
        else if (deviceMode == DeviceMode.Gamepad)
        {
            if (IsAnyWindowOpen())
                Managers.UI.ShowVirtualCursor();
        }
    }

    void Update()
    {
        PlayerBar.RefreshUI();
		//StaminaBar.RefreshUI();
	}

    public void GetInputQuickSlot(int number)
    {
        QuickSlotsUI.Items[number - 1].UseItem();
    }

    public void AddMinimapIndicator(Transform target, Sprite icon, Color color)
    {
        GameObject minimapUI = Util.FindChild(gameObject, "UI_Minimap");
        UI_MinimapIndicator indicator = Managers.UI.MakeSubItem<UI_MinimapIndicator>(minimapUI.transform);
        indicator.SetIndicator(target, icon, color);
    }

    public UI_NameIndicator ShowNameIndicator(Transform target, float height, string name)
    {
        UI_NameIndicator indicatorUI = Managers.UI.MakeSubItem<UI_NameIndicator>(GetObject((int)GameObjects.Indicators).transform);
        indicatorUI.SetTarget(target, height, name);

        return indicatorUI;
    }

    public void AddQuestGroupUI(QuestGroup questGroup)
    {
        Transform quests = Util.FindChild(gameObject, "Quests").transform;
        UI_QuestGroup questGroupUI = Managers.UI.MakeSubItem<UI_QuestGroup>(quests);
        questGroupUI.SetInfo(questGroup);
    }

    public void ShowHurtOverlay()
    {
        if (_coHurt != null)
        {
            StopCoroutine(_coHurt);
            _coHurt = null;
        }
        _coHurt = StartCoroutine(CoFadeHurtOverlay(0.25f));
    }

    public void ShowKeyGuide(int keyGuideId)
    {
        if (_keyGuideUI != null)
			Managers.Resource.Destroy(_keyGuideUI);
		_keyGuideUI = Managers.UI.MakeSubItem<UI_KeyGuide>(GetObject((int)GameObjects.KeyGuides).transform, "KeyGuide" + keyGuideId).gameObject;
    }

    public void CloseKeyGuide()
	{
		if (_keyGuideUI != null)
			Managers.Resource.Destroy(_keyGuideUI);
        _keyGuideUI = null;
    }

    public void ShowDialogue(string text, int charPerSeconds = 20)
    {
        if (_dialogueUI != null)
            Managers.Resource.Destroy(_dialogueUI.gameObject);

        GameObject Dialogues = Util.FindChild(gameObject, "Dialogues");
        _dialogueUI = Managers.UI.MakeSubItem<UI_Dialogue>(Dialogues.transform);
        _dialogueUI.SetTarget(Managers.Object.GetPlayer().transform, 2.5f);
        _dialogueUI.SetMsg(text, charPerSeconds);
    }

    public void ShowSysMsg(string text, float second)
    {
        if (_coSysMsg != null)
            StopCoroutine(_coSysMsg);

        _coSysMsg = StartCoroutine(CoSysMsg(text, second));
    }

    IEnumerator CoSysMsg(string text, float second)
    {
        float elapsedTime = 0f;
        TextMeshProUGUI textUI = Get<TextMeshProUGUI>((int)TMPs.SystemMsgText);
        Image fadeImage = GetImage((int)Images.SystemMsgBg);

        textUI.text = text;
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        while (elapsedTime < 1.0f)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            textUI.color = color;
            yield return null;
        }
        color.a = 1.0f;
        fadeImage.color = color;
        textUI.color = color;

        yield return new WaitForSeconds(second);

        elapsedTime = 0f;
        while (elapsedTime < 1.0f)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1.0f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            textUI.color = color;
            yield return null;
        }
        fadeImage.gameObject.SetActive(false);

        _coSysMsg = null;
    }

    public void ShowOrCloseInven()
	{
        if (InvenCraftingUI.gameObject.activeSelf)
            CloseInvenUI();
        else
            ShowInvenUI();
	}

    public void ShowInvenUI()
	{
		if (Managers.Game.CheckStatus(GameStatus.Enable_Inventory) == false)
			return;

		Managers.Sound.Play("UISOUND/InvenOpen");
		InvenCraftingUI.gameObject.SetActive(true);
        InvenCraftingUI.SetCraftSystem(Managers.Object.GetPlayer().GetComponent<CraftSystem>());
		InvenCraftingUI.RefreshUI();
        Managers.UI.ShowVirtualCursor();
	}

    public void CloseInvenUI()
	{
		Managers.Sound.Play("UISOUND/InvenClose");
		InvenCraftingUI.gameObject.SetActive(false);
        if (!IsAnyWindowOpen())
			Managers.UI.CloseVirtualCursor();
	}

    public void ShowBuildingUI(BuildingSystem buildingSystem)
    {
        CloseRefinementStationUI();
        CloseRestingUI();
        CloseStorageUI();

        BuildingUI.gameObject.SetActive(true);
        BuildingUI.SetBuildingSystem(buildingSystem);
		Managers.UI.ShowVirtualCursor();
	}

    public void CloseBuildingUI()
    {
        BuildingUI.gameObject.SetActive(false);
		if (!IsAnyWindowOpen())
			Managers.UI.CloseVirtualCursor();
	}

    public void ShowCraftingStationUI(CraftSystem craftSystem, Storage storage)
    {
        CraftingStationUI.gameObject.SetActive(true);
		CraftingStationUI.SetCraftSystem(craftSystem);
        CraftingStationUI.GetComponent<UI_Storage>().SetStorage(storage);
		Managers.UI.ShowVirtualCursor();
	}

	public void CloseCraftingStationUI()
	{
		CraftingStationUI.gameObject.SetActive(false);
		if (!IsAnyWindowOpen())
			Managers.UI.CloseVirtualCursor();
	}

	public void ShowRefinementStationUI(RefinementSystem refinementSystem, Storage storage)
	{
		RefinementStationUI.gameObject.SetActive(true);
		RefinementStationUI.SetRefinementSystem(refinementSystem);
		RefinementStationUI.GetComponent<UI_StorageIO>().SetStorage(storage);
		Managers.UI.ShowVirtualCursor();
	}

	public void CloseRefinementStationUI()
	{
		RefinementStationUI.gameObject.SetActive(false);
		if (!IsAnyWindowOpen())
			Managers.UI.CloseVirtualCursor();
	}

    public void ShowStorageUI(Storage storage)
    {
        StorageUI.gameObject.SetActive(true);
        StorageUI.SetStorage(storage);
		Managers.UI.ShowVirtualCursor();
	}

    public void CloseStorageUI()
    {
        StorageUI.gameObject.SetActive(false);
		if (!IsAnyWindowOpen())
			Managers.UI.CloseVirtualCursor();
	}

    public void ShowRestingUI(RestingSystem restingSystem, Storage storage)
    {
        RestingUI.gameObject.SetActive(true);
        RestingUI.SetRestingSystem(restingSystem);
        RestingUI.GetComponent<UI_Storage>().SetStorage(storage);
		Managers.UI.ShowVirtualCursor();
	}

    public void CloseRestingUI()
    {
        RestingUI.gameObject.SetActive(false);
		if (!IsAnyWindowOpen())
			Managers.UI.CloseVirtualCursor();
	}

	public void ShowDetailedDescriptionUI(int templateId)
	{
		OnCancelButton();
		DetailedDescriptionUI.gameObject.SetActive(true);
        DetailedDescriptionUI.SetItem(templateId);
	}

	public void CloseDetailedDescriptionUI()
	{
		DetailedDescriptionUI.gameObject.SetActive(false);
	}

	public void ShowTimeScaleControlUI()
    {
        TimeScaleControlUI.Show();
	}

    public void CloseTimeScaleControlUI()
    {
        TimeScaleControlUI.Close();
	}

	public void HideUI()
	{
		GetObject((int)GameObjects.Indicators).SetActive(false);
		GetObject((int)GameObjects.KeyGuides).SetActive(false);
		InvenCraftingUI.gameObject.SetActive(false);
        BuildingUI.gameObject.SetActive(false);
		CraftingStationUI.gameObject.SetActive(false);
        RefinementStationUI.gameObject.SetActive(false);
        StorageUI.gameObject.SetActive(false);
        RestingUI.gameObject.SetActive(false);
        DetailedDescriptionUI.gameObject.SetActive(false);
		GetObject((int)GameObjects.Quests).GetComponent<UI_Slide>().HideUI();
        TargetBar.GetComponent<UI_Slide>().HideUI();
        ClockUI.GetComponent<UI_Slide>().HideUI();
		GetObject((int)GameObjects.UI_Minimap).GetComponent<UI_Slide>().HideUI();
		PlayerBar.GetComponent<UI_Slide>().HideUI();
		QuickSlotsUI.GetComponent<UI_Slide>().HideUI();
	}

    public void ShowUI()
	{
		GetObject((int)GameObjects.Indicators).SetActive(true);
		GetObject((int)GameObjects.KeyGuides).SetActive(true);
		GetObject((int)GameObjects.Quests).GetComponent<UI_Slide>().ShowUI();
		TargetBar.GetComponent<UI_Slide>().ShowUI();
		ClockUI.GetComponent<UI_Slide>().ShowUI();
		GetObject((int)GameObjects.UI_Minimap).GetComponent<UI_Slide>().ShowUI();
		PlayerBar.GetComponent<UI_Slide>().ShowUI();
		QuickSlotsUI.GetComponent<UI_Slide>().ShowUI();
	}

    public IEnumerator CoFadeOut()
    {
        float elapsedTime = 0f;
        Image fadeImage = GetImage((int)Images.FadeImage);
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

    public IEnumerator CoFadeIn()
    {
        float elapsedTime = 0f;
        Image fadeImage = GetImage((int)Images.FadeImage);
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 1.0f;
        fadeImage.color = color;

        yield return new WaitForSeconds(0.5f);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1.0f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        fadeImage.gameObject.SetActive(false);
    }

    IEnumerator CoFadeHurtOverlay(float duration)
    {
        float timer = 0;

        Image hurtImage = GetImage((int)Images.HurtImage);
        hurtImage.gameObject.SetActive(true);

        // Fade in
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 0.7f, timer / duration);
            hurtImage.color = new Color(hurtImage.color.r, hurtImage.color.g, hurtImage.color.b, alpha);
            yield return null;
        }

        // Hold the image for a short period
        yield return new WaitForSeconds(0.1f);

        // Fade out
        timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0.7f, 0, timer / duration);
            hurtImage.color = new Color(hurtImage.color.r, hurtImage.color.g, hurtImage.color.b, alpha);
            yield return null;
        }

        hurtImage.gameObject.SetActive(false);
    }

	public override bool IsAnyWindowOpen()
    {
        return InvenCraftingUI.gameObject.activeInHierarchy
            || CraftingStationUI.gameObject.activeInHierarchy
            || BuildingUI.gameObject.activeInHierarchy
            || RefinementStationUI.gameObject.activeInHierarchy
            || StorageUI.gameObject.activeInHierarchy
            || RestingUI.gameObject.activeInHierarchy
            || DetailedDescriptionUI.gameObject.activeInHierarchy;
	}

	public override void OnCancelButton()
	{
		InvenCraftingUI.gameObject.SetActive(false);
		CraftingStationUI.gameObject.SetActive(false);
		RefinementStationUI.gameObject.SetActive(false);
		StorageUI.gameObject.SetActive(false);
		RestingUI.gameObject.SetActive(false);
		DetailedDescriptionUI.gameObject.SetActive(false);
		Managers.UI.CloseVirtualCursor();
	}

	#region Gamepad
	public override void OnPointerDown(PointerEventData.InputButton inputButton)
	{
        if (inputButton == PointerEventData.InputButton.Middle)
        {
            if (TimeScaleControlUI.gameObject.activeInHierarchy)
                TimeScaleControlUI.OnClickButton(new PointerEventData(EventSystem.current) { button = inputButton });
        }
	}
	#endregion
}
