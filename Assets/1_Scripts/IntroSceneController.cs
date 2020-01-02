using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Flags]
public enum AlarmCheackKeys
{
    On = 0,
    Off = 1
}

[Flags]
public enum UnitCheackKeys
{
    Kn = 0,
    Km = 1
}

[Flags]
public enum LanguageKeys
{
    English = 0,
    Korean = 1
}

public class IntroSceneController : MonoBehaviour
{
    public float logoTime;

    private void Awake()
    {
        ApplicationChrome.statusBarColor = ApplicationChrome.navigationBarColor = 0xff000000;
    }
    void Start()
    {
        //TEST
        ES3.DeleteFile();

        if (!ES3.FileExists())
        {
            ES3.Save<bool>("Test", true);   //좌표 나침반 고정
            ES3.Save<int>("Alarm", AlarmCheackKeys.Off);  
            ES3.Save<int>("Unit", UnitCheackKeys.Kn);
            ES3.Save<int>("Language", LanguageKeys.Korean);
        }

        StartCoroutine(SceneChangeA());
    }

    IEnumerator SceneChangeA()
    {
        yield return new WaitForSeconds(logoTime);

        SceneChangeB();
    }

    void SceneChangeB()
    {
        SceneManager.LoadScene("1_MainScene");
    }
}
