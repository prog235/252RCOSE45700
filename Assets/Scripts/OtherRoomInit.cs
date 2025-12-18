using System.Collections.Generic;
using UnityEngine;

public class OtherRoomInit : MonoBehaviour
{
    private Hotspot[] hotspots;
    
    void Start()
    {
        hotspots = GetComponentsInChildren<Hotspot>(true);
        
        foreach (var hs in hotspots)
            if (hs.transform.parent.gameObject.name == "OnlyForZoomIn")
                hs.gameObject.SetActive(false);
    }
}

