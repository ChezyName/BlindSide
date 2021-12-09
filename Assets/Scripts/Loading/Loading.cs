using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public static Loading current;

    //public Image loadRing;
    public Text mapname;
    //public Text cload;
    public GameObject LoadingObj;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        current = this;
        //SceneManager.LoadSceneAsync((int)SceneIndexs.Menu, LoadSceneMode.Additive);
        LoadingObj.SetActive(false);
        SceneManager.LoadSceneAsync((int)SceneIndexs.Menu);
    }
    public void showLoading(SceneData d)
    {
        LoadingObj.SetActive(true);
        mapname.text = d.MapName;
    }

    public void stopLoading()
    {
        LoadingObj.SetActive(false);
    }

    public void LoadScene(int scene)
    {
        SceneManager.LoadSceneAsync(scene);
        //StartCoroutine(GetSceneLoadProgress(scene));
    }

    public IEnumerator GetSceneLoadProgress(int sne)
    {
        //float totalSceneProgress;

        LoadingObj.SetActive(true);

        SceneData s = getSceneData(sne);

        mapname.text = "Loading " + s.MapName;

        AsyncOperation sdeload = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        AsyncOperation load = SceneManager.LoadSceneAsync(sne, LoadSceneMode.Additive);

        while(!load.isDone)
        {
            Debug.Log(sdeload.isDone + " ? " + load.isDone);
            yield return null;
        }

        Debug.Log("Done Loading");
        LoadingObj.SetActive(false);
    }

    public SceneData getSceneData(int buildIndex)
    {
        SceneData[] objects = Resources.LoadAll<SceneData>("Scenes");
        foreach (SceneData g in objects)
        {
            if (g.BuildIndex == buildIndex)
            {
                return g;
            }
        }
        return null;
    }
}
