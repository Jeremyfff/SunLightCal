using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using TMPro;

public class MouseControl : MonoBehaviour {
    [SerializeField] Texture2D cursor;
    bool CamRotateMode = false;
    bool CamPanMode = false;
    
    private Vector3 lastMousePos;
    private float lastValue_float;
    private int lastValue_int;
    private Camera cam;
    private Vector3 camTarget;
    // Start is called before the first frame update
    void Start()
    {
        lastMousePos = Input.mousePosition;
        cam = Camera.main;
        camTarget = Vector3.zero;
        cam.transform.LookAt(camTarget);
    }

    // Update is called once per frame
    void Update()
    {
/*        if (Input.GetMouseButtonDown(1)) {
            editMode = true;
            Debug.Log("right button");
        } else if(Input.GetMouseButtonDown(0)) {
            editMode = false;
            Debug.Log("left button");
        } else {
            Debug.Log("no button");
            editMode = false;
        }*/
    }


    public void DragEvent() {
        Debug.Log("mouse drag, edit mode : " + CamRotateMode);
        var currentMousePos = Input.mousePosition;
        if (CamRotateMode) {
            var distX = currentMousePos.x - lastMousePos.x;
            var distY = currentMousePos.y - lastMousePos.y;
            
            cam.transform.RotateAround(camTarget, Vector3.up, 0.1f * distX);
            cam.transform.RotateAround(camTarget, cam.transform.right, 0.1f * -distY);
            //cam.transform.eulerAngles += new Vector3(distY, distX, 0);
            cam.transform.LookAt(camTarget);

        }else if (CamPanMode) {
            var shift = 0.01f * (currentMousePos - lastMousePos);
            cam.transform.position -= (cam.transform.right * shift.x + cam.transform.up * shift.y);
            camTarget -= (cam.transform.right * shift.x + cam.transform.up * shift.y);

        }

        lastMousePos = currentMousePos;

    }
    public void EnterEvent() {
        Debug.Log("mouse enter");
    }
    public void ExitEvent() {
        Debug.Log("mouse exit");
    }


    public void MouseDownEvent() {
        Debug.Log("Mouse down");
        if (Input.GetMouseButtonDown(1)) {
            OnEnterCameraRotateMode();
        }else if (Input.GetMouseButtonDown(2)) {
            OnEnterCameraPanMode();
        }
    }

    public void MouseUpEvent() {
        Debug.Log("Mouse up");
        OnExitCameraRotateMode();
        OnExitCameraPanMode();
    }
    public void MouseScrollEvent() {
        Debug.Log("Mouse Scroll");
        Debug.Log(Input.mouseScrollDelta);
        cam.orthographicSize -= 0.1f * Input.mouseScrollDelta.y;
    }

    private void OnEnterCameraRotateMode() {
        CamRotateMode = true;
        lastMousePos = Input.mousePosition;
    }
    private void OnExitCameraRotateMode() {
        CamRotateMode = false;

    }
    private void OnEnterCameraPanMode() {
        CamPanMode = true;
        lastMousePos = Input.mousePosition;
    }
    private void OnExitCameraPanMode() {
        CamPanMode = false;

    }
    public void SettingsMouseStartDragEvent_float(TMP_InputField input) {
        lastMousePos = Input.mousePosition;
        lastValue_float = float.Parse(input.text);
    }
    public void SettingsMouseDragEvent_float(TMP_InputField input) {
        var currentMousePos = Input.mousePosition;
        var shift = currentMousePos - lastMousePos;
        var value =float.Parse( input.text);
        value += shift.x * 0.001f * (Mathf.Abs(lastValue_float)+1);
        input.text = value.ToString();
        SettingsMouseEndDragEvent_float(input);

        lastMousePos = currentMousePos;

    }
    public void SettingsMouseEndDragEvent_float(TMP_InputField input) {
        var name = input.onEndEdit.GetPersistentMethodName(0);
        try {
            GetComponent<SettingsUIManager>().Invoke(name, 0f);
            Debug.Log("call settings ui manager function");
        } catch { }
    }

    public void SettingsOnMouseEnterEvent() {
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
    }
    public void SettingsOnMouseExitEvent() {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
