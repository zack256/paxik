using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvaluateFunction : MonoBehaviour
{
    public float EvalFunc (string func, float x, float y)
    {
        return x + y;
    }

    public float PartialOfX (string func, float x, float y)
    {
        float deltaX = 0.0001f * x;
        float deltaZ = EvalFunc(func, x + deltaX, y) - EvalFunc(func, x, y);
        return deltaZ / deltaX;
    }

    public float PartialOfY(string func, float x, float y)
    {
        float deltaY = 0.0001f * y;
        float deltaZ = EvalFunc(func, x, y + deltaY) - EvalFunc(func, x, y);
        return deltaZ / deltaY;
    }
}
