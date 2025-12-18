using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Raycaster : MonoBehaviour
{   
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask hotspotLayer;


    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            HotspotAction(pos);
        }
    }

    private void HotspotAction(Vector2 screenPos) 
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 
                            10000, hotspotLayer, QueryTriggerInteraction.Collide))
        {
            var hotspot = hit.collider.GetComponent<Hotspot>();
            if (hotspot != null)
            {
                if (hotspot.tag == "Item_Hotspot") hotspot.Pickup();
                else if (hotspot.tag == "Closeup_Hotspot") hotspot.Closeup();
                else if (hotspot.tag == "Puzzle_Hotspot") hotspot.Puzzle();
                else if (hotspot.tag == "Target_Hotspot") hotspot.UseItem();
                else if (hotspot.tag == "Door_Hotspot") hotspot.SwitchRoom();
                else hotspot.Interact();
                return;
            }

            var book = hit.collider.GetComponent<Book>();
            if (book != null)
            {
                book.OnClicked();
                return;
            }

            var windowSlot = hit.collider.GetComponent<WindowLetterSlot>();
            if (windowSlot != null)
            {
                windowSlot.OnClickSlot();
                return;
            }
        }    
    }
}
