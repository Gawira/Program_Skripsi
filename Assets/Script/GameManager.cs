using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Cameras;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LockOnTarget lockOnSystem;

    void Start()
    {
        
    QualitySettings.vSyncCount = 0; //  VSync
    Application.targetFrameRate = 60;
        
    }

    void Update()
    {

    }

}
