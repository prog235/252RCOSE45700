using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }
    public event Action<Image> OnPuzzleActivated;
    public event Action<string> OnPuzzleSolved;
    private string cur;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this; 
    }

    public void ShowPuzzle(Image puzzle)
    {
        OnPuzzleActivated?.Invoke(puzzle);
    }

    public void PuzzleSolved(string puzzleName)
    {
        OnPuzzleSolved?.Invoke(puzzleName);
    }
}
