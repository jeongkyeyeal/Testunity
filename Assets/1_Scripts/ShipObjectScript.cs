using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipObjectScript : MonoBehaviour
{
    public MainSceneContoller _mainSceneController;

    public ARLocation.PlaceAtLocation _placeAtLocationScript;

    public TextMeshPro _shipNameLabel;
    public TextMeshPro _shipSpeedLabel;
    public TextMeshPro _shipDirectionLabel;
    public TextMeshPro _shipDistanceLabel;

    public string _userKey;
    private ShipData _shipData;

    public float _reLocationTime = 1.5f;

    private IEnumerator _moveLocationCoroutine;   //무브시키는 로케이션좌표 실행하는 코루틴(매초)

    public MapViewController _mapVC = null;

    public float _distance
    {
        get
        {
            return _dis;
        }
    }
    private float _dis { set; get; }

    public void Start()
    {
        _moveLocationCoroutine = ReLocationCoroutine();
    }

    private void OnDestroy()
    {
        StopCoroutine(_moveLocationCoroutine);
        if (_mapVC != null)
        {
            _mapVC.RemoveShipMap(_userKey);
        }
    }

    public void SetShipData(ShipData data)
    {
        _shipData = data;
        //8자리까지 짜르자.
        string name = _shipData.shipName;
        if (name.Length > 8)
        {
            name = name.Substring(0, 8);
        }
        if (name.Equals(""))
        {
            name = "No Data";
        }
        _shipNameLabel.text = name;
        _shipSpeedLabel.text = "SOG: " + float.Parse(_shipData.sog).ToString("F1") + "kt";
        _shipDirectionLabel.text = "COG: " + float.Parse(_shipData.cog).ToString("F0") + "°";
    }
    public ShipData GetShipData()
    {
        return _shipData;
    }


    private void LateUpdate()
    {
        _dis = Vector3.Distance(Camera.main.transform.position, gameObject.transform.position);

        Vector3 newRotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position).eulerAngles;
        newRotation.x = 0;
        newRotation.z = 0;
        transform.rotation = Quaternion.Euler(newRotation);

        if (_dis >= 1500)
        {
            transform.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            int t = (int)((_dis / 20.0f) + 1.0f);   //20m단위
            transform.localScale = new Vector3(t, t, 1);
        }

        _shipDistanceLabel.text = "Dist.: " + _dis.ToString("F1") + "m";
    }

    private float _sogKm = 0f;
    private float _cogData = 0f;
    /// <summary>
    /// 이동 코루틴 실행 함수
    /// </summary>
    public void SetFirstMoveCoroutineAction()
    {
        _sogKm = float.Parse(_shipData.sog) * 0.514f;
        _cogData = float.Parse(_shipData.cog);

        StartCoroutine(_moveLocationCoroutine);
    }
    public void SetChangeMoveCoroutineAction(ShipData data)
    {
        StopCoroutine(_moveLocationCoroutine);

        _shipData = data;
        _sogKm = float.Parse(_shipData.sog) * 0.514f;
        _cogData = float.Parse(_shipData.cog);

        StartCoroutine(_moveLocationCoroutine);
    }

    IEnumerator ReLocationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_reLocationTime);

            SetMovePostion();

            _mapVC.MoveShipMap(_userKey, gameObject, gameObject.transform.rotation.y, _distance);
        }
    }
  
    private void SetMovePostion()
    {
        float position_x = gameObject.transform.position.x;
        float position_y = gameObject.transform.position.z;

        float distance_x = 0;
        float distance_y = 0;

        // 단위 : m/s
        float distance = _sogKm * _reLocationTime;

        float newCog = _cogData % 90;
        distance_x = distance * (float)Math.Sin(newCog * Mathf.Deg2Rad);
        distance_y = distance * (float)Math.Cos(newCog * Mathf.Deg2Rad);

        int cogN = (int)_cogData / 90;
        float resultX = 0f;
        float resultY = 0f;
  
        switch (cogN)
        {
            case 0:
                resultX = position_x + distance_x;
                resultY = position_y + distance_y;
                break;
            case 1:
                resultX = position_x + distance_y;
                resultY = position_y - distance_x;
                break;
            case 2:
                resultX = position_x - distance_x;
                resultY = position_y - distance_y;
                break;
            case 3:
                resultX = position_x - distance_y;
                resultY = position_y + distance_x;
                break;
            default:
                resultX = position_x;
                resultY = position_y;
                break;
        }

        gameObject.transform.position = new Vector3(resultX, gameObject.transform.position.y, resultY);
    }
}
