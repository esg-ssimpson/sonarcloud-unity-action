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
    }

    // Update is called once per frame
    // This function is here to give SonarCloud something to complain about
    void Update()
    {
        
    }
}
