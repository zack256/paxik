using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;   // with this library gotta use UnityEngine.Debug 
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class InputPointCoords : MonoBehaviour
{
    public InputField pointNameInputField;
    public InputField pointCoordsInputField;
    public GameObject pointPrefab;
    public Button createPointButton;
    public GameObject pointsParent;
    public Button createLineSegmentButton;
    public GameObject lineSegmentsParent;
    public GameObject lineSegmentPrefab;
    public Camera cameraObject;

    //private Dictionary<string, GameObject> pointsDict = new Dictionary<string, GameObject>();
    private Dictionary<GameObject, string> pointsDict = new Dictionary<GameObject, string>();
    private int createLineSegmentMode = 0;
    private GameObject firstLineSegmentSegment;

    void HandleMouse (bool mouseWasClicked)
    {
        Transform mouseHitTransform = GetMousePointerObject();
        if (mouseHitTransform)
        {
            GameObject mouseHitObject = mouseHitTransform.gameObject;
            if (pointsDict.ContainsKey(mouseHitObject))
            {
                if (createLineSegmentMode == 1)
                {
                    if (mouseWasClicked)
                    {
                        firstLineSegmentSegment = mouseHitObject;
                        UnityEngine.Debug.Log("Select second point.");
                        createLineSegmentMode++;
                    }
                } else if (createLineSegmentMode == 2)
                {
                    if (mouseWasClicked)
                    {
                        if (mouseHitObject == firstLineSegmentSegment)
                        {
                            UnityEngine.Debug.Log("Cannot have second segment as same point!");
                            return;
                        }
                        Vector3[] lineSegmentPositons = new Vector3[] { firstLineSegmentSegment.transform.position, mouseHitObject.transform.position };
                        GameObject lineSegmentObject = Instantiate(lineSegmentPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        lineSegmentObject.GetComponent<LineRenderer>().SetPositions(lineSegmentPositons);
                        lineSegmentObject.transform.parent = lineSegmentsParent.transform;
                        createLineSegmentMode = 0;
                    }
                }
            }
        } else
        {

        }
    }

    Transform GetMousePointerObject()
    {
        RaycastHit hit;
        Ray ray = cameraObject.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            return objectHit;
        }
        return null;
    }

    bool isValidPointName (string wantedName)
    {
        return wantedName.Length > 0 && char.IsLetter(wantedName[0]);
    }

    bool PointNameTaken (string query)
    {
        foreach (var pointName in pointsDict.Values)
        {
            if (query == pointName)
            {
                return true;
            }
        }
        return false;
    }

    bool ParseAndInstantiatePoint ()
    {
        string wantedName = pointNameInputField.text;
        if ((wantedName == "") || (!isValidPointName(wantedName)) || (PointNameTaken(wantedName)))
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
        //pointsDict[wantedName] = point;
        pointsDict[point] = wantedName;
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

    void TryToCreateLineSegment ()
    {
        if (createLineSegmentMode != 0)
        {
            firstLineSegmentSegment = null;
            createLineSegmentMode = 0;
            return;
        }
        if (pointsDict.Count < 2)
        {
            UnityEngine.Debug.Log("Need 2+ points to make a line segment.");
            return;
        }
        UnityEngine.Debug.Log("Select first point.");
        createLineSegmentMode = 1;
    }

    void Start()
    {
        createPointButton.GetComponent<Button>().onClick.AddListener(TryToInstantiatePoint);
        createLineSegmentButton.GetComponent<Button>().onClick.AddListener(TryToCreateLineSegment);
    }

    void Update()
    {
        HandleMouse(Input.GetMouseButtonUp(0));   
    }
}
