using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private Inventory _inventory;

    public GameObject InventoryPanel;
    bool activeInventory = false;

    public Slot[] slots;
    public Transform slotHolder;

    private void Start()
    {
        _inventory = Inventory.instance;
        slots = slotHolder.GetComponentsInChildren<Slot>();
        _inventory.onSlotCountChange += SlotChange;
        InventoryPanel.SetActive(activeInventory);
    }

     
    private void SlotChange(int val)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < _inventory.slotCnt)
                slots[i].GetComponent<Button>().interactable = true;
            else slots[i].GetComponent<Button>().interactable = false;
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            activeInventory = !activeInventory;
            InventoryPanel.SetActive(activeInventory);
        }
    }

    public void AddSlot()
    {
        _inventory.slotCnt++;
    }
}
