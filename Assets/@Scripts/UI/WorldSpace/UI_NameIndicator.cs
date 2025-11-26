using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UI_NameIndicator : UI_Base
{
    [SerializeField]
    Text _nameText;

    Transform _target;
    float _height;
    string _name;
    //public void SetName(string name) { _name = name; }
    public void SetTarget(Transform target, float height, string name) 
    {
        _target = target;
        _height = height;
        _name = name; 
    }

    public override void Init()
    {
        _nameText.text = _name;
    }

    void Update()
    {
        if (_target == null)
        {
            Managers.Resource.Destroy(gameObject);
            return;
        }
        transform.position = Camera.main.WorldToScreenPoint(_target.position + new Vector3(0.0f, _height, 0.0f));
        //transform.rotation = Camera.main.transform.rotation;
    }
}
