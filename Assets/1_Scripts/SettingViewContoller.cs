using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingViewContoller : MonoBehaviour
{

    public GameObject _alarmOnBtn;
    public GameObject _alarmOffBtn;

    public Button _knBtn;
    public Button _kmBtn;

    public Button _koBtn;
    public Button _enBtn;

    public Text _titleText;
    public Text _alramText;
    public Text _alramDisText;
    public Text _unitText;
    public Text _languageText;

    public Slider _alramSlider;

    private AlarmCheackKeys _alarmKey;
    private UnitCheackKeys _unitKey;
    private LanguageKeys _languageKey;

    void Start()
    {
        _alarmKey = (AlarmCheackKeys)ES3.Load<int>("Alarm");
        _unitKey = (UnitCheackKeys)ES3.Load<int>("Unit");
        _languageKey = (LanguageKeys)ES3.Load<int>("Language");

        SetAction();

        SetLanguageAction();
    }

    private void SetAction()
    {
        if (_languageKey == LanguageKeys.English)
        {
            _titleText.text = "SETTING ";
            _alramText.text = "Crash alarm";
            _alramDisText.text = "Alarm distance\n" + (int)(300.0f * (_alramSlider.value / 1.0f * 100.0f) / 100.0f) + " m";
            _unitText.text = "Unit";
            _languageText.text = "Language";

            _koBtn.interactable = true;
            _enBtn.interactable = false;
        }
        else if (_languageKey == LanguageKeys.Korean)
        {
            _titleText.text = "설정 ";
            _alramText.text = "충돌 알람";
            _alramDisText.text = "알람 거리\n" + (int)(300.0f * (_alramSlider.value / 1.0f * 100.0f) / 100.0f) + " m";
            _unitText.text = "단위";
            _languageText.text = "언어";

            _koBtn.interactable = false;
            _enBtn.interactable = true;
        }

        if (_alarmKey == AlarmCheackKeys.On)
        {
            _alarmOnBtn.SetActive(true);
            _alarmOffBtn.SetActive(false);
            _alramSlider.enabled = true;
        }
        else
        {
            _alarmOnBtn.SetActive(false);
            _alarmOffBtn.SetActive(true);
            _alramSlider.enabled = false;
        }
        //kn = 0

        if (_unitKey == UnitCheackKeys.Kn)
        {
            _knBtn.interactable = false;
            _kmBtn.interactable = true;
        }
        else
        {
            _knBtn.interactable = true;
            _kmBtn.interactable = false;
        }
    }

    private void SetLanguageAction()
    {
        if (_languageKey == LanguageKeys.English)
        {
            _titleText.text = "SETTING ";
            _alramText.text = "Crash alarm";
            _alramDisText.text = "Alarm distance\n" + (int)(300.0f * (_alramSlider.value / 1.0f * 100.0f) / 100.0f) + " m";
            _unitText.text = "Unit";
            _languageText.text = "Language";
        }
        else if (_languageKey == LanguageKeys.Korean)
        {
            _titleText.text = "설정 ";
            _alramText.text = "충돌 알람";
            _alramDisText.text = "알람 거리\n" + (int)(300.0f * (_alramSlider.value / 1.0f * 100.0f) / 100.0f) + " m";
            _unitText.text = "단위";
            _languageText.text = "언어";
        }
    }


    #region SettingView Button Actions
    public void AlramButtonAction(int btnTag)
    {   //btnTag = 0- On | 1- Off
        if (btnTag == 0)
        {
            _alarmKey = AlarmCheackKeys.Off;

            _alarmOnBtn.SetActive(false);
            _alarmOffBtn.SetActive(true);

            _alramSlider.enabled = false;
        }
        else
        {
            _alarmKey = AlarmCheackKeys.On;

            _alarmOnBtn.SetActive(true);
            _alarmOffBtn.SetActive(false);

            _alramSlider.enabled = true;
        }

        ES3.Save<int>("Alarm", _alarmKey);
    }

    public void UnitButtonAction(int btnTag)
    {   //UnitCheackKeys 키값 == 태그값
        if (btnTag == 0)
        {   //kn
            _unitKey = UnitCheackKeys.Kn;

            _knBtn.interactable = false;
            _kmBtn.interactable = true;
        }
        else
        {   //km
            _unitKey = UnitCheackKeys.Km;

            _knBtn.interactable = true;
            _kmBtn.interactable = false;
        }

        ES3.Save<int>("Unit", _unitKey);
    }

    public void LanguageButtonAction(int btnTag)
    {   //0-ko, 1-en
        if (btnTag == 0)
        {
            _languageKey = LanguageKeys.Korean;

            _koBtn.interactable = false;
            _enBtn.interactable = true;
        }
        else
        {
            _languageKey = LanguageKeys.English;

            _koBtn.interactable = true;
            _enBtn.interactable = false;
        }

        ES3.Save<int>("Language", _languageKey);

        SetLanguageAction();
    }
    #endregion

    public void SliderChangedAction()
    {
        if (_languageKey == LanguageKeys.English)
        {
            _alramDisText.text = "Alarm distance\n" + (int)(300.0f * (_alramSlider.value / 1.0f * 100.0f) / 100.0f) + " m";
        }
        else if (_languageKey == LanguageKeys.Korean)
        {
            _alramDisText.text = "알람 거리\n" + (int)(300.0f * (_alramSlider.value / 1.0f * 100.0f) / 100.0f) + " m";
        }
    }
}
