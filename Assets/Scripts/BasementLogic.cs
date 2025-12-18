using System.Collections.Generic;
using UnityEngine;

public class BasementLogic : MonoBehaviour
{
    private Hotspot[] hotspots;
    private Hotspot fuseboxHotspot;
    [SerializeField] private Hotspot doorHs;

    
    void Start()
    {
        fuseboxHotspot = transform.Find("Group2/FuseBox").GetComponentInChildren<Hotspot>(true);
        hotspots = GetComponentsInChildren<Hotspot>(true);

        foreach (var hs in hotspots)
        {
            if (hs == fuseboxHotspot) hs.gameObject.SetActive(true);
            else hs.gameObject.SetActive(false);
        }

        PuzzleManager.Instance.OnPuzzleSolved += HandleHotspots;
    }


    private void HandleHotspots(string puzzleName)
    {
        if (puzzleName == "fusebox")
        {
            foreach (var hs in hotspots)
                if (hs.transform.parent.gameObject.name != "OnlyForZoomIn" && hs != doorHs)
                    hs.gameObject.SetActive(true);
        }
    }
}
