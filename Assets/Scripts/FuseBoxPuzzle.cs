using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class FuseBoxPuzzle : MonoBehaviour
{
    [SerializeField] private List<LeverToggle> levers;
    [SerializeField] public Light ceilingLight;
    private bool[] leverStates; // true = ON, false = OFF
    private readonly bool[] correct = { true, false, true, false, true };
    private Hotspot hotspot;
    private string puzzleName = "fusebox";

    void OnEnable()
    {
        for (int i = 0; i < levers.Count; i++)
        {
            int index = i;
            levers[i].OnLeverChanged += HandleValueChanged;
        }
    }


    void Awake()
    {
        leverStates = new bool[levers.Count];
        hotspot = transform.parent.parent.Find("Group2/FuseBox").GetComponentInChildren<Hotspot>(true);
        ceilingLight.gameObject.SetActive(false);
    }

    private void HandleValueChanged(int index)
    {
        leverStates[index] = !leverStates[index];
        AudioManager.Instance.PlayLever();
        CheckSolved();
    }

    private void CheckSolved()
    {
        for (int i = 0; i < levers.Count; i++)
            if (leverStates[i] != correct[i]) return;

        PuzzleManager.Instance.PuzzleSolved(puzzleName);
        ceilingLight.gameObject.SetActive(true);
        hotspot.gameObject.SetActive(false);
        gameObject.SetActive(false);
        AudioManager.Instance.PlayLightOn();
    }

    void OnDisable()
    {
        for (int i = 0; i < levers.Count; i++)
        {
            int index = i;
            levers[i].OnLeverChanged -= HandleValueChanged;
        }
    }
    
}
