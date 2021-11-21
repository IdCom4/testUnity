using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField] private UIItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;

    [SerializeField] private GraphicRaycaster m_Raycaster = null;
    private PointerEventData m_PointerEventData;
    [SerializeField] private EventSystem m_EventSystem = null;

    World world;

    private void Start() {
      world = GameObject.Find("World").GetComponent<World>();

      cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update() {
      if (!world.inUI) return ;

      cursorSlot.transform.position = Input.mousePosition;

      if (Input.GetMouseButtonDown(0)) {
        HandleSlotClick(CheckForSlot());
      }


    }

    private void HandleSlotClick(UIItemSlot clickedSlot) {
      if (clickedSlot == null) return ;
      if (!cursorSlot.hasItem && !clickedSlot.hasItem) return ;

      if (clickedSlot.itemSlot.isCreative) {
        cursorItemSlot.EmptySlot();
        cursorItemSlot.InsertStack(clickedSlot.itemSlot.stack);

        return ;
      }
      if (!cursorSlot.hasItem && clickedSlot.hasItem) {
        cursorItemSlot.stack = clickedSlot.itemSlot.TakeAll();
        cursorSlot.UpdateSlot();
        return ;
      }

      if (cursorSlot.hasItem && !clickedSlot.hasItem) {
        clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
        return ;
      }

      if (cursorSlot.hasItem && clickedSlot.hasItem) {
        if (cursorItemSlot.stack.id == clickedSlot.itemSlot.stack.id) {
          clickedSlot.itemSlot.AddToStack(cursorItemSlot.TakeAll().quantity);
        } else {
          ItemStack _cursorStack = cursorItemSlot.TakeAll();
          ItemStack _clickedStack = clickedSlot.itemSlot.TakeAll();

          clickedSlot.itemSlot.InsertStack(_cursorStack);
          cursorItemSlot.InsertStack(_clickedStack);

          return ;
        }
      }
    }

    private UIItemSlot CheckForSlot() {
      m_PointerEventData = new PointerEventData(m_EventSystem);
      m_PointerEventData.position = Input.mousePosition;

      List<RaycastResult> results = new List<RaycastResult>();

      m_Raycaster.Raycast(m_PointerEventData, results);

      foreach(RaycastResult result in results) {
        if (result.gameObject.tag == "UIItemSlot") {
          return result.gameObject.GetComponent<UIItemSlot>();
        }
      }

      return null;
    }
}
