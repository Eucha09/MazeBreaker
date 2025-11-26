using System.Collections;
using UnityEngine;

public class TransparentBlock : MonoBehaviour
{
    [SerializeField]
    string _dialogueText;

    Coroutine _coDialogue;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (_coDialogue == null)
                _coDialogue = StartCoroutine(CoDialogue());
        }
    }

    IEnumerator CoDialogue()
    {
        UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
        ui.ShowDialogue(_dialogueText);

        yield return new WaitForSeconds(8.0f);

        _coDialogue = null;
    }
}
