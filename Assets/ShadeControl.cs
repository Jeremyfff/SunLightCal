using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShadeControl : MonoBehaviour
{
    [SerializeField] public bool AutoUpdate;
    [SerializeField] public int mode;
    [Header("mode 1 settings")] 
    [SerializeField] public float GlobalShadeAngle;
    [SerializeField] public float GlobalShadeLengthMin;
    [SerializeField] public float GlobalShadeLengthMax;
    private float GlobalShadeLength;
    [SerializeField] public float LevelHeight;
    [SerializeField] public float GlobalShadeWidth;
    [SerializeField] public int ShadeNum;
    [SerializeField] public float ShadeAngle;
    [SerializeField] public float ShadeLength;
    [SerializeField] private GameObject ShadeInstance;
    [SerializeField] private GameObject ShadeBase;
    [SerializeField] private GameObject ShadeAxis;
    [SerializeField] private GameObject[] PVs;
    private GameObject PV;
    [SerializeField] public float Min;
    [SerializeField] public float Max;

    [Header("mode 2 added settings")]
    [SerializeField] private GameObject[] ShadeAxises;//包含第一个
    [SerializeField] private GameObject[] ShadeInstances;
    [SerializeField] public float Min2;
    [SerializeField] public float Max2;

    [Header("other settings")]
    [SerializeField] public float PVYOffset;
    [SerializeField] public float PVZOffset;
    [SerializeField] public float PVRotation;
    [SerializeField] public float PVHeight;
    [SerializeField] public float ShadeZOffset;

    private List<GameObject> ShadeList = new List<GameObject>();
    public static ShadeControl instance;
    
    private SunManager sunManager;
    private GameInfo gameInfo;
    private void Awake() {
        ShadeControl.instance = this;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        try {
            Gizmos.color = Color.blue;
            foreach (var PV in PVs) {
                
                Gizmos.DrawWireSphere(PV.transform.position, 0.1f);
                Gizmos.DrawLine(PV.transform.position, PV.transform.position + PV.transform.forward);
            }
            

        } catch {

        }
        
    }
    // Start is called before the first frame update
    void Start()
    {
        gameInfo = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameInfo>();
        if (gameInfo == null) {
            SceneManager.LoadScene(0);
        }
        
        for (int i = 0; i < PVs.Length; i++) {
            if(i == gameInfo.currentPVMode) {
                PV = PVs[i];
            } else {
                Destroy(PVs[i]);
            }
        }

        if(gameInfo.currentPVMode == 2) {
            //tradional
            //try { Destroy(ShadeAxis); } catch { }
            try { Destroy(ShadeInstance); } catch { }
            try { Destroy(ShadeBase); } catch { }
            try { foreach (var a in ShadeInstances) { Destroy(a); } } catch { }
            DestroyAllShades();
        }
/*        try {
            PV = PVs[gameInfo.currentPVMode];
        } catch {
            PV = PVs[0];
        }*/

        sunManager = SunManager.instance;

        mode = gameInfo.currentModelMode;
        if (mode == 0) {
            Debug.Log("current model mode = 0");
            Min2 = GlobalShadeAngle;
            Max2 = Min2 + 0.1f;
            Debug.Log("Min2 : " + Min2 + "max2 : " + Max2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (AutoUpdate) {
            UpdateModel();
        }
        
    }

    private void UpdateModel() {
        if (gameInfo.currentPVMode != 2) {
            DestroyAllShades();


            var sun = Mathf.PI / 2 - sunManager.GetSunHs();

            var alpha = GlobalShadeAngle / 180f * Mathf.PI;

            GlobalShadeLength = LevelHeight * (Mathf.Cos(-alpha) * Mathf.Tan(alpha + sun) + Mathf.Sin(-alpha));

            GlobalShadeLength = GlobalShadeLength < GlobalShadeLengthMax ? GlobalShadeLength : GlobalShadeLengthMax;
            GlobalShadeLength = GlobalShadeLength > GlobalShadeLengthMin ? GlobalShadeLength : GlobalShadeLengthMin;



            if (mode == 0 ) {


                ShadeAxis.transform.eulerAngles = new Vector3(GlobalShadeAngle, 0, 0);
                ShadeBase.transform.localScale = new Vector3(GlobalShadeWidth * 2, ShadeBase.transform.localScale.y, GlobalShadeLength);
                ShadeBase.transform.localPosition = new Vector3(0, 0, -0.5f * GlobalShadeLength);
                ShadeInstance.transform.localEulerAngles = new Vector3(ShadeAngle - GlobalShadeAngle, 0, 0);
                ShadeInstance.transform.localScale = new Vector3(GlobalShadeWidth * 2, ShadeInstance.transform.localScale.y, ShadeLength);
                GenerateShades();
            } else if (mode == 1) {

                ShadeAxis.transform.eulerAngles = new Vector3(GlobalShadeAngle, 0, 0);

                for (int i = 0; i < ShadeInstances.Length; i++) {
                    var shadeInstance = ShadeInstances[i];
                    var shadeAxis = ShadeAxises[i];

                    shadeInstance.transform.localScale = new Vector3(GlobalShadeWidth * 2, shadeInstance.transform.localScale.y, ShadeLength);
                    //Destroy(shadeInstance.GetComponentInChildren<RayTracingObject>());
                    shadeInstance.transform.localPosition = new Vector3(0, 0, -0.5f * ShadeLength);
                    if (i != 0) {
                        shadeAxis.transform.localEulerAngles = new Vector3(ShadeAngle, 0, 0);
                        shadeAxis.transform.localPosition = new Vector3(0, 0, -ShadeLength);
                    }


                

                    var shadeInstance_copy = GameObject.Instantiate(shadeInstance, shadeInstance.transform.parent.transform);
                    //shadeInstance_copy.transform.localPosition = shadeInstance.transform.localPosition;
                    shadeInstance_copy.transform.position += new Vector3(0, LevelHeight, 0);
                    shadeInstance_copy.transform.localScale = new Vector3(shadeInstance_copy.transform.localScale.x * 20, shadeInstance_copy.transform.localScale.y, shadeInstance_copy.transform.localScale.z);
                    ShadeList.Add(shadeInstance_copy);

                }


            }

            PV.transform.position = new Vector3(PV.transform.position.x, PVYOffset + 0.5F * PVHeight, 0.092f + PVZOffset);
            PV.transform.eulerAngles = new Vector3(-PVRotation, 0, 0);
            PV.transform.localScale = new Vector3(PV.transform.localScale.x, PVHeight, PV.transform.localScale.z);
            this.transform.position = new Vector3(0, 0, -ShadeZOffset);
        }
    }

    private void GenerateShades() {
        float step = GlobalShadeLength / ShadeNum;
        ShadeInstance.GetComponent<MeshRenderer>().enabled = true;

        for (int i = 0; i < ShadeNum; i++) {
            var child = GameObject.Instantiate(ShadeInstance, ShadeAxis.transform);
            child.transform.localPosition = new Vector3(0, 0, -step * i);
            child.transform.localEulerAngles = new Vector3(ShadeAngle - GlobalShadeAngle, 0, 0);
            ShadeList.Add(child);
        }
        ShadeInstance.GetComponent<MeshRenderer>().enabled = false;
    }
    private void DestroyAllShades() {
        foreach(var shade in ShadeList) {
            Destroy(shade);
        }
        ShadeList.Clear();
    }

    public void SetAndUpdateInputVar(float var) {
        ShadeAngle = var;
        UpdateModel();
    }
    internal void SetAndUpdateInputVar2(float var) {
        GlobalShadeAngle = var;
        UpdateModel();
    }

    public void GetInputRange(out float a,out float b) {
        a = Min;
        b = Max;
    }
    public float GetMinInputRange() {
        return this.Min;
    }

    public float GetMaxInputRange() {
        return this.Max;
    }

    public float GetMinInputRange2() {
        return this.Min2;
    }

    public float GetMaxInputRange2() {
        return this.Max2;
    }

    public float GetPVPercentage() {
        return GlobalShadeWidth / this.PV.transform.localScale.x;
    }

    public float GetPVArea() {
        return PV.transform.localScale.x * PV.transform.localScale.y * GetPVPercentage();
    }

    public float GetPVProjectArea(Vector3 dir) {
        var angle = Vector3.Angle(-PV.transform.forward, dir);
        angle = angle > 90 ? 90 : angle;
        return Mathf.Cos(angle/180 * Mathf.PI) * GetPVArea(); 
    }


}
