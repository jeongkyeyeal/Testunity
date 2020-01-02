using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LongButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Animator _ani;
    private bool isBtnDown = false;
    private bool isAction = false;

    private void Update()
    {
        if (isBtnDown)
        {
            AnimationAction();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Test");
        isBtnDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isBtnDown = false;
        isAction = false;
        _ani.SetTrigger("IsOff");
    }

    private void AnimationAction()
    {
        if (isAction) return;

        isAction = true;

        _ani.SetTrigger("IsOn");
    }
}
