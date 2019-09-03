using UnityEngine;
using UnityEngine.EventSystems;

public static class Extension
{
    public static UIEventListener GetUIEventListener(this GameObject parentObject)
    {
        UIEventListener ret = parentObject.GetComponent<UIEventListener>();
        if (ret == null) ret = parentObject.AddComponent<UIEventListener>();
        return ret;
    }
}

public class UIEventListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public delegate void UIEventProxy(GameObject gameObject, Vector2 eventPosition);

    public event UIEventProxy OnClick;
    public event UIEventProxy OnMouseDown;
    public event UIEventProxy OnMouseUp;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClick != null)
            OnClick(this.gameObject, eventData.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (OnMouseDown != null)
            OnMouseDown(this.gameObject, eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (OnMouseUp != null)
            OnMouseUp(this.gameObject, eventData.position);
    }
}