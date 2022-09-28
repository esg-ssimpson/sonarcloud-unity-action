using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloWorld : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // removed commented code
        Debug.Log("Submodule version: " + SubmoduleTest.SubmoduleVersion());
        Debug.Log("Adding some code to merge");
        Debug.Log("So what happens now?");
    }
}
