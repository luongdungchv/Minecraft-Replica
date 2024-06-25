using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Test2 : MonoBehaviour, IDragHandler, IDropHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("drag");
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("drop");
    }
}
