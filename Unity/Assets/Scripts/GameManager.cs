using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

using System;

public enum SceneMap
{
    MENU_SCENE = 0,
    GAME_SCENE = 1,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => instance;

    [SerializeField] private Image sceneFader;
    [SerializeField] private GameObject progressBar;

    [HideInInspector] public bool SubtitlesOn;

    private static GameManager instance;

    private List<AsyncOperation> sceneAsyncOps = new List<AsyncOperation>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);

        QualitySettings.vSyncCount = 1;
    }

    public void LoadScene(SceneMap sceneMap, Action callback, LoadSceneMode loadSceneMode=LoadSceneMode.Single)
    {
        sceneAsyncOps.Add(SceneManager.LoadSceneAsync((int)sceneMap, loadSceneMode));
        StartCoroutine(SceneProgressCoroutine(sceneMap, callback));
    }

    public void UnloadScene(SceneMap sceneMap)
    {
        sceneAsyncOps.Add(SceneManager.UnloadSceneAsync((int)sceneMap));
    }

    public IEnumerator SceneProgressCoroutine(SceneMap sceneMap, Action callback)
    {
        Color tmp = sceneFader.color;
        tmp.a = 1f;
        sceneFader.color = tmp;
        sceneFader.gameObject.SetActive(true);

        for (int i = 0; i < sceneAsyncOps.Count; i++)
        {
            while (!sceneAsyncOps[i].isDone)
            {
                yield return null;
            }
        }

        callback?.Invoke();

        float progress = 0f;
        while (progress < 1f)
        {
            progress += 0.5f * Time.deltaTime;

            tmp = sceneFader.color;
            tmp.a = 1 - progress;
            sceneFader.color = tmp;

            yield return null;
        }

        tmp = sceneFader.color;
        tmp.a = 0f;
        sceneFader.color = tmp;
        sceneFader.gameObject.SetActive(false);

        if (sceneMap == SceneMap.GAME_SCENE)
        {
            AudioManager.Instance.Play("Background");
            if (callback == null)
            {
                TutorialManager.Instance.NextSequence(0);
            }
        }
    }

    public void AutosaveEffect()
    {
        StartCoroutine(AutosaveEffectCoroutine());
    }

    private IEnumerator AutosaveEffectCoroutine()
    {
        if (progressBar != null)
        {
            progressBar.SetActive(true);
            yield return new WaitForSeconds(UnityEngine.Random.Range(1.45f, 1.9f));
            yield return new WaitUntil(() => SerializationManager.IsAutosaving == false);
            progressBar.SetActive(false);
        }
    }

    public void EndGame(float waitTime=0f)
    {
        StartCoroutine(ToMainMenu(waitTime));
    }

    private IEnumerator ToMainMenu(float waitTime)
    {
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }

        StopAllCoroutines();
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.Reset();
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopCurrentVoice();
            AudioManager.Instance.StopAllCoroutines();
        }
        GameLevel.Reset();

        Color tmp = sceneFader.color;
        tmp.a = 1f;
        sceneFader.color = tmp;
        sceneFader.gameObject.SetActive(true);

        AudioManager.Instance.Stop("Background");
        AudioManager.Instance.Play("Main Menu");

        LoadScene(SceneMap.MENU_SCENE, callback: null);
    }
}
