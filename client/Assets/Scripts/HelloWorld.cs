﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloWorld : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Submodule version: " + SubmoduleTest.SubmoduleVersion());
        Debug.Log("This feature is awesome!");
        //Debug.Log("I didn't need this code though");        
    }
}
