using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;   // with this library gotta use UnityEngine.Debug 
using UnityEngine;
using UnityEngine.UI;

public class InputPointCoords : MonoBehaviour
{
    public InputField pointNameInputField;
    public InputField pointCoordsInputField;
    public GameObject pointPrefab;
    public Button createPointButton;
    public GameObject pointsParent;
    //public Dictionary<string, GameObject> pointsDict;

    private Dictionary<string, GameObject> pointsDict = new Dictionary<string, GameObject>();
    
    bool isValidPointName (string wantedName)
    {
        return wantedName.Length > 0 && char.IsLetter(wantedName[0]);
    }

    bool ParseAndInstantiatePoint ()
    {
        string wantedName = pointNameInputField.text;
        if ((wantedName == "") || (pointsDict.ContainsKey(wantedName)) || (!isValidPointName(wantedName)))
        {
            UnityEngine.Debug.Log("bad point name!!");
            return false;
        }
        string query = pointCoordsInputField.text;
        query = query.Trim();
        if (query.Substring(0, 1) != "(")
        {
            return false;
        } else if (query.Substring(query.Length - 1) != ")")
        {
            return false;
        }
        int firstCommaIdx = query.IndexOf(",");
        if (firstCommaIdx == -1)
        {
            return false;
        }
        int secondCommaIdx = query.IndexOf(",", firstCommaIdx + 1);
        if (secondCommaIdx == -1)
        {
            return false;
        }
        string firstNumberString = query.Substring(1, firstCommaIdx - 1).Trim();
        string secondNumberString = query.Substring(firstCommaIdx + 1, secondCommaIdx - firstCommaIdx - 1).Trim();
        string thirdNumberString = query.Substring(secondCommaIdx + 1, query.Length - secondCommaIdx - 2).Trim();
        bool firstGood = double.TryParse(firstNumberString, out double xCoord);
        bool secondGood = double.TryParse(secondNumberString, out double yCoord);
        bool thirdGood = double.TryParse(thirdNumberString, out double zCoord);
        //UnityEngine.Debug.Log(firstGood + " " + secondGood + " " + thirdGood + " " + firstNumberString + " " + secondNumberString + " " + thirdNumberString);
        if (!(firstGood & secondGood & thirdGood))
        {
            return false;
        }
        Vector3 pointCoords = new Vector3((float) xCoord, (float) yCoord, (float) zCoord);
        GameObject point = Instantiate(pointPrefab, pointCoords, Quaternion.identity);
        pointsDict[wantedName] = point;
        point.name = "Point " + wantedName;
        point.transform.parent = pointsParent.transform;
        pointNameInputField.text = "";
        pointCoordsInputField.text = "";
        return true;
    }

    void TryToInstantiatePoint ()
    {
        bool res = ParseAndInstantiatePoint();
        if (res)
        {
            UnityEngine.Debug.Log("Successfully instantiated the point!");
        } else
        {
            UnityEngine.Debug.Log("Failed to instantiate point, bad query :(");
        }
    }

    void Start()
    {
        Button btn = createPointButton.GetComponent<Button>();
        btn.onClick.AddListener(TryToInstantiatePoint);
    }

    void Update()
    {
        
    }
}
