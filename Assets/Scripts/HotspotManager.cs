using UnityEngine;

public class HotspotManager : MonoBehaviour
{
    public static HotspotManager Instance { get; private set; }
    private Hotspot cur;
    private Hotspot[] onlyForZoomIns;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public void OnCloseupHotspot()
    {
        if (onlyForZoomIns != null)
            foreach (var h in onlyForZoomIns)
                h.gameObject.SetActive(false);

        onlyForZoomIns = null;

        cur.gameObject.SetActive(true);
        cur = null;
    }

    public void OffCloseupHotspot(Hotspot hs)
    {
        Transform t = hs.transform.parent.Find("OnlyForZoomIn");
        if (t != null) 
        {
            onlyForZoomIns = t.GetComponentsInChildren<Hotspot>(true);

            foreach (var h in onlyForZoomIns)
                h.gameObject.SetActive(true);
        }

        hs.gameObject.SetActive(false);
        cur = hs;
    }
}
