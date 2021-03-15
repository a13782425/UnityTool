using Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicTest : MonoBehaviour
{
    public LogicGraphBase LogicGraph;
    // Start is called before the first frame update
    void Start()
    {
        LogicRuntime.Instance.Begin(LogicGraph, onFinish);
    }

    private void onFinish()
    {
        Debug.LogError("逻辑图结束回调");
    }
}
