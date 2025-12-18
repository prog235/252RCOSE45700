using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;

public class CrossFader : MonoBehaviour
{
    [Header("Groups (parents with GroupFader)")]
    [SerializeField] private GroupFader groupA;
    [SerializeField] private GroupFader groupB;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 1.0f;

    private bool showingA;

    void Start()
    {
        // Assume Group A visible, Group B hidden at start
        groupA.gameObject.SetActive(true);
        groupB.gameObject.SetActive(false);
        showingA = true;
    }

    public void Fade() {
        if (showingA) {
            // A -> out, B -> in
            groupA.FadeOut(fadeDuration);
            groupB.FadeIn(fadeDuration);
            showingA = false;
        }
        else {
            // B -> out, A -> in (example condition)
            groupB.FadeOut(fadeDuration);
            groupA.FadeIn(fadeDuration);
            showingA = true;
        }
    }
}
