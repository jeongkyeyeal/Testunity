using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipInfoViewContoller : MonoBehaviour
{
    private readonly string[] _languageArrE = {
        "Distance",
        "Last Update",
        "Latitude",
        "Longitude",
        "Destination",
        "ETA",
        "Status"
    };
    private readonly string[] _languageArrK = {
        "거리",
        "마지막 업데이트",
        "위도",
        "경도",
        "목적지",
        "도착 예상시간",
        "상태"
    };

    private readonly string[] _navigationMessageE =
    {
        "under way using engine",
        "at anchor",
        "not under command",
        "restricted manoeuvrability",
        "constrained by her draught",
        "moored",
        "aground",
        "engaged in fishing",
        "under way sailing",
        "Navigational status\nReserved for future\nmodification(Category C)",
        "For sailing status\nReserved for future\nrevision(Category A)",
        "reserved for future use",
        "not defined",
    };
    private readonly string[] _navigationMessageK =
    {
        "엔진 사용 중",
        "닻을 내림",
        "명령이 아님",
        "제한된 기동성",
        "홀수에 의한 제한적",
        "정박",
        "좌초",
        "낚시에 종사",
        "항해 중",
        "네비게이션 상태의\n향후 수정을 위해 예약 됨\n(카테고리C)",
        "항해 상태에 대한\n향후 개정을 위해 예약 됨\n(카테고리A)",
        "향후 사용을 위해 예약 됨",
        "정의되지 않음",
    };

    public MainSceneContoller _mainSceneController;

    public List<Text> _titleTexts;

    public Text _name;
    public Text _callSign;
    public Text _mmsi;
    public Text _distance;
    public Text _cpaTcpa;
    public Text _lastUpdate;
    public Text _latitude;
    public Text _longtitude;
    public Text _sog;
    public Text _cog;
    public Text _rot;
    public Text _destination;
    public Text _eta;
    public Text _status;

    public RectTransform contentViewRect;
    private string _noText = "No data";

    private void Awake()
    {
        //리얼버튼 온오프 여기다 넣기
    }

    private void OnEnable()
    {
        SetTitleMenuChangeLanguageAction();  //메뉴로컬라이징
    }

    public void SetShipData(string key, ShipData data, float distance)
    {
        _name.text = data.shipName.Equals("") ? _noText : data.shipName;

        _callSign.text = data.callSign.Equals("") ? _noText : GetNaviID(data.callSign);

        _mmsi.text = key;

        _distance.text = distance.ToString("F1") + "m";

        _lastUpdate.text = data.regDate;

        _cpaTcpa.text = GetCpaTcpa(data.latitude, data.longitude, data.sog, data.cog);
        
        _lastUpdate.text = data.regDate.Equals("") ? _noText : data.regDate;

        _latitude.text = GetLatitudeRead(data.latitude);
        _longtitude.text = GetLongiRead(data.longitude);

        _sog.text = float.Parse(data.sog).ToString("F1") + "kt";
        _cog.text = data.cog + "°";

        _rot.text = data.rot.Equals("") ? _noText : data.rot + "°";

        _destination.text = data.destination.Equals("") ? _noText : data.destination;

        _eta.text = data.eta.Equals("") ? _noText : GetNaviID(data.eta);    //* 파라메타 없다

        _status.text = data.naviStatus.Equals("") ? _noText : GetNaviID(data.naviStatus);
    }

    #region Animation Callback
    public void OpenAnimationCallback()
    {
        contentViewRect.anchoredPosition = new Vector2(0, 0);
    }

    public void CloseAnimationCallback()
    {
        gameObject.SetActive(false);
    }
    #endregion


    /// <summary>
    /// 메뉴 로컬라이징
    /// </summary>
    private void SetTitleMenuChangeLanguageAction()
    {
        switch ((LanguageKeys)ES3.Load<int>("Language"))
        {
            case LanguageKeys.English:
                _noText = "No Data";
                for (int i = 0; i < _titleTexts.Count; i++) { _titleTexts[i].text = _languageArrE[i]; }
                _titleTexts[1].fontSize = 26;
                break;
            case LanguageKeys.Korean:
                _noText = "미표기";
                for (int i = 0; i < _titleTexts.Count; i++) { _titleTexts[i].text = _languageArrK[i]; }
                _titleTexts[1].fontSize = 23;
                break;
            default:
                break;
        }
    }

    private string GetNaviID(string id)
    {
        string result = _noText;

        int n = int.Parse(id);
        if (n >= 11 && n <= 14)
        {
            n = 11;
        }
        if (n == 15)
        {
            n = 12;
        }

        switch ((LanguageKeys)ES3.Load<int>("Language"))
        {
            case LanguageKeys.English:
                result = _navigationMessageE[n];
                break;
            case LanguageKeys.Korean:
                result = _navigationMessageK[n];
                break;
            default:
                break;
        }

        return result;
    }

    /// <summary>
    /// 위도
    /// </summary>
    /// <param name="lat"></param>
    /// <returns></returns>
    private string GetLatitudeRead(string lat)
    {
        double lat_temp = Double.Parse(lat);
        double temp_binary = lat_temp * 600000;
        int binary = (int)temp_binary;
        string ew;

        binary <<= 5;
        binary >>= 5;
        if (binary >= 0)
        {
            ew = "N";
        }
        else
        {
            ew = "S";
            binary *= -1;
        }

        // 분 초 계산
        double min = binary % 600000;
        min = min / 10000; //2자리만 표현 
        double scond = (min - (int)min) * 60.0;
        scond = Math.Truncate(scond * 10) / 10;

        string resultMsg;
        if ((int)min == 0 || (int)min < 10)
        {
            resultMsg = ((int)lat_temp).ToString() + "º 0" + ((int)min).ToString() + "' " + scond.ToString() + "\"" + ew;
        }
        else
        {
            resultMsg = ((int)lat_temp).ToString() + "º" + ((int)min).ToString() + "' " + scond.ToString() + "\"" + ew;
        }

        return resultMsg;
    }
    /// <summary>
    /// 경도
    /// </summary>
    /// <param name="longi"></param>
    /// <returns></returns>
    private string GetLongiRead(string longi)
    {
        double longi_temp = Double.Parse(longi);
        double temp_binary = longi_temp * 600000;

        int binary = (int)temp_binary;
        string ew;

        binary <<= 4;
        binary >>= 4;
        if (binary >= 0)
        {
            ew = "E";
        }
        else
        {
            ew = "W";
            binary *= -1;
        }

        // 분 초 계산
        double min = binary % 600000;
        min = min / 10000; //2자리만 표현 
        double scond = (min - (int)min) * 60.0;
        scond = Math.Truncate(scond * 10) / 10;

        string resultMsg;
        if ((int)min == 0 || (int)min < 10)
        {
            resultMsg = ((int)longi_temp).ToString() + "º 0" + ((int)min).ToString() + "' " + scond.ToString() + "\"" + ew;
        }
        else
        {
            resultMsg = ((int)longi_temp).ToString() + "º" + ((int)min).ToString() + "' " + scond.ToString() + "\"" + ew;
        }

        return resultMsg;
    }

    /**/
    /// <summary>
    /// cpa/tcpa 산출 string
    /// </summary>
    /// <param name="yLat">상대 위도</param>
    /// <param name="yLon">상대 경도</param>
    /// <param name="ySog">상대 sog</param>
    /// <param name="yCog">상대 cog</param>
    /// <returns></returns>
    public string GetCpaTcpa(string yLat, string yLon, string ySog, string yCog)
    {
        var lat1 = Double.Parse(_mainSceneController._userLocation.latitude);
        var lon1 = Double.Parse(_mainSceneController._userLocation.longitude);
        //TODO: 내배 조회 가능할때 넣자.
        var sog1 = 0;
        var cog1 = 0;

        var lat2 = Double.Parse(yLat);
        var lon2 = Double.Parse(yLon);

        ySog = ySog.Equals("") ? "0" : ySog;
        yCog = yCog.Equals("") ? "0" : yCog;
        var sog2 = Double.Parse(ySog);
        var cog2 = Double.Parse(yCog);


        double[] position1 = { lat1, lon1, 0 };
        double[] velocity1 = GenerateSpeedVector(lat1, sog1, cog1);

        double[] position2 = { lat2, lon2, 0 };
        double[] velocity2 = GenerateSpeedVector(lat2, sog2, cog2);

        double tcpa = CalcCPATime(position1, velocity1, position2, velocity2);
        //Debug.Log(tcpa);
        int hh = ((int)tcpa / 3600);
        string hStr = "";
        if (hh != 0)
        {
            hStr = hh.ToString() + "h";
        }
        string tcpaStr = hStr + ((int)tcpa / 60).ToString() + "m " + tcpa % 3600 + "s";

        double[] cpaPosition1 = GetPositionByVeloAndTime(position1, velocity1, tcpa);
        double[] cpaPosition2 = GetPositionByVeloAndTime(position2, velocity2, tcpa);

        double cpa = GetDistanceSimple(cpaPosition1, cpaPosition2, 100);
        cpa = cpa * 1.852 * 1000;//해리 -> km ->m

        string result = cpa.ToString("F1") + " / " + tcpaStr;

        return result;
    }
    //0위도 1경도
    double GetDistanceSimple(double[] start, double[] end, double accuracy)
    {
        double radius = 6371.8;
        accuracy = Math.Floor(accuracy);

        var distance =
            Math.Round(
                Math.Acos(
                    Math.Sin(
                        ToRad(end[0])
                    ) *
                    Math.Sin(
                        ToRad(start[0])
                    ) +
                    Math.Cos(
                       ToRad(end[0])
                    ) *
                    Math.Cos(
                       ToRad(start[0])
                    ) *
                    Math.Cos(
                        ToRad(start[1]) - ToRad(end[1])
                    )
                ) * radius
            );

        return Math.Floor(Math.Round(distance / accuracy) * accuracy);

    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    double[] GenerateSpeedVector(double latitude, double speed, double course)
    {
        var northSpeed = speed * Math.Cos(course * Math.PI / 180) / 60 / 3600;
        var eastSpeed = speed * Math.Sin(course * Math.PI / 180) / 60 / 3600 * Math.Abs(Math.Sin(latitude * Math.PI / 180));
        double[] result = { northSpeed, eastSpeed, 0 };
        return result;
    }
    double CalcCPATime(double[] position1, double[] velocity1, double[] position2, double[] velocity2)
    {
        var posDiff = MathFuncSub(position2, position1);
        var veloDiff = MathFuncSub(velocity2, velocity1);

        var zaehler = -MathFuncdot(posDiff, veloDiff);
        var nenner = MathFuncLengthSquard(veloDiff);

        return nenner == 0.0 ? -1 : zaehler / nenner;
    }
    double[] GetPositionByVeloAndTime(double[] position, double[] velocity, double dt)
    {
        return MathFuncAdd(position, MathFuncMulScalar(velocity, dt));
    }
    double ToRad(double degree)
    {
        double radian = 0.0;
        radian = (degree * Math.PI) / 180.0;

        return radian;
    }

    //수학 함수
    double[] MathFuncAdd(double[] a, double[] b)
    {
        double[] result = new double[3];
        result[0] = a[0] + b[0];
        result[1] = a[1] + b[1];
        result[1] = a[2] + b[2];

        return result;
    }
    double[] MathFuncSub(double[] a, double[] b)
    {
        double[] result = new double[3];
        result[0] = a[0] - b[0];
        result[1] = a[1] - b[1];
        result[1] = a[2] - b[2];

        return result;
    }
    double[] MathFuncMulScalar(double[] a, double s)
    {
        double[] result = new double[3];
        result[0] = a[0] * s;
        result[1] = a[1] * s;
        result[1] = a[2] * s;

        return result;
    }
    double MathFuncdot(double[] a, double[] b)
    {
        double result = 0;
        result = (a[0] * b[0]) + (a[1] * b[1]) + (a[2] + b[2]);

        return result;
    }
    double MathFuncLengthSquard(double[] a)
    {
        double result = 0;
        result = (a[0] * a[0]) + (a[1] * a[1]) + (a[2] * a[2]);

        return result;
    }
}
