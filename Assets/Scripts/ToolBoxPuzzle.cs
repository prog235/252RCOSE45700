using UnityEngine;
using System;
using System.Collections.Generic;

public class ToolBoxPuzzle : MonoBehaviour
{
    private readonly int[] correct = {1, 0, 2, 5};
    [SerializeField] private List<IncNumber> nums;
    [SerializeField] private GameObject[] closed;
    [SerializeField] private GameObject[] opened;
    [SerializeField] private GameObject crowbar;
    private int[] cur;
    private string puzzleName = "toolbox";
    
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
        crowbar.SetActive(false);
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
        crowbar.SetActive(true);
        StateManager.Instance.AddTruthByNum(5);
        AudioManager.Instance.PlayToolboxOpen();
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
