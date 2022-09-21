using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloWorld : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // commenting out code to give SonarCloud something to complain about on the PR
        //Debug.Log("Behaviour Working. Time to take Monday off!");
        Debug.Log("Submodule version: " + SubmoduleTest.SubmoduleVersion());
    }

    // Update is called once per frame
    // This function is here to give SonarCloud something to complain about
    void Update()
    {
        
    }
}
