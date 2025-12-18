using UnityEngine;

public class Controller : MonoBehaviour
{
    public bool closed = false;
    public GameObject[] closedObjs;
    public GameObject[] openedObjs;

    void Start()
    {
        foreach (var obj in openedObjs) obj.SetActive(true);
        foreach (var obj in closedObjs) obj.SetActive(false);
    }
    

    public void Play()
    {
        if (!closed)
        {
            foreach (var obj in openedObjs) obj.SetActive(false);
            foreach (var obj in closedObjs) obj.SetActive(true);
        }
        else
        {
            foreach (var obj in openedObjs) obj.SetActive(true);
            foreach (var obj in closedObjs) obj.SetActive(false);
        }
        closed = !closed;
        AudioManager.Instance.PlayCurtain();
    }
}
