using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
  public Player player;

  public RectTransform selected;
  public UIItemSlot[] slots;

  public int slotIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
      byte index = 1;

      foreach(UIItemSlot s in slots) {
        ItemStack stack = new ItemStack(index, Random.Range(2, 65));
        ItemSlot slot = new ItemSlot(s, stack);
        index++;
      }

    }

    // Update is called once per frame
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0) {
          if (scroll > 0)
            slotIndex--;
          else
            slotIndex++;

          if (slotIndex < 0)
            slotIndex = slots.Length - 1;
          if (slotIndex >= slots.Length)
            slotIndex = 0;

          selected.position = slots[slotIndex].slotIcon.transform.position + Vector3.left * (selected.sizeDelta.x / 2) + Vector3.down * (selected.sizeDelta.y / 2);
        }
        
    }
}
