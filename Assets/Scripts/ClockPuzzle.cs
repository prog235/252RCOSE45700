using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ClockPuzzle : MonoBehaviour
{
    readonly private int[] scene2Key = {2, 1, 0, 2, 5, 1, 7, 4, 5};
    readonly private int[] scene3Key = {5, 1, 0, 2, 6, 2, 1, 0, 0};
    [SerializeField] List<IncNumberCustom> nums;
    public int[] cur = {5, 1, 0, 2, 5, 1, 7, 0, 0};
    private bool isLoading = false;
    
    void OnEnable()
    {
        for (int i = 0; i < nums.Count; i++)
        {
            int index = i;
            nums[i].OnNumberChanged += HandleNumChanged;
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < nums.Count; i++)
        {
            int index = i;
            nums[i].OnNumberChanged -= HandleNumChanged;
        }
    }


    private void HandleNumChanged(int index)
    {
        cur[index] += 1;
        if (cur[index] == nums[index].max) cur[index] = 0;
        CheckSolved();
    }
    
    private void CheckSolved()
    {
        if (Matches(scene2Key))
        {
            LoadSceneOnce("1");
            return;
        }

        if (Matches(scene3Key))
        {
            LoadSceneOnce("2");
            return;
        }
    }

    private bool Matches(int[] key)
    {
        if (key.Length != cur.Length) return false;

        for (int i = 0; i < cur.Length; i++)
        {
            if (key[i] != cur[i])
                return false;
        }
        return true;
    }

    private void LoadSceneOnce(string sceneName)
    {
        if (isLoading) return;
        isLoading = true;

        // Single mode (default): unload current scene, load new scene
        TransitionManager.Instance.LoadSceneWithStatic(sceneName);
    }
}
