using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;

[DisallowMultipleComponent]
public class Hotspot : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemSO item;
    [SerializeField] private bool pickupOnce = true;
    [SerializeField] private bool destroyOnPick = true;
    [SerializeField] private bool destroyOnUse = true;

    [Header("Closeup Data")]
    [SerializeField] private CinemachineCamera cam;

    [Header("Puzzle Data")]
    [SerializeField] private Image puzzle;

    [Header("Enter Data")]
    [SerializeField] private int roomIdx;

    [Header("Interact Data")]
    public Controller cont;

    [Header("Truth Data")]
    [SerializeField] private string truthKey;
    [SerializeField] private bool IsChecked = false;

    private bool _consumed = false;
    
    // Called by raycaster when clicked/hit
    public void Pickup()
    {
        if (_consumed && pickupOnce) return;

        var ok = ItemManager.Instance != null &&
                 ItemManager.Instance.CanAdd(item);

        if (!ok) return;

        _consumed = true;
        InventoryManager.Instance.AddItem(item);

        if (destroyOnPick)
        {
            transform.parent.gameObject.SetActive(false);
        }
    }

    public void Closeup()
    {
        CameraManager.Instance.LookAt(cam);
        HotspotManager.Instance.OffCloseupHotspot(this);
        if (!string.IsNullOrWhiteSpace(truthKey))
        {
            if (!IsChecked) StateManager.Instance.AddTruthByKey(truthKey);
            IsChecked = true;
        }
    }

    public void Puzzle()
    {
        PuzzleManager.Instance.ShowPuzzle(puzzle);
        if (!string.IsNullOrWhiteSpace(truthKey))
        {
            if (!IsChecked) StateManager.Instance.AddTruthByKey(truthKey);
            IsChecked = true;
        }
    }

    public void UseItem()
    {
        if (InventoryManager.Instance.selected == null) return;

        if (ItemManager.Instance.CanUse(InventoryManager.Instance.selected, item))
            InventoryManager.Instance.UseItem();

        if (destroyOnUse) gameObject.SetActive(false);
    }

    public void SwitchRoom()
    {
        GameManager.Instance.SwitchRoom(roomIdx);
        AudioManager.Instance.PlayDoor();
    }

    public void Interact()
    {
        if (cont != null) cont.Play();
    }
}
