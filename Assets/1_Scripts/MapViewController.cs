using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapViewController : MonoBehaviour
{
    private bool _isMapOpen = false;

    public RectTransform _mapRectTransform;

    public Animator _viewAnimator;

    private int _sr = 0;

    public RectTransform _item0;
    public RectTransform _item1;
    public RectTransform _item2;
    public RectTransform _item3;

    public Image buttonImage;
    public Sprite buttonImageA;
    public Sprite buttonImageB;

    void Update()
    {
        if (!_isMapOpen) return;

        int r = 0;
        if (ES3.Load<bool>("Test"))
        {
            r = (int)Camera.main.transform.rotation.eulerAngles.y;
        }
        else
        {
            r = (int)Input.compass.trueHeading;
        }

        if (r >= _sr + 18 || r <= _sr - 18)
        {
            if (r % 5 == 0)
            {
                _sr = r;
                _mapRectTransform.rotation = Quaternion.Euler(0f, 0f, _sr);
                _item0.localEulerAngles = new Vector3(0, 0, -_sr);
                _item1.localEulerAngles = new Vector3(0, 0, -_sr);
                _item2.localEulerAngles = new Vector3(0, 0, -_sr);
                _item3.localEulerAngles = new Vector3(0, 0, -_sr);
            }
        }
    }

    public GameObject _map;
    public GameObject _shipPrefab;
    public Dictionary<string, GameObject> _shipMapList = new Dictionary<string, GameObject>();
     
    public void SetShipMap(GameObject obj)
    {
        ShipObjectScript sc = obj.GetComponent<ShipObjectScript>();
        //Vector2 tempPs = new Vector2(obj.transform.position.x, obj.transform.position.z);
        //Vector2 newShipPs = GetObjectPosition(tempPs, obj.transform.rotation.y, sc._distance);
        //Vector2 newPs = MapPosition(newShipPs.x, newShipPs.y);
        Vector2 newPs = MapPosition(obj.transform.position.x, obj.transform.position.z);

        GameObject copyPrefab = Instantiate<GameObject>(_shipPrefab);

        copyPrefab.transform.parent = _map.transform;

        copyPrefab.GetComponent<RectTransform>().localPosition = new Vector2(newPs.x, newPs.y);
        //copyPrefab.transform.SetParent(_map.transform, true);

        _shipMapList.Add(sc._userKey, copyPrefab);
    }
    public void RemoveShipMap(string userID)
    {
        bool check = false;
        foreach (string key in _shipMapList.Keys)
        {
            if (key.Equals(userID))
            {
                check = true;
                break;
            }
        }
        if (!check) return;

        GameObject shipMap = _shipMapList[userID];
        
        _shipMapList.Remove(userID);

        Destroy(shipMap);
    }
    public void MoveShipMap(string userID, GameObject obj, float angle, float dis)
    {
        foreach (string key in _shipMapList.Keys)
        {
            if (key.Equals(userID))
            {   //여기서 이동 처리 하고 가즈아.
                GameObject objMap = _shipMapList[key];
                //Vector2 tempPs = new Vector2(obj.transform.position.x, obj.transform.position.z);
                //Vector2 newShipPs = GetObjectPosition(tempPs, angle, dis);
                //Vector2 newPs = MapPosition(newShipPs.x, newShipPs.y);
                Vector2 newPs = MapPosition(obj.transform.position.x, obj.transform.position.z);
                objMap.GetComponent<RectTransform>().localPosition = new Vector2(newPs.x, newPs.y);
                break;
            }
        }
    }

    Vector2 GetObjectPosition(Vector2 objPs, float angle, float dis)
    {
        //float angle 물체가 카메라를 보는 각도
        //float dis 카메라와 물체사이거리
        float caldistance_z = 0;
        float caldistance_x = 0;

        Quaternion temp = Quaternion.Euler(0, 0, 0);

        if (objPs.x > 0 && objPs.y > 0)
        {
            temp = Quaternion.Euler(0, 270 - angle, 0);
            caldistance_z = dis * Mathf.Sin((temp.eulerAngles.y) * Mathf.Deg2Rad);
            caldistance_x = dis * Mathf.Cos((temp.eulerAngles.y) * Mathf.Deg2Rad);

        }
        else if (objPs.x < 0 && objPs.y > 0)
        {
            temp = Quaternion.Euler(0, angle - 90, 0);
            caldistance_z = dis * Mathf.Sin((temp.eulerAngles.y) * Mathf.Deg2Rad);
            caldistance_x = -dis * Mathf.Cos((temp.eulerAngles.y) * Mathf.Deg2Rad);
        }
        else if (objPs.x > 0 && objPs.y < 0)
        {
            temp = Quaternion.Euler(0, angle - 270, 0);
            caldistance_z = -dis * Mathf.Sin((temp.eulerAngles.y) * Mathf.Deg2Rad);
            caldistance_x = dis * Mathf.Cos((temp.eulerAngles.y) * Mathf.Deg2Rad);
        }
        else if (objPs.x < 0 && objPs .y < 0)
        {
            temp = Quaternion.Euler(0, 90 - angle, 0);
            caldistance_z = -dis * Mathf.Sin((temp.eulerAngles.y) * Mathf.Deg2Rad);
            caldistance_x = -dis * Mathf.Cos((temp.eulerAngles.y) * Mathf.Deg2Rad);
        }

        return new Vector2(caldistance_x, caldistance_z);
    }


    Vector2 MapPosition(float worldPostion_x, float worldPostion_z)
    {
        //Rect Transform의 map x,y
        float mapPosition_x = worldPostion_x * 0.28f;
        float mapPosition_y = worldPostion_z * 0.28f;

        Vector2 position = new Vector2(mapPosition_x, mapPosition_y);

        return position;
    }

    #region Button Actions
    public void OceanMapButtonAction()
    {//TODO: 전자해도 키는 버튼
        if (!_isMapOpen)
        {   //open
            if (ES3.Load<bool>("Test"))
            {
                _sr = (int)Camera.main.transform.rotation.eulerAngles.y;
            }
            else
            {
                _sr = (int)Input.compass.trueHeading;
            }
            _mapRectTransform.rotation = Quaternion.Euler(0f, 0f, _sr);

            _item0.localEulerAngles = new Vector3(0, 0, -_sr);
            _item1.localEulerAngles = new Vector3(0, 0, -_sr);
            _item2.localEulerAngles = new Vector3(0, 0, -_sr);
            _item3.localEulerAngles = new Vector3(0, 0, -_sr);

            _viewAnimator.SetTrigger("IsOn");
            buttonImage.sprite = buttonImageB;
        }
        else
        {   //off
            _viewAnimator.SetTrigger("IsOff");
            buttonImage.sprite = buttonImageA;
        }

        _isMapOpen = !_isMapOpen;
    }
    #endregion

}
