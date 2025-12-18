using UnityEngine;

public class WindowLetterSlot : MonoBehaviour
{
    [Header("Letter Position")]
    public Transform letterAnchor;

    private ItemSO currentItem;        // 이 슬롯에 꽂힌 아이템
    private GameObject placedInstance; // 창문에 보이는 프리팹

    public void OnClickSlot()
    {
        // 1) 현재 선택된 아이템을 먼저 로컬 변수로 확보
        ItemSO selected = InventoryManager.Instance.selected;

        // 선택된 아이템이 없다면: 나중에 "슬롯에 있는 걸 회수" 용도로 쓸지 말지 결정 가능
        if (selected == null)
        {
            if (currentItem != null)
                InventoryManager.Instance.AddItem(currentItem);
            
            var shadowCreator = GetComponentInParent<WindowFakeShadowCreator>();
            if (shadowCreator != null && placedInstance != null)
            {
                shadowCreator.RemoveShadowFor(placedInstance.transform);
            }

            if (placedInstance != null)
                Destroy(placedInstance);
            
            currentItem = null;
            placedInstance = null;

            return;
        }

        // 2) 슬롯에 기존 아이템이 있다면 인벤토리로 되돌리고 프리팹 제거
        if (currentItem != null)
        {
            // 기존 아이템 인벤토리에 반환
            InventoryManager.Instance.AddItem(currentItem);

            var shadowCreator = GetComponentInParent<WindowFakeShadowCreator>();
            if (shadowCreator != null && placedInstance != null)
            {
                shadowCreator.RemoveShadowFor(placedInstance.transform);
            }

            // 기존 프리팹 삭제
            if (placedInstance != null)
                Destroy(placedInstance);

            currentItem = null;
            placedInstance = null;
        }

        // 3) 새 아이템을 슬롯에 배치
        PlaceItem(selected);
    }

    private void PlaceItem(ItemSO item)
    {
        if (item == null)
            return;

        if (item.windowPrefab == null)
        {
            Debug.LogWarning($"선택된 아이템({item.name})은 windowPrefab이 설정되지 않았습니다.");
            return;
        }

        currentItem = item;

        // 프리팹 생성
        placedInstance = Instantiate(item.windowPrefab, letterAnchor);

        placedInstance.transform.localPosition = new Vector3(0.203f, 0.6783054f, -0.071f);
        placedInstance.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        placedInstance.transform.localScale    = new Vector3(0.3390284f, 7.676628f, 2.016962f);
        
        var shadowCreator = GetComponentInParent<WindowFakeShadowCreator>();
        if (shadowCreator != null)
        {
            shadowCreator.SpawnShadowFor(placedInstance.transform);
        }
        // 인벤토리에서 이 아이템 1개 사용 처리
        InventoryManager.Instance.UseItem();
        AudioManager.Instance.PlayVelcro();
    }
}
