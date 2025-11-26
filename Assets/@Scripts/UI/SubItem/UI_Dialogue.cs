using UnityEngine;
using UnityEngine.UI;

public class UI_Dialogue : UI_Base
{
    [SerializeField]
    Text _dialogueText;

    Transform _target;
    float _height;
    string _name;

    string _targetMsg;
    int _index;
    int _charPerSeconds = 20;

    public void SetTarget(Transform target, float height)
    {
        _target = target;
        _height = height;
    }

    public void SetMsg(string msg, int charPerSeconds)
    {
        _targetMsg = msg;
        _charPerSeconds = charPerSeconds;
        EffectStart();
    }

    void EffectStart()
    {
        _dialogueText.text = "";
        _index = 0;
        Invoke("Effecting", 1.0f / _charPerSeconds);
    }

    void Effecting()
    {
        if (_dialogueText.text == _targetMsg)
        {
            EffectEnd();
            return;
        }

        _dialogueText.text += _targetMsg[_index];
        _index++;

        Invoke("Effecting", 1.0f / _charPerSeconds);
    }

    void EffectEnd()
    {
        Destroy(gameObject, 3.0f);
    }

    public override void Init()
    {

    }

    void Update()
    {
        if (_target == null)
        {
            Managers.Resource.Destroy(gameObject);
            return;
        }
        transform.position = Camera.main.WorldToScreenPoint(_target.position + new Vector3(0.0f, _height, 0.0f));
    }
}
