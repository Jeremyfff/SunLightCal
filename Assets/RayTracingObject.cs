using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RayTracingObject : MonoBehaviour {
    public int type = 0;

    private ComputeShaderTest computeShaderTest;
    private void OnEnable() {
        StartCoroutine("IE_Register");
    }
    private void OnDisable() {
        computeShaderTest.UnregisterObject(this);
    }

    private void Start() {
        
    }
    IEnumerator IE_Register() {
        bool success = false;
        while (!success) {
            try {
                computeShaderTest = ComputeShaderTest.instance;
                computeShaderTest.RegisterObject(this);
                success = true;
            } catch {

            }
            yield return null;
        }
    }


}