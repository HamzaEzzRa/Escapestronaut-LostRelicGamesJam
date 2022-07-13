using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public bool IsCoroutineRunning => isCoroutineRunning;

    [SerializeField] private UIFader[] sequences;

    [SerializeField, Range(0.1f, 5f)] private float fadeInSpeed = 2f, fadeOutSpeed = 1f;

    public static TutorialManager Instance => instance;

    private static TutorialManager instance;

    private int currentSequenceIdx = -1;

    private bool isCoroutineRunning;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void Reset()
    {
        if (currentSequenceIdx >= 0 && currentSequenceIdx < sequences.Length)
        {
            sequences[currentSequenceIdx].StopActiveCoroutine();   
            sequences[currentSequenceIdx].Hide();
        }

        StopAllCoroutines();
        isCoroutineRunning = false;
        currentSequenceIdx = -1;
    }

    public void NextSequence(int order)
    {
        if (currentSequenceIdx >= sequences.Length || order != currentSequenceIdx + 1 || IsCoroutineRunning || (currentSequenceIdx > -1 && !sequences[currentSequenceIdx].IsVisible))
        {
            return;
        }

        StartCoroutine(NextSequenceCoroutine());
    }

    private IEnumerator NextSequenceCoroutine()
    {
        isCoroutineRunning = true;

        if (currentSequenceIdx > -1)
        {
            UIFader currentSequence = sequences[currentSequenceIdx];

            currentSequence.FadeOut(fadeOutSpeed);
            yield return new WaitUntil(() => !currentSequence.IsVisible);
        }
        currentSequenceIdx++;

        if (currentSequenceIdx < sequences.Length)
        {
            sequences[currentSequenceIdx].FadeIn(fadeInSpeed);
        }

        isCoroutineRunning = false;
    }
}
