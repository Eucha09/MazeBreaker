using UnityEngine;
using static Define;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField]
    protected string _name;
    [SerializeField]
    protected float _indicatorHeight = 3.0f;
    protected bool _update;

	UI_NameIndicator _indicatorUI;

    public abstract void Interact();
    public virtual void Cancel() { }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Detector")
        {
            if (_indicatorUI != null && _update)
            {
                Managers.Resource.Destroy(_indicatorUI.gameObject);
                _indicatorUI = null;
                _update = false;
            }

            if (_indicatorUI == null)
            {
                UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
                _indicatorUI = ui.ShowNameIndicator(transform, _indicatorHeight, _name);
            }
            _indicatorUI.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Detector")
        {
            _indicatorUI.gameObject.SetActive(false);
        }
    }
}
