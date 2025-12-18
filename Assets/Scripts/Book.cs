using UnityEngine;

public class Book : MonoBehaviour
{
    private BookPuzzle puzzle;

    private void Start()
    {
        puzzle = GetComponentInParent<BookPuzzle>();
    }

    public void OnClicked()
    {
        if (puzzle == null) return;
        puzzle.OnBookClicked(this.transform);
        AudioManager.Instance.PlayBook();
    }
}
