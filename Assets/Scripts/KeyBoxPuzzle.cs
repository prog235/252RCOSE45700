using UnityEngine;
using System;
using System.Collections.Generic;

public class KeyBoxPuzzle : MonoBehaviour
{
    private readonly int[] correct = {0, 3, 3, 6, 8};
    [SerializeField] private IncAlphabet alp;
    [SerializeField] private List<IncNumber> nums;
    [SerializeField] private GameObject[] closed;
    [SerializeField] private GameObject[] opened;
    [SerializeField] private GameObject key;
    private int[] cur;
    private int alpNum;
    private string puzzleName = "keybox";
    
    void OnEnable()
    {    
        for (int i = 0; i < nums.Count; i++)
        {
            int index = i;
            nums[i].OnNumberChanged += HandleNumChanged;
        }

        alp.OnAlpChanged += HandleAlpChanged;
    }


    void Awake()
    {
        cur = new int[nums.Count];

        key.SetActive(false);
        foreach (var obj in closed) obj.SetActive(true);
        foreach (var obj in opened) obj.SetActive(false);
    }


    private void HandleNumChanged(int index)
    {
        cur[index] += 1;
        if (cur[index] == 10) cur[index] = 0;
        CheckSolved();
    }

    private void HandleAlpChanged()
    {
        alpNum += 1;
        if (alpNum == 5) alpNum = 0;
        CheckSolved();
    }
    
    private void CheckSolved()
    {
        if (alpNum != correct[0]) return;

        for (int i = 0; i < nums.Count; i++)
            if (correct[i + 1] != cur[i]) return;
        
        PuzzleManager.Instance.PuzzleSolved(puzzleName);
        foreach (var obj in closed) obj.SetActive(false);
        foreach (var obj in opened) obj.SetActive(true);
        key.SetActive(true);
        AudioManager.Instance.PlayDoorUnlock();
        AudioManager.Instance.PlayBox();
    }

    void OnDisable()
    {
        for (int i = 0; i < nums.Count; i++)
        {
            int index = i;
            nums[i].OnNumberChanged -= HandleNumChanged;
        }

        alp.OnAlpChanged -= HandleAlpChanged;
    }
}
