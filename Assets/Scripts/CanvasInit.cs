using UnityEngine;
using UnityEngine.UI;

public class CanvasInit : MonoBehaviour
{
    [SerializeField] private Image[] closeUps;
    void Start()
    {
        foreach (var i in closeUps) i.gameObject.SetActive(false);
    }
}