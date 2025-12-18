using System.Collections.Generic;
using UnityEngine;

public class LivingRoomLogic : MonoBehaviour
{
    private Hotspot[] hotspots;
    [SerializeField] private Hotspot[] doorHs;
    
    void Start()
    {
        hotspots = GetComponentsInChildren<Hotspot>(true);
        
        foreach (var hs in hotspots)
            if (hs.transform.parent.gameObject.name == "OnlyForZoomIn")
                hs.gameObject.SetActive(false);
        
        foreach (var dh in doorHs)
            dh.gameObject.SetActive(false);
    }
}

