using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ObjectTest : MonoBehaviour
    {
        [Header("Test")]
        public ObjectLights objectLights;

        private void Start()
        {
            if (objectLights != null)
            {
                objectLights.ExecuteRandomAction();
            }
        }
    }