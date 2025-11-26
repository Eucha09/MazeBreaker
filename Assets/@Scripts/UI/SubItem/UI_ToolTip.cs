using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ToolTip : UI_Base
{
    [SerializeField]
	TextMeshProUGUI _text;

	public override void Init()
	{

	}

	public void SetToolTip(string text)
    {
        _text.text = text;
    }
}
