using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserLocation
{
    public string latitude = "";     //위도
    public string longitude = "";    //경도
    public string altitude = "";     //고도
}

enum SystemState
{
    kNone = 0,
    kSettingView,
    kShipInfoView,
}

public class MainSceneContoller : MonoBehaviour
{
    private SystemState _userState = SystemState.kNone;

    private readonly object _lockObj = new object();
    public GameObject _shipObjPrefab;
    public GameObject _itemObjPrefabA;  //장애물
    public GameObject _itemObjPrefabB;  //부표
    public GameObject _itemObjPrefabC;  //공사장

    public GameObject _itemObjPrefabD;  //좌현
    public GameObject _itemObjPrefabE;  //우현

    public float HttpTime = 4.0f;
    public HttpScript _httpClass;

    public GameObject _loadingView;
    public GameObject _mainMenuBtnView;
    public GameObject _data5GView;
    public GameObject _settingView;
    public GameObject _shipInfoView;
    public Animator _shipInfoAnimator;

    public GameObject _cameraErrorView;

    public Animator _alertAnimator;

    public UserLocation _userLocation = new UserLocation();

    public MapViewController _mapVC;

    /// <summary>
    /// 처음 에러를 띄우기위한 코루틴 변수
    /// </summary>
    IEnumerator _firstCoroutine;

    private Dictionary<string, GameObject> _shipObjs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        //compass 사용
        Input.location.Start();
        Input.compass.enabled = true;

        _settingView.SetActive(false);
        _shipInfoView.SetActive(false);
        _data5GView.SetActive(false);

        _shipObjs.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        _firstCoroutine = FirstTimerCoroutine();
        StartCoroutine(_firstCoroutine);


        StartCoroutine(FirstStartCheckCoroutine());
    }

    RaycastHit hit;
    // Update is called once per frame
    void Update()
    {
       // _errorText.text = "x- " + Camera.main.transform.position.x + ",\n" + "y- " + Camera.main.transform.position.y + ",\n" + "z- " + Camera.main.transform.position.z;

        if (Input.touchCount == 1 && _userState == SystemState.kNone)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    ShipObjectScript sc = hit.transform.gameObject.GetComponent<ShipObjectScript>();
                    OpenShipInfoViewAction(sc._userKey, sc.GetShipData(), sc._distance);
                    //sc._shipSc._userKey
                    //sc._shipSc._shipData
                }
            }
        }
    }

    /// <summary>
    /// 상세뷰 열기 액션
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="value"></param>
    /// <param name="distance"></param>
    public void OpenShipInfoViewAction(string userID, ShipData value, float distance)
    {
        _userState = SystemState.kShipInfoView;

        _mainMenuBtnView.SetActive(false);

        ShipInfoViewContoller sc = _shipInfoView.GetComponent<ShipInfoViewContoller>();
        sc.SetShipData(userID, value, distance);

        _shipInfoView.SetActive(true);
        _shipInfoAnimator.SetTrigger("ShipInfoOpenActiion");
    }

    private void AddShipObject(string key, ShipData value)
    {
        if (!value.messageID.Equals("0")) return;   //**원래라면 여기서 장애물 선별해야함.

        ARLocation.Location newLocation = new ARLocation.Location()
        {
            Latitude = double.Parse(value.latitude),
            Longitude = double.Parse(value.longitude),
            Altitude = 0,
            AltitudeMode = ARLocation.AltitudeMode.GroundRelative
        };

        //new object
        GameObject copyPrefab = Instantiate<GameObject>(_shipObjPrefab);
        ShipObjectScript objSc = copyPrefab.GetComponent<ShipObjectScript>();

        objSc._mapVC = _mapVC;
      
        ARLocation.PlaceAtLocation.PlaceAtOptions opts = new ARLocation.PlaceAtLocation.PlaceAtOptions()
        {
            HideObjectUntilItIsPlaced = true,
            MaxNumberOfLocationUpdates = 1, //** 업데이트 횟 수 관리
            MovementSmoothing = 0,
            UseMovingAverage = true
        };

        objSc._mainSceneController = this;
        objSc._userKey = key;
        objSc.SetShipData(value);

        objSc._placeAtLocationScript = ARLocation.PlaceAtLocation.AddPlaceAtComponentCustom(objSc._placeAtLocationScript, newLocation, opts);

        objSc._placeAtLocationScript.ObjectLocationUpdated.AddListener(ObjectUpdatedEvent); //이벤트 등록 (델리게이트)

        _shipObjs.Add(key, copyPrefab);
    }

    #region ARLocation PlaceAtLocation
    /// <summary>
    /// 배 위치 조정후 콜백 함수
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="location"></param>
    /// <param name="count"></param>
    void ObjectUpdatedEvent(GameObject obj, ARLocation.Location location, int count)
    {
        ARLocation.PlaceAtLocation scriptA = obj.GetComponent<ARLocation.PlaceAtLocation>();
        Destroy(scriptA);
        ShipData shipData = obj.GetComponent<ShipObjectScript>().GetShipData();
        float f = float.Parse(shipData.sog);

        _mapVC.SetShipMap(obj);

        //TODO: 여기서 미니맵 좌표 찍어줘야함.
        if (f > 0)
        {
            Vector2 newPs = SetNewPostion(obj.transform.position.x, obj.transform.position.z, shipData.regDate, shipData.sog, float.Parse(shipData.cog));
            obj.transform.position = new Vector3(newPs.x, obj.transform.position.y, newPs.y);
            obj.GetComponent<ShipObjectScript>().SetFirstMoveCoroutineAction();
        }
        else
        {
            return;
        }
    }

    Vector2 SetNewPostion(float position_x, float position_y, String time, String sog, float cog)
    {
        float distance_x = 0;
        float distance_y = 0;

        float distance = 0;

        //통신 형식(바뀌면 안됨) - "2019-10-29 09:18:58"
        int y = int.Parse(time.Substring(0, 4));
        int m = int.Parse(time.Substring(5, 2));
        int d = int.Parse(time.Substring(8, 2)); //7,8
        int hh = int.Parse(time.Substring(11, 2)); //
        int mm = int.Parse(time.Substring(14, 2));
        int ss = int.Parse(time.Substring(17, 2));

        var timeA = DateTime.Now; //현재시간
        var timeB = new DateTime(y, m, d, hh, mm, ss); //통신에서 받아온 시간
        var resultTime = timeA - timeB;

        if ((float)resultTime.TotalSeconds < 0) return new Vector2(position_x, position_y);

        // 단위 : m/s
        distance = (float.Parse(sog) * 0.514f) * (float)resultTime.TotalSeconds;

        float newCog = cog % 90;
        distance_x = distance * (float)Math.Sin(newCog * Mathf.Deg2Rad);
        distance_y = distance * (float)Math.Cos(newCog * Mathf.Deg2Rad);

        int cogN = (int)cog / 90;
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

        return new Vector2(resultX, resultY);
    }
    #endregion
    //AIS정보
    #region Http Delegate
    public Text _errorText;
    public void HttpAisResultDelegate(bool isError, Dictionary<string, ShipData> datas, string errorMsg)
    {
        if (isError)
        {
            _errorText.text = errorMsg;
            _alertAnimator.SetTrigger("SetAlertAction");
            return;
        }
        _errorText.text = "";

        lock (_lockObj)
        {
            //삭제
            List<string> newRemoveKeys = new List<string>();
            foreach (string userID_a in _shipObjs.Keys)
            {
                foreach (string userID_b in datas.Keys)
                {
                    if (userID_a.Equals(userID_b))
                    {   //있으면 일루 들어와짐. 삭제 예정해주고, 통신시간 틀리면 데이터 정보만 바꿔주는걸로.
                        newRemoveKeys.Add(userID_a);

                        ShipObjectScript shipSC = _shipObjs[userID_a].GetComponent<ShipObjectScript>();
                        if (!shipSC.GetShipData().regDate.Equals(datas[userID_b].regDate))
                        {
                            shipSC.SetChangeMoveCoroutineAction(datas[userID_b]);  //이동 정보 수정해주고.
                        }
                        break;
                    }
                }
            }
            foreach (string str in newRemoveKeys)
            {
                datas.Remove(str);
            }

            //추가
            foreach (string userID in datas.Keys)
            {
                AddShipObject(userID, datas[userID]);
            }
        }

        if (_loadingView.activeSelf)
        {
            _loadingView.SetActive(false);
            _data5GView.SetActive(true);
        }
    }
    //바다정보 생성
    public void HttpItemsResultDelegate(bool isError, List<ItemInfo> datas)
    {
        if (isError)
        {
            _httpClass.SendItemsHttpLocation(_userLocation.latitude, _userLocation.longitude);
            return;
        }

        foreach (ItemInfo itemInfo in datas)
        {
            ARLocation.Location newLocation = new ARLocation.Location()
            {
                Latitude = Double.Parse(itemInfo.LATI),
                Longitude = Double.Parse(itemInfo.LONGI),
                Altitude = 0,
                AltitudeMode = ARLocation.AltitudeMode.GroundRelative
            };
            ARLocation.PlaceAtLocation.PlaceAtOptions opts = new ARLocation.PlaceAtLocation.PlaceAtOptions()
            {
                HideObjectUntilItIsPlaced = true,
                MaxNumberOfLocationUpdates = 1,
                MovementSmoothing = 0,
                UseMovingAverage = true
            };

            GameObject copyPrefab = null;

            string latA = itemInfo.LATI;
            string lonA = itemInfo.LONGI;
            //Debug.Log(itemInfo.DANGER_TYPE);
            if (itemInfo.DANGER_TYPE.Equals("1"))
            {
                copyPrefab = Instantiate<GameObject>(_itemObjPrefabA);
            }
            else if (itemInfo.DANGER_TYPE.Equals("2"))
            {
                copyPrefab = Instantiate<GameObject>(_itemObjPrefabB);
            }
            else if (itemInfo.DANGER_TYPE.Equals("3"))
            {
                copyPrefab = Instantiate<GameObject>(_itemObjPrefabC);
            }
            else if (itemInfo.DANGER_TYPE.Equals("4"))
            {
                copyPrefab = Instantiate<GameObject>(_itemObjPrefabD);
                latA = "35.104148";
                lonA = "129.063706";
            }
            else if (itemInfo.DANGER_TYPE.Equals("5"))
            {
                copyPrefab = Instantiate<GameObject>(_itemObjPrefabE);
                latA = "35.106500";
                lonA = "129.066195";
            }
            else
            {
                continue;
            }

            //좌현 33.3,   길이 1.9km
            //우현 213.3  길이 1.8km
            ItemObjectController objSc = copyPrefab.GetComponent<ItemObjectController>();
            objSc._mainSceneController = this;

            objSc.lat = latA;
            objSc.lon = lonA;

            objSc._placeAtLocationScript = ARLocation.PlaceAtLocation.AddPlaceAtComponentCustom(objSc._placeAtLocationScript, newLocation, opts);

            objSc._placeAtLocationScript.ObjectLocationUpdated.AddListener(ItemObjectUpdatedEvent); //이벤트 등록 (델리게이트)
        }
    }
    /// <summary>
    /// 장애물 위치 조정후 콜백 함수
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="location"></param>
    /// <param name="count"></param>
    void ItemObjectUpdatedEvent(GameObject obj, ARLocation.Location location, int count)
    {
        ARLocation.PlaceAtLocation scriptA = obj.GetComponent<ARLocation.PlaceAtLocation>();
        Destroy(scriptA);

        //ItemObjectController objSc = obj.GetComponent<ItemObjectController>();
        //if (objSc.type.Equals("4") || objSc.type.Equals("5"))
        //{
        //    //    Debug.Log("type-" + objSc.type + ", x-" + obj.transform.position.x + ", z-" + obj.transform.position.z);
        //    obj.transform.rotation = Quaternion.Euler(new Vector3(-5, 33.3f, 0));
        //}
    }
    #endregion

    #region GPS Delegate
    /// <summary>
    /// 최초 위치 정보를 받으면 발생
    /// </summary>
    /// <param name="location"></param>
    public void FirstLocationEvent(ARLocation.Location location)
    {
        _userLocation.latitude = location.Latitude.ToString("F6");
        _userLocation.longitude = location.Longitude.ToString("F6");
        _userLocation.altitude = location.Altitude.ToString("F6");
    }

    /// <summary>
    /// 위치 정보가 바뀌면 발생
    /// </summary>
    /// <param name="location"></param>
    public void UpdatedLocationEvent(ARLocation.Location location)
    {
        _userLocation.latitude = location.Latitude.ToString("F6");
        _userLocation.longitude = location.Longitude.ToString("F6");
        _userLocation.altitude = location.Altitude.ToString("F6");
    }
    #endregion

    #region Start AR_Camera Delegate
    /// <summary>
    /// 처음 추적이 시작되면 발생
    /// </summary>
    public void StartARCameraTrackingEvent()
    {
        _cameraErrorView.SetActive(false);
    }
    /// <summary>
    /// 추적이 유실되면 발생
    /// </summary>
    public void LostARCameraTrackingEvent()
    {
        //알림 띄우고
        _cameraErrorView.SetActive(true);
    }
    /// <summary>
    /// 추적이 다시 시작되면 발생
    /// </summary>
    public void RestoredARCameraTrackingEvent()
    {
        //알림 내리고
        _cameraErrorView.SetActive(false);
    }
    #endregion
    //설정버튼
    #region Button Actions
    public void MenuButtonAction()
    {
        _userState = SystemState.kSettingView;

        _mainMenuBtnView.SetActive(false);
        _settingView.SetActive(true);
    }
    //설정버튼 닫기
    public void EndViewsAction()
    {
        if (_userState == SystemState.kShipInfoView)
        {
            _shipInfoAnimator.SetTrigger("ShipInfoCloseAction");
        }
        if (_userState == SystemState.kSettingView)
        {
            _settingView.SetActive(false);
        }

        _mainMenuBtnView.SetActive(true);

        _userState = SystemState.kNone;
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// 처음 GPS 읽기 실패시 액션을 띄우는 코루틴함수
    /// </summary>
    /// <returns></returns>
    IEnumerator FirstTimerCoroutine()
    {
        yield return new WaitForSeconds(30.0f);

        _alertAnimator.SetTrigger("SetAlertAction");
    }

    /// <summary>
    /// 처음 위치 정보값을 받게 되면 서버 요청을 하게 하는 코루틴함수
    /// </summary>
    /// <returns></returns>
    IEnumerator FirstStartCheckCoroutine()
    {
        yield return new WaitUntil(() => (!_userLocation.latitude.Equals("")));

        StopCoroutine(_firstCoroutine);

        StartCoroutine(HttpTimerCroutine());

        _httpClass.SendItemsHttpLocation(_userLocation.latitude, _userLocation.longitude);  //TEST- 장애물API 따로만들었음 재사용시 제거해야할듯.
    }

    /// <summary>
    /// 설정시간값에 따른 통신 요청 코루틴함수
    /// </summary>
    /// <returns></returns>
    IEnumerator HttpTimerCroutine()
    {
        while (true)
        {
            if (!_userLocation.latitude.Equals(""))
            {
                _httpClass.SendAisHttpLocation(_userLocation.latitude, _userLocation.longitude);
            }
            yield return new WaitForSeconds(HttpTime);
        }
    }
    #endregion

    #region Public Actions
    #endregion

}