using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using CI.HttpClient;

public class HttpScript : MonoBehaviour
{
    private const bool _isTest = false;//Test코드를 사용할 것인지 안 할것인지 설정

    //public delegate void GetInAppPurchaseItem(bool isNonConsumable);
    //public static event GetInAppPurchaseItem SendInAppPurchaseItem;

    public MainSceneContoller _mainSceneContoller;

    static readonly string _mainAisUrl = "https://mariners.or.kr/vrar/ais_json.php";
    static readonly string _mainItemsUrl = "https://mariners.or.kr/vrar/danger_json.php";

    private void Awake()
    {
        //유효성 검사 강제로 열기..
        //참고: https://github.com/ClaytonIndustries/HttpClient/wiki/HTTPS
        System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) =>
        {
            return true;
        };
    }

    /// <summary>
    /// AIS 통신 요청
    /// </summary>
    /// <param name="latitude">위도</param>
    /// <param name="longitude">경도</param>
    public void SendAisHttpLocation(string latitude, string longitude)
    {
        HttpClient client = new HttpClient();

        Dictionary<string, string> pr = new Dictionary<string, string>()
        {
            { "mode", "ais" },
            //{"lati", "35.008862" },
            //{ "longi", "128.982101" }
         { "lati", latitude },
          { "longi", longitude }
        };

        FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(pr);

        client.Post(
            new System.Uri(_mainAisUrl),
            formUrlEncodedContent,
            HttpCompletionOption.AllResponseContent,
            (response) =>
            {
                //HttpResponseMessage
                if (response != null)
                {
                    // Debug.Log(response.StatusCode);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK) //통신 코드
                    {
                        //Test
                        if (_isTest)
                        {
                            _mainSceneContoller.HttpAisResultDelegate(false, ShipDiction(), "");
                            return;
                        }

                        try
                        {
                            AisData aisData = JsonMapper.ToObject<AisData>(response.ReadAsString());

                            if (aisData.code == 200)
                            {
                                //   List<ShipInfo> datas
                                Dictionary<string, ShipData> rowDatas = new Dictionary<string, ShipData>();

                                foreach (ShipInfo shipInfo in aisData.rows)
                                {
                                    ShipData shipData = new ShipData()
                                    {
                                        shipName = shipInfo.SHIP_NAME,
                                        destination = shipInfo.DESTINATION,
                                        messageID = shipInfo.MESSIGE_ID,
                                        naviStatus = shipInfo.NAVI_STATUS,
                                        longitude = shipInfo.LONGI,
                                        latitude = shipInfo.LATI,
                                        altitude = shipInfo.ALTITUDE,
                                        heading = shipInfo.HEADING,
                                        rot = shipInfo.ROT,
                                        rotSpeed = shipInfo.ROT_SPEED,
                                        sog = shipInfo.SOG,
                                        cog = shipInfo.COG,
                                        regDate = shipInfo.REG_DATE,
                                        distance = shipInfo.DISTANCE,
                                        callSign = shipInfo.CALL_SIGN,
                                        eta = shipInfo.ETA
                                    };

                                    rowDatas.Add(shipInfo.USER_ID, shipData);
                                }
                                
                                _mainSceneContoller.HttpAisResultDelegate(false, rowDatas, "");
                                //   Debug.Log("1- 통신 파싱 성공");
                            }
                            else
                            {
                                // Debug.Log("2- 통신 파싱 내용 없음");
                                _mainSceneContoller.HttpAisResultDelegate(true, null, "파싱데이터없음");
                            }
                        }
                        catch
                        {
                            //  Debug.Log("3- 통신 파싱 실패");
                            _mainSceneContoller.HttpAisResultDelegate(true, null, "파싱실패");
                        }

                    }
                }
                else
                {
                    //Debug.Log("4- 통신 실패");
                    _mainSceneContoller.HttpAisResultDelegate(true, null, "통신실패");
                }

            }
            );
    }

    /// <summary>
    /// 부표장애물 통신요청
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    public void SendItemsHttpLocation(string latitude, string longitude)
    {
        HttpClient client = new HttpClient();
        Dictionary<string, string> pr = new Dictionary<string, string>()
        {
            { "mode", "area" },
            { "lati", latitude },
            { "longi", longitude }
        };

        FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(pr);

        client.Post(
            new System.Uri(_mainItemsUrl),
            formUrlEncodedContent,
            HttpCompletionOption.AllResponseContent,
            (response) =>
            {
                //HttpResponseMessage
                if (response != null)
                {
                    // Debug.Log(response.StatusCode);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK) //통신 코드
                    {
                        if (_isTest)
                        {
                            string[] la = { "35.088797", "35.089555", "35.088837", "35.088951" };//{ "35.088806", "35.088681", "35.088633", "35.088514", "35.088635", "35.089366" };
                            string[] lo = { "129.039259", "129.03832", "129.039368", "129.039926" };//{ "129.038666", "129.039251", "129.039117", "129.039184", "129.038819", "129.038457" };
                            List<ItemInfo> testDatas = new List<ItemInfo>();
                            ItemInfo a = new ItemInfo()
                            {
                                DANGER_TYPE = "1",   //타입 1- 장애물, 2- 부표, 3- 공사장, 4- 좌현, 5- 우현
                                LATI = la[0],
                                LONGI = lo[0]
                            };
                            ItemInfo b = new ItemInfo()
                            {
                                DANGER_TYPE = "1",   //타입 1- 장애물, 2- 부표, 3- 공사장, 4- 좌현, 5- 우현
                                LATI = la[1],
                                LONGI = lo[1]
                            };
                            ItemInfo c = new ItemInfo()
                            {
                                DANGER_TYPE = "2",   //타입 1- 장애물, 2- 부표, 3- 공사장, 4- 좌현, 5- 우현
                                LATI = la[2],
                                LONGI = lo[2]
                            };
                            ItemInfo d = new ItemInfo()
                            {
                                DANGER_TYPE = "3",   //타입 1- 장애물, 2- 부표, 3- 공사장, 4- 좌현, 5- 우현
                                LATI = la[3],
                                LONGI = lo[3]
                            };
                            testDatas.Add(a);
                            testDatas.Add(b);
                            testDatas.Add(c);
                            testDatas.Add(d);
                            _mainSceneContoller.HttpItemsResultDelegate(false, testDatas);
                            return;
                        }

                        try
                        {
                            ItemDatas data = JsonMapper.ToObject<ItemDatas>(response.ReadAsString());

                            if (data.code == 200)
                            {
                                //   List<ShipInfo> datas
                                List<ItemInfo> rowDatas = data.rows;

                                _mainSceneContoller.HttpItemsResultDelegate(false, rowDatas);

                                //   Debug.Log("1- 통신 파싱 성공");
                            }
                            else
                            {
                                // Debug.Log("2- 통신 파싱 내용 없음");
                                _mainSceneContoller.HttpItemsResultDelegate(true, null);
                            }
                        }
                        catch
                        {
                            //  Debug.Log("3- 통신 파싱 실패");
                            _mainSceneContoller.HttpItemsResultDelegate(true, null);
                        }

                    }
                }
                else
                {
                    //Debug.Log("4- 통신 실패");
                    _mainSceneContoller.HttpItemsResultDelegate(true, null);
                }

            }
            );
    }

    //TEST
    Dictionary<string, ShipData> ShipDiction()
    {
        Dictionary<string, ShipData> diction = new Dictionary<string, ShipData>();

        //public double distance; //나와의 거리
        string[] la = { "35.088797", "35.089555", "35.088837", "35.088951" };//{ "35.088806", "35.088681", "35.088633", "35.088514", "35.088635", "35.089366" };
        string[] lo = { "129.039259", "129.03832", "129.039368", "129.039926" };//{ "129.038666", "129.039251", "129.039117", "129.039184", "129.038819", "129.038457" };

        //35.088405, 129.040336 중학교옥상
        //35.089555, 129.038327 사거리
        for (int i = 0; i < 4; i++)
        {
            ShipData dd = new ShipData()
            {
                shipName = "a00" + i,
                destination = "",
                messageID = "0",
                naviStatus = "1",
                longitude = lo[i],
                latitude = la[i],
                altitude = "75",
                heading = "",
                rot = "0",
                rotSpeed = "s",
                sog = "0",
                cog = "0",
                regDate = "2019-10-16 15:41:12",
                distance = "1.1234",
                callSign = "",
                eta = ""
            };
            diction.Add(i.ToString(), dd);
        }

        return diction;
    }

}


#region Json Data

[Serializable]
public class AisData
{
    public int code;
    public string msg;
    public List<ShipInfo> rows = new List<ShipInfo>();
}

[Serializable]
public class ShipInfo
{
    public string USER_ID;   //배고유넘버
    public string SHIP_NAME; //배이름
    public string DESTINATION;  //목적지(항구명)
    public string MESSIGE_ID;    //0-배 1-부표 null이면 무시할 것.
    public string NAVI_STATUS;   //네비 상태 1~15 정의 될 것.
    public string LONGI;    //경도
    public string LATI; //위도
    public string HEADING;  //운전대 각도
    public string ALTITUDE; //고도
    public string ROT;  //배의 턴 각도
    public string ROT_SPEED; //배의 턴 진행 속도
    public string SOG;  //현재 속도
    public string COG;  //현재 배 방향
    public string REG_DATE; //배통신 시간
    public string DISTANCE; //나와의 거리
    public string CALL_SIGN;    //배부를때 이름
    public string ETA;  //목적지 도착 시간
}

[Serializable]
public class ShipData
{
    public string shipName; //배이름
    public string destination;  //목적지(항구명)
    public string messageID;    //0-배 1-부표 null이면 무시할 것.
    public string naviStatus;   //네비 상태 1~15 정의 될 것.
    public string longitude;    //경도
    public string latitude; //위도
    public string altitude; //고도
    public string heading;  //운전대 각도
    public string rot;  //배의 턴 각도
    public string rotSpeed; //배의 턴 진행 속도
    public string sog;  //현재 속도
    public string cog;  //현재 배 방향
    public string regDate; //배통신 시간
    public string distance; //나와의 거리
    public string callSign; //배부르는 이름
    public string eta;  //목적지 도착시간
}

[Serializable]
public class ItemDatas
{
    public int code;
    public string msg;
    public List<ItemInfo> rows = new List<ItemInfo>();
}

[Serializable]
public class ItemInfo
{
    public string DANGER_TYPE;   //타입 1- 장애물, 2- 부표, 3- 공사장, 4- 좌현, 5- 우현
    public string LATI; //위도
    public string LONGI;  //경도
}

#endregion