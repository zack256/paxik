﻿using System;
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

    public InputField functionNameInputField;
    public InputField functionInputField;
    public Button createFunctionButton;
    public GameObject functionsParent;

    public Material highlightMaterial;
    private GameObject currentlyHighlightedObject = null;
    private Material currentlyHighlightedObjectMaterial;
    private Color currentlyHighlightedLineColor;

    public Camera cameraObject;

    public EvaluateFunction evalFuncScript;

    public GameObject surfacePlanePrefab;

    //private Dictionary<string, GameObject> pointsDict = new Dictionary<string, GameObject>();
    private Dictionary<GameObject, string> pointsDict = new Dictionary<GameObject, string>();
    private Dictionary<GameObject, string> lineSegmentsDict = new Dictionary<GameObject, string>();
    private Dictionary<GameObject, string> functionsDict = new Dictionary<GameObject, string>();

    private int createLineSegmentMode = 0;
    private GameObject firstLineSegmentSegment;

    void ColorLineSegment (GameObject lineSegment, Color color)
    {
        //lineSegment.GetComponent<LineRenderer>().SetColors(color, color);
        lineSegment.GetComponent<LineRenderer>().startColor = color;
        lineSegment.GetComponent<LineRenderer>().endColor = color;
    }

    void HighlightObject (Transform objT)
    {
        if (((objT == null) && (currentlyHighlightedObject == null)) || (((objT) && (currentlyHighlightedObject)) && (objT == currentlyHighlightedObject.transform)))
        {
            return;
        }
        if (currentlyHighlightedObject)
        {
            if (lineSegmentsDict.ContainsKey(currentlyHighlightedObject))
            {
                //currentlyHighlightedObject.GetComponent<LineRenderer>().SetColors(currentlyHighlightedLineColor, currentlyHighlightedLineColor);
                ColorLineSegment(currentlyHighlightedObject, currentlyHighlightedLineColor);
                currentlyHighlightedLineColor = new Color (0, 0, 0, 0);
            } else
            {
                currentlyHighlightedObject.GetComponent<Renderer>().material = currentlyHighlightedObjectMaterial;
                currentlyHighlightedObjectMaterial = null;
            }
            currentlyHighlightedObject = null;
        }
        if (!objT)
        {
            return;
        }
        GameObject obj = objT.gameObject;
        if ((pointsDict.ContainsKey(obj)) || (lineSegmentsDict.ContainsKey(obj)))
        {
            if (pointsDict.ContainsKey(obj))
            {
                currentlyHighlightedObjectMaterial = obj.GetComponent<Renderer>().material;
                obj.GetComponent<Renderer>().material = highlightMaterial;
            } else
            {
                currentlyHighlightedLineColor = obj.GetComponent<LineRenderer>().startColor;
                ColorLineSegment(obj, highlightMaterial.color);
            }
            currentlyHighlightedObject = obj;
        }
    }

    void CreateLineSegment (GameObject mouseHitObject)
    {
        Vector3[] lineSegmentPositons = new Vector3[] { firstLineSegmentSegment.transform.position, mouseHitObject.transform.position };
        GameObject lineSegmentObject = Instantiate(lineSegmentPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        lineSegmentsDict[lineSegmentObject] = "temporary name";
        lineSegmentObject.GetComponent<LineRenderer>().SetPositions(lineSegmentPositons);
        lineSegmentObject.transform.parent = lineSegmentsParent.transform;

        lineSegmentObject.GetComponent<LineSegmentMesh>().CreateLineSegmentMesh(lineSegmentObject, firstLineSegmentSegment, mouseHitObject, 8);

        createLineSegmentMode = 0;
    }

    void HandleMouse (bool mouseWasClicked)
    {
        Transform mouseHitTransform = GetMousePointerObject();
        HighlightObject(mouseHitTransform);
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
                        CreateLineSegment(mouseHitObject);
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

    bool IsValidPointName (string wantedName)
    {
        return wantedName.Length > 0 && char.IsLetter(wantedName[0]);
    }

    bool IsValidFunctionName (string wantedName)
    {
        return IsValidPointName(wantedName);
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

    bool FunctionNameTaken(string query)
    {
        foreach (var functionName in functionsDict.Values)
        {
            if (query == functionName)
            {
                return true;
            }
        }
        return false;
    }

    bool ParseAndInstantiatePoint ()
    {
        string wantedName = pointNameInputField.text;
        if ((wantedName == "") || (!IsValidPointName(wantedName)) || (PointNameTaken(wantedName)))
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

    bool GraphRequestedSurface ()
    {
        string functionNameQuery = functionNameInputField.text;
        if ((functionNameQuery == "") || (!IsValidFunctionName(functionNameQuery)) || (FunctionNameTaken(functionNameQuery)))
        {
            UnityEngine.Debug.Log("bad function name!!");
            return false;
        }
        string query = functionInputField.text;
        query = query.Trim();

        //float res = evalFuncScript.EvalFunc(query);
        //UnityEngine.Debug.Log(res);
        GameObject oneSurfacePlane;
        GameObject specificFunctionParent = new GameObject();
        specificFunctionParent.name = "Function " + functionNameQuery;
        specificFunctionParent.transform.parent = functionsParent.transform;

        float partialX, partialY;
        int surfaceIterations = 200;
        float xStart = -50;
        float yStart = -50;
        float xEnd = 50;
        float yEnd = 50;
        float xStep = (xEnd - xStart) / surfaceIterations;
        float yStep = (yEnd - yStart) / surfaceIterations;
        float x, y, z;
        int i, j;

        for (i = 0; i < surfaceIterations; i++)
        {
            x = i * xStep;
            for (j = 0; j < surfaceIterations; j++)
            {
                y = j * yStep;
                z = evalFuncScript.EvalFunc(query, x, y);
                //Vector3 planePos = new Vector3(x, y, z);
                Vector3 planePos = new Vector3(x, y, -z);
                //Vector3 planePos = new Vector3(x, z, y);    // yz flip
                partialX = evalFuncScript.PartialOfX(query, x, y);
                partialY = evalFuncScript.PartialOfY(query, x, y);
                oneSurfacePlane = Instantiate(surfacePlanePrefab, planePos, Quaternion.identity);
                //oneSurfacePlane.transform.rotation = Quaternion.AngleAxis(45, new Vector3(0, 0, 1));
                //oneSurfacePlane.transform.rotation = Quaternion.AngleAxis(90, oneSurfacePlane.transform.right);
                float dotX = Vector3.Dot(new Vector3(0, 0, 1), new Vector3(1, 0, partialX));
                float dotY = Vector3.Dot(new Vector3(0, 0, 1), new Vector3(0, 1, partialY));
                float lhsX = dotX / Mathf.Pow(1 + Mathf.Pow(partialX, 2), 0.5f);
                float lhsY = dotY / Mathf.Pow(1 + Mathf.Pow(partialY, 2), 0.5f);
                float angleFromPartialXInDegrees = Mathf.Acos(lhsX);
                float angleFromPartialYInDegrees = Mathf.Acos(lhsY);
                float angleFromPartialX = angleFromPartialXInDegrees * (180 / Mathf.PI);
                float angleFromPartialY = angleFromPartialYInDegrees * (180 / Mathf.PI);
                oneSurfacePlane.transform.rotation = Quaternion.AngleAxis(angleFromPartialX, oneSurfacePlane.transform.up);
                oneSurfacePlane.transform.rotation = Quaternion.AngleAxis(angleFromPartialY, oneSurfacePlane.transform.right);
                oneSurfacePlane.transform.parent = specificFunctionParent.transform;
            }
        }

        return true;
    }

    void TryToCreateFunction ()
    {
        GraphRequestedSurface();
    }

    void Start()
    {
        createPointButton.GetComponent<Button>().onClick.AddListener(TryToInstantiatePoint);
        createLineSegmentButton.GetComponent<Button>().onClick.AddListener(TryToCreateLineSegment);
        createFunctionButton.GetComponent<Button>().onClick.AddListener(TryToCreateFunction);
    }

    void Update()
    {
        HandleMouse(Input.GetMouseButtonUp(0));   
    }
}
