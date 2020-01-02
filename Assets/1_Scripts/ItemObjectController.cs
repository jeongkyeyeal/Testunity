using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObjectController : MonoBehaviour
{
    public MainSceneContoller _mainSceneController;

    public ARLocation.PlaceAtLocation _placeAtLocationScript;

    public string type; //1- 장애물, 2- 부표, 3- 공사장, 4- 좌현, 5- 우현
    public string lat;
    public string lon;

    public float _distance
    {
        get{
            return _dis;
        }
    }
    private float _dis { set; get; }

    private void LateUpdate()
    {
        _dis = Vector3.Distance(Camera.main.transform.position, gameObject.transform.position);

        if (_dis >= 1500)
        {
            transform.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            int t = (int)((_dis / 20.0f) + 1.0f);   //20m단위
            transform.localScale = new Vector3(t, t, 1);
        }

        if (type.Equals("4") || type.Equals("5")) return;

        Vector3 newRotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position).eulerAngles;
        newRotation.x = 0;
        newRotation.z = 0;
        transform.rotation = Quaternion.Euler(newRotation);
    }
}
