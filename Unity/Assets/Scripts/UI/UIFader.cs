using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIFader : MonoBehaviour
{
    public bool IsVisible => isVisible;

    private Image[] images = default;
    private TextMeshProUGUI[] textMeshes = default;

    private float currentAlpha = 0f;
    private bool isVisible;

    private Coroutine activeCoroutine = null;

    private void Awake()
    {
        images = GetComponentsInChildren<Image>(true);
        textMeshes = GetComponentsInChildren<TextMeshProUGUI>(true);
    }

    public void Hide()
    {
        currentAlpha = 0f;
        UpdateAlphas();

        isVisible = false;
    }

    public void StopActiveCoroutine()
    {
        if (activeCoroutine == null)
        {
            return;
        }

        StopCoroutine(activeCoroutine);
        activeCoroutine = null;
    }

    public void FadeIn(float speed)
    {
        StopActiveCoroutine();
        activeCoroutine = StartCoroutine(FadeInCoroutine(speed));
    }

    public void FadeOut(float speed)
    {
        StopActiveCoroutine();
        activeCoroutine = StartCoroutine(FadeOutCoroutine(speed));
    }

    private IEnumerator FadeInCoroutine(float speed)
    {
        while (currentAlpha < 1f)
        {
            currentAlpha = Mathf.Min(1f, currentAlpha + speed * Time.deltaTime);
            UpdateAlphas();
            yield return null;
        }

        currentAlpha = 1f;
        UpdateAlphas();

        isVisible = true;
    }

    private IEnumerator FadeOutCoroutine(float speed)
    {
        while (currentAlpha > 0f)
        {
            currentAlpha = Mathf.Max(0f, currentAlpha - speed * Time.deltaTime);
            UpdateAlphas();
            yield return null;
        }

        Hide();
    }

    private void UpdateAlphas()
    {
        foreach (Image image in images)
        {
            if (!image.gameObject.activeInHierarchy && currentAlpha > 0f)
            {
                image.gameObject.SetActive(true);
            }
            else if (currentAlpha == 0f && image.gameObject.activeInHierarchy)
            {
                image.gameObject.SetActive(false);
            }

            Color tmp = image.color;
            tmp.a = currentAlpha;
            image.color = tmp;
        }
        foreach (TextMeshProUGUI textMesh in textMeshes)
        {
            if (!textMesh.gameObject.activeInHierarchy && currentAlpha > 0f)
            {
                textMesh.gameObject.SetActive(true);
            }
            else if (currentAlpha == 0f && textMesh.gameObject.activeInHierarchy)
            {
                textMesh.gameObject.SetActive(false);
            }

            Color tmp = textMesh.color;
            tmp.a = currentAlpha;
            textMesh.color = tmp;
        }
    }
}
