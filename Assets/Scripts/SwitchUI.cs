using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchUI : MonoBehaviour
{
    private Button back;
    private Button rotate;
    private Image chatBar;
    private Image cur;
    private bool isZoomin;

    private void Start()
    {
        isZoomin = false;
        back = transform.Find("Back").GetComponent<Button>();
        rotate = transform.Find("Rotate").GetComponent<Button>();
        chatBar = transform.Find("ChatBar").GetComponent<Image>();

        PuzzleManager.Instance.OnPuzzleActivated += OnPuzzleMode;
        PuzzleManager.Instance.OnPuzzleSolved += SolvedAndClose;
        CameraManager.Instance.ZoomIn += HandleZoomIn;
        CameraManager.Instance.ZoomOut += HandleZoomOut;

        back.onClick.AddListener(Back);
        back.gameObject.SetActive(false);    
    }

    // Update is called once per frame
    private void OnDisable()
    {
        PuzzleManager.Instance.OnPuzzleActivated -= OnPuzzleMode;
        PuzzleManager.Instance.OnPuzzleSolved -= SolvedAndClose;
        CameraManager.Instance.ZoomIn -= HandleZoomIn;
        CameraManager.Instance.ZoomOut -= HandleZoomOut;
    }

    private void HandleZoomIn()
    {
        back.gameObject.SetActive(true);
        rotate.gameObject.SetActive(false);
        chatBar.gameObject.SetActive(false);
        isZoomin = true;
    }

    private void HandleZoomOut()
    {
        Back();
    }

    private void OnPuzzleMode(Image puzzle)
    {
        cur = puzzle;
        puzzle.gameObject.SetActive(true);
        back.gameObject.SetActive(true);
        rotate.gameObject.SetActive(false);
        chatBar.gameObject.SetActive(false);
    }

    private void Back()
    {
        // already Zoom Out
        if (!isZoomin && cur == null) return;

        // When ZoomIn & PuzzleMode 
        if (cur != null && cur.gameObject.activeSelf && isZoomin)
            cur.gameObject.SetActive(false);

        // When ZoomIn & Not PuzzleMode
        else if (isZoomin)
        {
            back.gameObject.SetActive(false);
            rotate.gameObject.SetActive(true);
            chatBar.gameObject.SetActive(true);
            isZoomin = false;
            CameraManager.Instance.BackToRoom();
            HotspotManager.Instance.OnCloseupHotspot();
        }
        
        // When Only PuzzleMode
        else
        {
            cur.gameObject.SetActive(false);
            back.gameObject.SetActive(false);
            rotate.gameObject.SetActive(true);
            chatBar.gameObject.SetActive(true);
        }
    }

    private void SolvedAndClose(string puzzleName)
    {
        Back();
    }
}
