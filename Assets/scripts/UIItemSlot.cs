using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public Text slotQuantity;

    World world;

    private void Awake() {
      world = GameObject.Find("World").GetComponent<World>();
    }

    public bool hasItem {
      get {
        if (itemSlot == null)
          return false;
        else
          return itemSlot.hasItem;
      }
    }

    public void Link(ItemSlot _itemSlot) {
      itemSlot = _itemSlot;
      isLinked = true;
      itemSlot.LinkUISlot(this);
      UpdateSlot();
    }

    public void Unlink() {
      itemSlot.UnlinkUISlot();
      itemSlot = null;
      UpdateSlot();
    }

    public void UpdateSlot() {
      if (itemSlot != null && itemSlot.hasItem) {

        slotIcon.sprite = world.itemSprites[itemSlot.stack.id];
        slotQuantity.text = itemSlot.stack.quantity.ToString();
        slotIcon.enabled = true;
        slotQuantity.enabled = true;

      } else
        Clear();
    }

    public void Clear() {
      slotIcon.sprite = null;
      slotQuantity.text = "";
      slotIcon.enabled = false;
      slotQuantity.enabled = false;
    }

    private void OnDestroy() {
      if (isLinked) {
        itemSlot.UnlinkUISlot();
      }
    }
}
public class ItemSlot {
  public ItemStack stack = null;
  private UIItemSlot uiItemSlot = null;

  public bool isCreative = false;

  public bool hasItem {
    get {
      if (stack != null)
        return true;
      else
        return false;
    }
  }

  public ItemSlot(UIItemSlot _uiItemSlot) {
    uiItemSlot = _uiItemSlot;
    uiItemSlot.Link(this);
  }

  public ItemSlot(UIItemSlot _uiItemSlot, ItemStack _stack) {
    stack = _stack;
    uiItemSlot = _uiItemSlot;
    uiItemSlot.Link(this);
  }

  public void LinkUISlot(UIItemSlot _uiItemSlot) {
    uiItemSlot = _uiItemSlot;
    }

  public void UnlinkUISlot() { uiItemSlot = null; }

  public void EmptySlot() {
    stack = null;
    if (uiItemSlot != null)
      uiItemSlot.UpdateSlot();
  }

  public void AddToStack(int quantity) {
    stack.quantity += quantity;
    uiItemSlot.UpdateSlot();
  }
  public void InsertStack(ItemStack _stack) {
    stack = _stack;
    uiItemSlot.UpdateSlot();
  }

  public int Take(int quantity) {

    if (quantity >= stack.quantity) {
      int takenQuantity = stack.quantity;
      EmptySlot();
      return takenQuantity;
    } else {
      stack.quantity -= quantity;
      uiItemSlot.UpdateSlot();
      return quantity;
    }

  }

  public ItemStack TakeAll() {

    ItemStack handOver = new ItemStack(stack.id, stack.quantity);
    EmptySlot();
    return handOver;

  }
}
