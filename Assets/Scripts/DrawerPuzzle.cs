using UnityEngine;
using System;
using System.Collections.Generic;

public class DrawerPuzzle : MonoBehaviour
{
    private readonly int[] correct = {9, 5, 0, 2};
    [SerializeField] private List<IncNumber> nums;
    [SerializeField] private GameObject[] closed;
    [SerializeField] private GameObject[] opened;
    private int[] cur;
    private string puzzleName = "drawer";
    
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
        foreach (var obj in closed) obj.SetActive(true);
        foreach (var obj in opened) obj.SetActive(false);
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

        PuzzleManager.Instance.PuzzleSolved(puzzleName);
        foreach (var obj in closed) obj.SetActive(false);
        foreach (var obj in opened) obj.SetActive(true);
        AudioManager.Instance.PlayDoorUnlock();
        AudioManager.Instance.PlayDrawer();
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
