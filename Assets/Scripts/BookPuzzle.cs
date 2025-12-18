using System.Collections;
using UnityEngine;

public class BookPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class BookEntry
    {
        public Transform book;                // 책 Transform

        [HideInInspector] public Vector3 baseLocalPos; // 기본 자리(local)
        [HideInInspector] public bool isRaised;        // 위로 떠 있는지
        [HideInInspector] public Coroutine moveRoutine;
    }

    [Header("Books")]
    [SerializeField] private BookEntry[] entries;

    [Header("Animation Settings")]
    [SerializeField] private float raiseOffset = 0.15f;   // 선택 시 위로 뜨는 높이
    [SerializeField] private float moveDuration = 0.15f;  // 이동 애니메이션 시간

    private BookEntry selectedEntry;
    private bool interactionLocked = false;   // 스왑 중일 때 추가 입력 막기

    private void Awake()
    {
        // 시작 시 각 책의 현재 localPosition을 "자리"로 기억해둠
        foreach (var e in entries)
        {
            if (e == null || e.book == null) continue;
            e.baseLocalPos = e.book.localPosition;
            e.isRaised = false;
        }
    }

    /// <summary>
    /// 책이 클릭되었을 때 호출하면 됨.
    /// 예: Book.cs 의 OnMouseDown / Hotspot에서 puzzle.OnBookClicked(transform);
    /// </summary>
    public void OnBookClicked(Transform clickedBook)
    {
        if (interactionLocked || clickedBook == null)
            return;

        BookEntry clickedEntry = FindEntry(clickedBook);
        if (clickedEntry == null)
            return;

        // 아직 선택된 책이 없을 때 → 이 책 선택 & 위로 띄우기
        if (selectedEntry == null)
        {
            SelectEntry(clickedEntry);
            return;
        }

        // 같은 책을 다시 클릭 → 내려놓고 선택 해제
        if (selectedEntry == clickedEntry)
        {
            DeselectEntry(clickedEntry);
            selectedEntry = null;
            return;
        }

        // 다른 책 클릭 → 두 책 자리 스왑
        StartCoroutine(SwapRoutine(selectedEntry, clickedEntry));
        selectedEntry = null;
    }

    private BookEntry FindEntry(Transform bookTransform)
    {
        foreach (var e in entries)
        {
            if (e != null && e.book == bookTransform)
                return e;
        }
        return null;
    }

    private void SelectEntry(BookEntry entry)
    {
        selectedEntry = entry;
        MoveBook(entry, entry.baseLocalPos + Vector3.up * raiseOffset, true);
    }

    private void DeselectEntry(BookEntry entry)
    {
        MoveBook(entry, entry.baseLocalPos, false);
    }

    private void MoveBook(BookEntry entry, Vector3 targetLocalPos, bool raiseFlag)
    {
        if (entry.book == null) return;

        if (entry.moveRoutine != null)
            StopCoroutine(entry.moveRoutine);

        entry.moveRoutine = StartCoroutine(MoveBookRoutine(entry, targetLocalPos, raiseFlag));
    }

    private IEnumerator MoveBookRoutine(BookEntry entry, Vector3 targetLocalPos, bool raiseFlag)
    {
        Vector3 start = entry.book.localPosition;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            entry.book.localPosition = Vector3.Lerp(start, targetLocalPos, t);
            yield return null;
        }

        entry.book.localPosition = targetLocalPos;
        entry.isRaised = raiseFlag;
        entry.moveRoutine = null;
    }

    private IEnumerator SwapRoutine(BookEntry a, BookEntry b)
    {
        interactionLocked = true;

        // 떠 있는 책이 있으면 먼저 자기 자리로 내려놓기
        if (a.isRaised)
            yield return MoveBookRoutine(a, a.baseLocalPos, false);
        if (b.isRaised)
            yield return MoveBookRoutine(b, b.baseLocalPos, false);

        // 각 책의 "자리" 정보를 스왑
        Vector3 tempBase = a.baseLocalPos;
        a.baseLocalPos = b.baseLocalPos;
        b.baseLocalPos = tempBase;

        // 현재 위치에서 새 자리까지 동시에 이동
        Coroutine ca = StartCoroutine(MoveBookRoutine(a, a.baseLocalPos, false));
        Coroutine cb = StartCoroutine(MoveBookRoutine(b, b.baseLocalPos, false));

        // 둘 다 끝날 때까지 대기 (moveDuration으로 충분)
        yield return new WaitForSeconds(moveDuration);

        a.isRaised = false;
        b.isRaised = false;
        interactionLocked = false;
    }
}
