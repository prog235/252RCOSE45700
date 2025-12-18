using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InvenSlot : MonoBehaviour
{
    private TextMeshProUGUI nameText;
    private Image icon;
    private Image outline;
    private ItemSO item;


    private void Awake()
    {   
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClick);
        nameText = GetComponentInChildren<TextMeshProUGUI>();
        icon = transform.Find("Icon").GetComponent<Image>();
        outline = transform.Find("Border").GetComponent<Image>();
        outline.enabled = false;
    }

    public void SetItem(ItemSO newItem)
    {
        item = newItem;
        icon.sprite = item.icon;
        nameText.text = item.itemName;
    }

    public void SetOutline(ItemSO selected)
    {
        outline.enabled = (item == selected);
    }

    public void OnClick()
    {   
        InventoryManager.Instance.SelectItem(item);
    }
}
