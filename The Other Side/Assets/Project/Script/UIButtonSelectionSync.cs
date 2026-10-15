using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSelectionSync : MonoBehaviour,
    IPointerEnterHandler,
    IPointerMoveHandler,
    IPointerExitHandler
{
    public static bool IsPointerOverButton => hoveredButton != null;

    private static UIButtonSelectionSync hoveredButton;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectThisButton();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        SelectThisButton();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoveredButton == this)
            hoveredButton = null;
    }

    private void OnDisable()
    {
        if (hoveredButton == this)
            hoveredButton = null;
    }

    private void SelectThisButton()
    {
        if (button == null || !button.IsActive() || !button.IsInteractable())
            return;

        hoveredButton = this;

        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != gameObject)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
