using UnityEngine;
using System;
using System.Collections.Generic;

public class DoorPuzzle : MonoBehaviour
{
    private readonly int[] correct = {3, 5, 7};
    [SerializeField] private List<IncNumber> nums;
    [SerializeField] private Hotspot doorHs;
    [SerializeField] private Hotspot closeup;

    private int[] cur;
    private string puzzleName = "door";
    
    void OnEnable()
    {
        for (int i = 0; i < nums.Count; i++)
        {
            int index = i;
            nums[i].OnNumberChanged += HandleNumChanged;
        }
    }


    void Awake()
    {
        cur = new int[nums.Count];
        doorHs.gameObject.SetActive(false);
    }


    private void HandleNumChanged(int index)
    {
        cur[index] += 1;
        if (cur[index] == 10) cur[index] = 0;
        CheckSolved();
    }
    
    private void CheckSolved()
    {
        for (int i = 0; i < nums.Count; i++)
            if (correct[i] != cur[i]) return;

        doorHs.gameObject.SetActive(true);
        PuzzleManager.Instance.PuzzleSolved(puzzleName);
        CameraManager.Instance.BackToRoom();
        closeup.gameObject.SetActive(false);
        AudioManager.Instance.PlayDoorUnlock();
    }

    void OnDisable()
    {
        for (int i = 0; i < nums.Count; i++)
        {
            int index = i;
            nums[i].OnNumberChanged -= HandleNumChanged;
        }
    }
}