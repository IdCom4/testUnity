using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    public byte id;
    public int quantity;

    public ItemStack(byte _itemID, int _quantity) {
      id = _itemID;
      quantity = _quantity;
    }
}
