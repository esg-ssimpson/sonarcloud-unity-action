using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EastSideGames.Game
{
    public class HelloWorld : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("Submodule version: " + SubmoduleTest.SubmoduleVersion());
            Debug.Log("This feature is awesome!");
        }

        public static string TestTest()
        {
            // making a change
            return "Yes";
        }
    }
}

