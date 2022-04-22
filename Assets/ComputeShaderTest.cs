//new
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System;

public class ComputeShaderTest : MonoBehaviour {
    [Header("全局设置")]
    [Tooltip("手动进行一次计算")]
    [SerializeField] public bool ManualCal;
    [Tooltip("自动更新")]
    [SerializeField] public bool AutoUpdate;
    [Tooltip("是否预览")]
    [SerializeField] public bool Preview;
    [Tooltip("采样数量。不要在运行时更改")]
    [SerializeField] public int size;//do not change while running
    [Tooltip("次采样等级")]
    [SerializeField] public int _multiSample;
    [Tooltip("刷新")]
    [SerializeField] public bool Refresh;
    [Tooltip("内圈细分")]
    [SerializeField] public int _innerSub = 8;
    [Tooltip("内圈细分次数(循环次数)")]
    [SerializeField] public int _innerLoopTime = 4;
    [Tooltip("外圈细分")]
    [SerializeField] public int _outterSub = 8;
    [Tooltip("外圈细分次数(循环次数)")]
    [SerializeField] public int _outterLoopTime = 4;
    [Tooltip("是否使用宽范围")]
    [SerializeField] public bool _useWiderRange = true;

    [Header("最优解计算")]
    [SerializeField] public bool startCal;
    [SerializeField] public bool stopCal;
    [SerializeField] public int startDayIndex;
    [SerializeField] public int endDayIndex;
    [SerializeField] public int startMinuteIndex;
    [SerializeField] public int endMinuteIndex;
    [SerializeField] public int DayStep;
    [SerializeField] public int MinuteStep;
    [SerializeField] public bool SaveAfterCal;
    [SerializeField] public string FileSavePath;

    [Header("shader相关设置")]
    [Tooltip("compute Shader文件")]
    public ComputeShader computeShader;

    public Transform _emitSurface;
    public float _emitDistance = 10f;
    public float _emitRange = 5f;
    private float _emitArea;
    [Tooltip("直射光")]
    public Light _directionalLight;

    [Tooltip("计算结果将被储存在这张RenderTexture中")]
    public RenderTexture result;
    [Tooltip("用以显示的image组件。当preview = false时，不会预览")]
    public Image previewImage;
    public RenderTexture TargetTexture;
    public Image cameraPreviewImage;
    public Camera camera;
    private long startTime;
    //instances
    private SunManager sunManager;
    [SerializeField] private ShadeControl[] shadeControls;
    private ShadeControl shadeControl;
    private GameInfo gameInfo;
    [HideInInspector] public string defaultFileName = "";

    //data
    private List<float> DirectHav;



    //compute shader
    private static bool _meshObjectsNeedRebuilding = true;
    private static List<RayTracingObject> _rayTracingObjects = new List<RayTracingObject>();

    private static List<MeshObject> _meshObjects = new List<MeshObject>();

    private static List<Vector3> _vertices = new List<Vector3>();
    private static List<int> _indices = new List<int>();
    private static List<int> _types = new List<int>();
    private int[] _score = new int[4];
    


    private ComputeBuffer _meshObjectBuffer;
    private ComputeBuffer _ScoreBuffer;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _indexBuffer;
    private ComputeBuffer _typeBuffer;


    //for gc
    private Texture2D texout;
    private Texture2D cameratex;
    private Sprite sprite;
    private Sprite camerasprite;

    //private ComputeBuffer _ScoreBuffer;
    public bool complete;

    struct MeshObject {
        public Matrix4x4 localToWorldMatrix;
        public int indices_offset;
        public int indices_count;
    }
    public static ComputeShaderTest instance;
    private void Awake() {
        ComputeShaderTest.instance = this;
    }
    private void Start() {
        complete = false;
        try {
            gameInfo = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameInfo>();
        } catch {

        }
        
        if(gameInfo == null) {
            SceneManager.LoadScene(0);
        }

        
        sunManager = SunManager.instance;

        for (int i = 0; i < shadeControls.Length; i++) {
            if(i == gameInfo.currentModelMode) {
                shadeControl = shadeControls[i];
            } else {
                
                Destroy(shadeControls[i].gameObject);
            }
        }


        DirectHav = FileOperator.ReadDirectHav();


        Debug.Log("ComputeShaderTest.size = "+size);
        complete = true;

    }
    private void Update() {
        
        if (ManualCal || AutoUpdate) {
            ManualCal = false;
            float score1;
            float score2;
            SingleCal(out score1,out score2, Preview);

        }

        if (startCal) {

            StartCalEvent();
            startCal = false;
            
        }

        if (stopCal) {
            StopCalEvent();
            stopCal = false;
        }

        if (Refresh) {
            RefreshEvent();
            Refresh = false;
        }

    }

    public void StartCalEvent() {
        if(gameInfo.currentModelMode == 0) {
            StartCoroutine("IE_CalLoop_org");
        } else {
            StartCoroutine("IE_CalLoop");
        }
        
        
    }

    public void StopCalEvent() 
    {
        if (gameInfo.currentModelMode == 0) {
            StopCoroutine("IE_CalLoop_org");
        } else {
            StopCoroutine("IE_CalLoop");
        }
    }

    public void RefreshEvent() {
        InitRenderTexture();
        InitCameraTexture();
    }

    IEnumerator IE_CalLoop() {
        //init
        startTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        int ComputeCount = 0;
        AutoUpdate = false;
        sunManager.AutoUpdate = false;
        shadeControl.AutoUpdate = false;

        var currentDayIndex = startDayIndex;
        var currentMinuteIndex = startMinuteIndex;

        var defaultInputVar = shadeControl.GetMinInputRange();
        var defaultInputVar2 = shadeControl.GetMinInputRange2();

        var PVPercentage = shadeControl.GetPVPercentage();
        _emitArea = _emitRange * _emitRange * 4;
        var PVArea = shadeControl.GetPVArea();
        sunManager.SetTimeAndUpdate(0, currentDayIndex, 0, currentMinuteIndex);


        //cal loop
/*        List<float> bestInputVarList = new List<float>();
        List<float> bestInputVarList2 = new List<float>();
        List<float> bestDirectScoreList = new List<float>();
        List<float> bestDirectValueList = new List<float>();
        List<float> bestReflectScoreList = new List<float>();
        List<float> bestReflectValueList = new List<float>();
        List<float> bestTotalScoreList = new List<float>();
        List<float> bestTotalValueList = new List<float>();
        List<float> bestPVScoreList = new List<float>();
        List<float> bestPVValueList = new List<float>();
        List<bool> needToCalList = new List<bool>();
        List<int> dateIndexList = new List<int>();
        List<int> minuteIndexList = new List<int>();*/

        Data data = new Data();
        foreach(var graph in Graph.instanceList) {
            graph.SetInternalData(data);//将data赋予graph
        }
        

        int dayCount =(int) Mathf.Ceil((endDayIndex - startDayIndex) / DayStep ) + 1;
        int minuteCount = (int)Mathf.Ceil((endMinuteIndex - startMinuteIndex) / MinuteStep) + 1 ;


        float progressStep = (1f / (float)(dayCount)) / (float)(minuteCount);
        Debug.Log("dayCount :" + dayCount + ", minuteCount: " + minuteCount);
        Debug.Log("progressStep" + progressStep);
        float currentProgress = 0f;

        //UI更新
        //开始计算时
        //设置XY范围

        List<Graph> graphList = new List<Graph>();
        graphList.Add(DownPanelUI.graph1);
        graphList.Add(DownPanelUI.graph2);
        for (int i = 0; i < graphList.Count; i++) {
            var graph = graphList[i];
            graph.ClearGraph();

            graph.SetXRange(startMinuteIndex, endMinuteIndex, minuteCount);

            switch (graph.acceptGraphType) {
                case 0: graph.SetYRange(shadeControl.GetMinInputRange(), shadeControl.GetMaxInputRange(), 10); break;
                case 1: graph.SetYRange(0, 3f, 10); break;
                case 2: graph.SetYRange(0, 800f, 10); break;
                case 3: graph.SetYRange(shadeControl.GetMinInputRange2(), shadeControl.GetMaxInputRange2(), 10); break;

            }
            if(i == 0) {
                graph.GenerateColor(dayCount);
            } else {
                graph.GetColorFromGraph(graphList[0]);
            }
            
            
        }

        

        int colorGroup = 0;
        while (currentDayIndex < endDayIndex) {
            //每一天发生的事件
            currentMinuteIndex = startMinuteIndex;
            Debug.Log("==》》》》》》》》》》》Current day index: " + currentDayIndex);

            //UI更新
            //清除图表
            //DownPanelUI.graph.ClearGraph();
            foreach(var graph in graphList) {
                graph.FirstTimeInADay = true;
            }

            while (currentMinuteIndex < endMinuteIndex) {
                //每一分钟发生的事件
                Debug.Log("==>>>>>>>>>>>>>>>>>>>进入该时刻计算, current minute index : " + currentMinuteIndex);

                sunManager.SetTimeAndUpdate(0, currentDayIndex, 0, currentMinuteIndex);


                //更新UI
                DownPanelUI.instance.SetTimeText(currentDayIndex, currentMinuteIndex);




                //进行优化
                bool needToCal = true;
                //float bestScore = 0;
                float bestDirectScore = 0;
                float bestReflectScore = 0;
                float bestTotalScore = 0;
                float totalValueSum = 0;
                float PVValueSum = 0;

                float bestInputVar = defaultInputVar;
                float bestInputVar2 = defaultInputVar2;
                float defaultSampleDegree2;
                if (gameInfo.currentModelMode == 0) {
                    defaultSampleDegree2 = 100;
                } else {
                    defaultSampleDegree2 = shadeControl.GetMaxInputRange2() - shadeControl.GetMinInputRange2();
                }
                
                Debug.Log("defaultSampleDegree2 : " + defaultSampleDegree2);

                int var2SampleTime = shadeControl.mode == 0 ? 1 : _outterLoopTime;
                
                for (int j = 0; j < var2SampleTime; j++) {//outer
                    Debug.Log("==########################################################################");
                    Debug.Log("=====outer loop =======精细度等级: " + j + ", 最大精细度 : " + var2SampleTime);
                    if (Vector3.Dot(sunManager.GetSunDir(), new Vector3(0, 1, 0)) > 0 || Vector3.Dot(sunManager.GetSunDir(), new Vector3(0, 0, 1)) < 0) {
                        needToCal = false;
                        Debug.Log("don't need to cal");
                        break;
                    }
                    float maxInputVar2;
                    float currentInputVar2;
                    if (j == 0) {
                        Debug.Log("currentInputVar2 已被重置");
                        currentInputVar2 = defaultInputVar2;
                        maxInputVar2 = shadeControl.GetMaxInputRange2();
                    } else {
                        Debug.Log("currentInputVar2 未被重置");
                        if (_useWiderRange) {
                            currentInputVar2 = bestInputVar2 - defaultSampleDegree2;
                            maxInputVar2 = bestInputVar2 + defaultSampleDegree2;
                        } else {
                            currentInputVar2 = bestInputVar2 - defaultSampleDegree2/2f;
                            maxInputVar2 = bestInputVar2 + defaultSampleDegree2/2f;
                        }
                        
                    }
                    defaultSampleDegree2 /= _outterSub;//每次采样个数
                    Debug.Log("====当前采样精度(outer)====" + defaultSampleDegree2);

                    int counter2 = 0;
                   
                    while (currentInputVar2 < maxInputVar2) {
                        Debug.Log("======outer进入当前精细度下循环: counter2:"+counter2+",精细度等级 : "+ j);
                        //Debug.Log("while " + currentInputVar2 + "(currentInputVar2) <" + maxInputVar2 + "maxInputVar2");
                        //每一次更新inputvar2
                        shadeControl.SetAndUpdateInputVar2(currentInputVar2);
                        //复位参数
                        float bestDirectScore_ = 0;
                        float bestReflectScore_ = 0;
                        float bestTotalScore_ = 0;
                        float bestInputVar_ = defaultInputVar;

                        //float defaultSampleDegree = shadeControl.GetMaxInputRange() - shadeControl.GetMinInputRange();
                        float defaultSampleDegree = 60f;
                        //开始outer 某值下的单input var计算******************************************************************
                        
                        for (int i = 0; i < _innerLoopTime; i++) {//进行多少次精细划分
                            //inner loop
                            Debug.Log("--------------inner loop------------");

                            float maxInputVar;
                            float currentInputVar;
                            if (i == 0) {
                                currentInputVar = defaultInputVar;
                                maxInputVar = shadeControl.GetMaxInputRange();
                            } else {
                                if (_useWiderRange) {
                                    currentInputVar = bestInputVar - defaultSampleDegree;
                                    maxInputVar = bestInputVar + defaultSampleDegree;
                                } else {
                                    currentInputVar = bestInputVar - defaultSampleDegree/2f;
                                    maxInputVar = bestInputVar + defaultSampleDegree/2f;
                                }
                                
                            }

                            defaultSampleDegree /= _innerSub;//每次采样个数

                            Debug.Log("-inner : 当前精细度等级 :"+i+"/"+_innerLoopTime+" , 当前采样精度" + defaultSampleDegree);


                            int counter = 0;
                            while (currentInputVar < maxInputVar) {
                                //每一次更新inputvar
                                shadeControl.SetAndUpdateInputVar(currentInputVar);
                                float score1;
                                float score2;
                                SingleCal(out score1, out score2, Preview);
                                ComputeCount++;

                                score1 *= PVPercentage * _emitArea;//直接光的等效面积
                                score2 *= _emitArea;//反射光的等效面积

                                if (score1 + score2 > bestTotalScore_) {
                                    bestDirectScore_ = score1;
                                    bestReflectScore_ = score2;
                                    bestTotalScore_ = score1 + score2;
                                    bestInputVar_ = currentInputVar;
                                }


                                currentInputVar += defaultSampleDegree;
                                counter++;
                                

                            }
                            
                            Debug.Log("-inner : 当前精度下采样了" + counter + "次");



                               yield return null;


                        }//单input var计算完毕******************************************************************


                        if (bestTotalScore_ > bestTotalScore) {
                            bestReflectScore = bestReflectScore_;
                            bestDirectScore = bestDirectScore_;
                            bestTotalScore = bestTotalScore_;
                            bestInputVar = bestInputVar_;
                            bestInputVar2 = currentInputVar2;
                        }
                        currentInputVar2 += defaultSampleDegree2;
                        counter2++;
                        Debug.Log("next currentInputVar2 = " + currentInputVar2);

                    }
                    Debug.Log("=====outer : 当前精度采样结束，当前精度下采样了" + counter2 + "次");


                }
                Debug.Log("===================outer本轮计算完毕================");
                float directHav = GetAvgDirectHav(currentDayIndex, currentMinuteIndex,DayStep);
                var bestPVScore = PVArea;

                var bestDirectValue = bestDirectScore * directHav;
                var bestReflectValue = bestReflectScore * directHav;
                var bestTotalValue = bestDirectValue + bestReflectValue;
                var bestPVValue = bestPVScore * directHav;

                totalValueSum += bestTotalValue * DayStep;
                PVValueSum += bestPVValue * DayStep;

                data.bestDirectScoreList.Add(bestDirectScore);
                data.bestReflectScoreList.Add(bestReflectScore);
                data.bestTotalScoreList.Add(bestTotalScore);
                data.bestPVScoreList.Add(bestPVScore);
                data.bestDirectValueList.Add(bestDirectValue);
                data.bestReflectValueList.Add(bestReflectValue);
                data.bestTotalValueList.Add(bestTotalValue);
                data.bestPVValueList.Add(bestPVValue);
                data.bestInputVarList.Add(bestInputVar);
                data.bestInputVarList2.Add(bestInputVar2);
                data.dateIndexList.Add(currentDayIndex);
                data.minuteIndexList.Add(currentMinuteIndex);
                data.needToCalList.Add(needToCal);


                //UI更新

                currentProgress += progressStep;
                Debug.Log("progressStep"+progressStep);
                DownPanelUI.instance.SetProgress(currentProgress);
                DownPanelUI.instance.SetSumText(totalValueSum, PVValueSum);

                //如果太阳位置大于设定值，则更新graph
                if (needToCal) {
                    foreach(var graph in graphList) {
                        if (graph.acceptGraphType == 0 && graph.graphSource == 0) {
                            //绘制best input
                            graph.AddData(currentMinuteIndex, bestInputVar, colorGroup, !graph.FirstTimeInADay);
                            graph.FirstTimeInADay = false;
                            continue;
                        }
                        if(graph.acceptGraphType == 1 && graph.graphSource == 0) {
                            //绘制best score
                            graph.AddBar(currentMinuteIndex, bestTotalScore, colorGroup,0.8f);
                            graph.AddBar(currentMinuteIndex, bestReflectScore, colorGroup, 1f);
                            graph.AddData(currentMinuteIndex, bestPVScore, 0, !graph.FirstTimeInADay);
                            graph.FirstTimeInADay = false;
                            continue;
                        }
                        if(graph.acceptGraphType == 2 && graph.graphSource == 0) {
                            //绘制hav
                            graph.AddBar(currentMinuteIndex, bestTotalValue, colorGroup, 0.8f);
                            graph.AddBar(currentMinuteIndex, bestReflectValue, colorGroup, 1f);
                            graph.AddData(currentMinuteIndex, bestPVValue, 0, !graph.FirstTimeInADay);
                            graph.FirstTimeInADay = false;

                            continue;
                        }
                        if (graph.acceptGraphType == 3 && graph.graphSource == 0) {
                            //绘制best input 2
                            graph.AddData(currentMinuteIndex, bestInputVar2, colorGroup, !graph.FirstTimeInADay);
                            graph.FirstTimeInADay = false;
                            continue;
                        }
                    }


                    
                }
                

                currentMinuteIndex += MinuteStep;
                Debug.Log("==<<<<<<<<<<<<<<<<<<<该时刻计算完毕"+sunManager.ToString());
                //该时刻计算完毕
                
            }

            colorGroup++;
            currentDayIndex += DayStep;
            //该天计算完毕

        }



        //计算完成
        Debug.Log("CAL COMPLLETE! COMPUTE COUNT = "+ComputeCount);

        //UI更新
        DownPanelUI.instance.CalCompleteEvent();
        SettingsUIManager.instance.ReadAll();
        AutoUpdate = true;
        sunManager.AutoUpdate = true;
        shadeControl.AutoUpdate = true;

        if (SaveAfterCal) {
            data.info =
                "D:" + startDayIndex + ":" + endDayIndex + ":" + DayStep +
                "-T:" + startMinuteIndex + ":" + endMinuteIndex + ":" + MinuteStep +
                "-S:" + size + ":" + _multiSample + ":" + _emitRange + ":" + _emitDistance +
                "-L:" + sunManager.lon + ":" + sunManager.lat +
                "-M:" + shadeControl.GlobalShadeAngle + ":" + shadeControl.GlobalShadeLengthMin + ":" + shadeControl.GlobalShadeLengthMax +
                ":" + shadeControl.GlobalShadeWidth + ":" + shadeControl.ShadeNum + ":" + shadeControl.GetMinInputRange() + ":" +
                shadeControl.GetMaxInputRange() + ":" + shadeControl.ShadeLength;

            /* Data data = new Data(dateIndexList, minuteIndexList, bestInputVarList, bestInputVarList2,
                 bestDirectScoreList, bestReflectScoreList, bestTotalScoreList, bestPVScoreList,
                 bestDirectValueList, bestReflectValueList, bestTotalValueList, bestPVValueList,
                 needToCalList,
                 "D:" + startDayIndex + ":" + endDayIndex + ":" + DayStep +
                 "-T:" + startMinuteIndex + ":" + endMinuteIndex + ":" + MinuteStep +
                 "-S:" + size + ":" + _multiSample + ":" + _emitRange + ":" + _emitDistance +
                 "-L:" + sunManager.lon + ":" + sunManager.lat +
                 "-M:" + shadeControl.GlobalShadeAngle + ":" + shadeControl.GlobalShadeLengthMin + ":" + shadeControl.GlobalShadeLengthMax +
                 ":" + shadeControl.GlobalShadeWidth + ":" + shadeControl.ShadeNum + ":" + shadeControl.GetMinInputRange() + ":" +
                 shadeControl.GetMaxInputRange() + ":" + shadeControl.ShadeLength

                 );*/
            try { FileOperator.WriteToCsv(data,defaultFileName); } catch { }
            

            //info example :   D:0:365:30-T:361:1081:30-S:256:2:3.5:10-L:120:32-M:-20:0.5:1.2:2.0:6:-20:40:0.2
            Debug.Log("File Save to = " + FileSavePath);
        }
        

    }


    IEnumerator IE_CalLoop_org() {
        Debug.Log("进入原始计算协程");
        //init
        int ComputeCount = 0;
        AutoUpdate = false;
        sunManager.AutoUpdate = false;
        shadeControl.AutoUpdate = false;

        var currentDayIndex = startDayIndex;
        var currentMinuteIndex = startMinuteIndex;
        var defaultInputVar = shadeControl.GetMinInputRange();
        var PVPercentage = shadeControl.GetPVPercentage();
        var EmitArea = _emitRange * _emitRange * 4;
        var PVArea = shadeControl.GetPVArea();
        sunManager.SetTimeAndUpdate(0, currentDayIndex, 0, currentMinuteIndex);


        
        /*List<float> bestInputVarList = new List<float>(); 
        List<float> bestInputVarList2 = new List<float>();
        List<float> bestDirectScoreList = new List<float>();
        List<float> bestDirectValueList = new List<float>();
        List<float> bestReflectScoreList = new List<float>();
        List<float> bestReflectValueList = new List<float>();
        List<float> bestTotalScoreList = new List<float>();
        List<float> bestTotalValueList = new List<float>();
        List<float> bestPVScoreList = new List<float>();
        List<float> bestPVValueList = new List<float>();
        List<bool> needToCalList = new List<bool>();
        List<int> dateIndexList = new List<int>();
        List<int> minuteIndexList = new List<int>();*/
        Data data = new Data();

        foreach(var graph in Graph.instanceList) {
            graph.SetInternalData(data);//将data 赋予graph
        }
        float valuesum = 0;
        float pvsum = 0;

        //cal loop
        int dayCount = (endDayIndex - startDayIndex) / DayStep + 1;
        int minuteCount = (endMinuteIndex - startMinuteIndex) / MinuteStep  +1;
        float progressStep = 1f / dayCount / minuteCount ;
        float currentProgress = 0f;
        Debug.Log("一共需要计算" + dayCount + "天, 每天" + minuteCount + "个时段");


        //UI更新
        //开始计算时
        //设置XY范围
        List<Graph> graphList = new List<Graph>();
        graphList.Add(DownPanelUI.graph1);
        graphList.Add(DownPanelUI.graph2);
        for (int i = 0; i < graphList.Count; i++) {
            var graph = graphList[i];
            graph.ClearGraph();

            graph.SetXRange(startMinuteIndex, endMinuteIndex, minuteCount);

            switch (graph.acceptGraphType) {
                case 0: graph.SetYRange(shadeControl.GetMinInputRange(), shadeControl.GetMaxInputRange(), 10); break;
                case 1: graph.SetYRange(0, 3f, 10); break;
                case 2: graph.SetYRange(0, 800f, 10); break;

            }
            if (i == 0) {
                graph.GenerateColor(dayCount);
            } else {
                graph.GetColorFromGraph(graphList[0]);
            }


        }


        Debug.Log("开始进入计算循环》》》》》》》");
        int colorGroup = 0;
        while (currentDayIndex < endDayIndex) {
            //每一天发生的事件
            Debug.Log("currentDayIndex : " + currentDayIndex);
            currentMinuteIndex = startMinuteIndex;
            Debug.Log("currentMinuteIndex 已被重置为: " + currentMinuteIndex);

            //UI更新
            //清除图表
            //DownPanelUI.graph.ClearGraph();
            foreach (var graph in graphList) {
                graph.FirstTimeInADay = true;
            }
            Debug.Log("进入分钟循环》》》》");
            while (currentMinuteIndex < endMinuteIndex) {
                //每一分钟发生的事件
                Debug.Log("current minute index : " + currentMinuteIndex);

                sunManager.SetTimeAndUpdate(0, currentDayIndex, 0, currentMinuteIndex);


                //更新UI
                DownPanelUI.instance.SetTimeText(currentDayIndex, currentMinuteIndex);




                //进行优化
                //float bestScore = 0;
                float bestDirectScore = 0;
                float bestReflectScore = 0;
                float bestTotalScore = 0;
                float bestInputVar = defaultInputVar;
                float defaultSampleDegree = 60f;



                bool needToCal = true;



                for (int i = 0; i < _innerSub; i++) {//进行多少次精细划分

                    if (Vector3.Dot(sunManager.GetSunDir(), new Vector3(0, 1, 0)) > 0 || Vector3.Dot(sunManager.GetSunDir(), new Vector3(0, 0, 1)) < 0) {
                        needToCal = false;

                        Debug.Log("don't need to cal");
                        break;
                    }

                    float maxInputVar;
                    float currentInputVar;
                    if (i == 0) {
                        currentInputVar = bestInputVar;
                        maxInputVar = shadeControl.GetMaxInputRange();
                    } else {
                        if (_useWiderRange) {
                            currentInputVar = bestInputVar - defaultSampleDegree;
                            maxInputVar = bestInputVar + defaultSampleDegree;
                        } else {
                            currentInputVar = bestInputVar - defaultSampleDegree/2f;
                            maxInputVar = bestInputVar + defaultSampleDegree/2f;
                        }
                        
                    }

                    defaultSampleDegree /= _innerLoopTime;//每次采样个数

                    Debug.Log("当前采样精度" + defaultSampleDegree);

                    int counter = 0;
                    while (currentInputVar < maxInputVar) {
                        //每一次更新inputvar
                        shadeControl.SetAndUpdateInputVar(currentInputVar);
                        float score1;
                        float score2;
                        SingleCal(out score1, out score2, Preview);
                        ComputeCount++;

                        //score1 *= PVPercentage * EmitArea;//直接光的等效面积
                        score1 *= EmitArea;//直接光的等效面积
                        score2 *= EmitArea;//反射光的等效面积

                        if (score1 + score2 > bestTotalScore) {
                            bestDirectScore = score1;
                            bestReflectScore = score2;
                            bestTotalScore = score1 + score2;
                            bestInputVar = currentInputVar;
                        }


                        currentInputVar += defaultSampleDegree;
                        counter++;
                        

                    }
                    Debug.Log("当前精度下采样了" + counter + "次");
                    yield return null;
                }
                Debug.Log("采样完毕");
                
                float directHav = GetAvgDirectHav(currentDayIndex, currentMinuteIndex, DayStep);
                var bestPVScore = PVArea;

                var bestDirectValue = bestDirectScore * directHav;
                var bestReflectValue = bestReflectScore * directHav;
                var bestTotalValue = bestDirectValue + bestReflectValue;
                var bestPVValue = bestPVScore * directHav;

                valuesum += bestTotalValue * DayStep;
                pvsum += bestPVValue * DayStep;

                data.bestDirectScoreList.Add(bestDirectScore);
                data.bestReflectScoreList.Add(bestReflectScore);
                data.bestTotalScoreList.Add(bestTotalScore);
                data.bestPVScoreList.Add(bestPVScore);

                data.bestDirectValueList.Add(bestDirectValue);
                data.bestReflectValueList.Add(bestReflectValue);
                data.bestTotalValueList.Add(bestTotalValue);
                data.bestPVValueList.Add(bestPVValue);


                data.bestInputVarList.Add(bestInputVar);
                data.bestInputVarList2.Add(0);

                data.dateIndexList.Add(currentDayIndex);
                data.minuteIndexList.Add(currentMinuteIndex);

                data.needToCalList.Add(needToCal);

                //UI更新
                currentProgress += progressStep;
                DownPanelUI.instance.SetProgress(currentProgress);
                DownPanelUI.instance.SetSumText(valuesum, pvsum);
                //如果太阳位置大于设定值，则更新graph
                if (needToCal) {
                    foreach (var graph in graphList) {
                        if (graph.acceptGraphType == 0 && graph.graphSource == 0) {
                            //绘制best input
                            graph.AddData(currentMinuteIndex, bestInputVar, colorGroup, !graph.FirstTimeInADay);
                            graph.FirstTimeInADay = false;
                            continue;
                        }
                        if (graph.acceptGraphType == 1 && graph.graphSource == 0) {
                            //绘制best score
                            graph.AddBar(currentMinuteIndex, bestTotalScore, colorGroup, 0.8f);
                            graph.AddBar(currentMinuteIndex, bestReflectScore, colorGroup, 1f);
                            graph.AddData(currentMinuteIndex, bestPVScore, 0, !graph.FirstTimeInADay);
                            graph.FirstTimeInADay = false;
                            continue;
                        }
                        if (graph.acceptGraphType == 2 && graph.graphSource == 0) {
                            graph.AddBar(currentMinuteIndex, bestTotalValue, colorGroup, 0.8f);
                            graph.AddBar(currentMinuteIndex, bestReflectValue, colorGroup, 1f);
                            graph.AddData(currentMinuteIndex, bestPVValue, 0, !graph.FirstTimeInADay);
                            graph.FirstTimeInADay = false;

                            continue;
                        }
                    }



                }


                currentMinuteIndex += MinuteStep;
                Debug.Log(sunManager.ToString()+"计算完毕");
                //该时刻计算完毕

            }

            colorGroup++;
            currentDayIndex += DayStep;
            //该天计算完毕

        }



        //计算完成
        Debug.Log("CAL COMPLLETE! COMPUTE COUNT = " + ComputeCount);

        //UI更新
        DownPanelUI.instance.CalCompleteEvent();
        SettingsUIManager.instance.ReadAll();
        AutoUpdate = true;
        sunManager.AutoUpdate = true;
        shadeControl.AutoUpdate = true;

        if (SaveAfterCal) {
            data.info = 
                "D:" + startDayIndex + ":" + endDayIndex + ":" + DayStep +
                "-T:" + startMinuteIndex + ":" + endMinuteIndex + ":" + MinuteStep +
                "-S:" + size + ":" + _multiSample + ":" + _emitRange + ":" + _emitDistance +
                "-L:" + sunManager.lon + ":" + sunManager.lat +
                "-M:" + shadeControl.GlobalShadeAngle + ":" + shadeControl.GlobalShadeLengthMin + ":" + shadeControl.GlobalShadeLengthMax +
                ":" + shadeControl.GlobalShadeWidth + ":" + shadeControl.ShadeNum + ":" + shadeControl.GetMinInputRange() + ":" +
                shadeControl.GetMaxInputRange() + ":" + shadeControl.ShadeLength
                ;

            try { FileOperator.WriteToCsv(data,defaultFileName); } catch { }
            

            //info example :   D:0:365:30-T:361:1081:30-S:256:2:3.5:10-L:120:32-M:-20:0.5:1.2:2.0:6:-20:40:0.2
            Debug.Log("File Save to = " + FileSavePath);
        }


    }

    private void SingleCal(out float score1,out float score2,bool preview) {
        ReleaseBuffer();
        GC();//垃圾回收

        //更新模型
        UpdateEmitter();
        _meshObjectsNeedRebuilding = true;
        RebuildMeshObjectBuffers();
        
        //传递数据
        SetShaderParameters();

        //进行运算
        Render(out texout, out score1,out score2);
        //in this case we will not use gpu score1

        var projectArea = shadeControl.GetPVProjectArea(sunManager.GetSunDir());
        score1 = projectArea / (_emitRange * _emitRange * 4);
        //Debug.Log("CPU SCORE 1 = " + score1);
        
        
        if (preview) {


            sprite = Sprite.Create(texout, new Rect(0, 0, texout.width, texout.height), new Vector2(0, 0));
            previewImage.sprite = sprite;
            
            InitCameraTexture();
            cameratex = toTexture2D(TargetTexture);
            camerasprite = Sprite.Create(cameratex, new Rect(0, 0, cameratex.width, cameratex.height), new Vector2(0, 0));
            cameraPreviewImage.sprite = camerasprite;

        }
    }
    private void GC() {
        //GC
        if (texout != null) {
            Destroy(texout);
        }
        if (sprite != null) {
            Destroy(sprite);
        }
        if (cameratex != null) {
            Destroy(cameratex);
        }
        if (camerasprite != null) {
            Destroy(camerasprite);
        }
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;

        var projection = _emitSurface.localToWorldMatrix;
        for (int i = 0; i < 10; i++) {
            for (int j = 0; j < 10; j++) {
                var u = ((float)i / 10f) * 2f - 1f;
                var v = ((float)j / 10f) * 2f - 1f;
                var vec4 = projection * new Vector4(u, 0, v, 1);
                var vec3 = new Vector3(vec4.x, vec4.y, vec4.z);
                Gizmos.DrawSphere(vec3, 0.1f);
                Gizmos.DrawLine(vec3, vec3 + _emitSurface.transform.up);
            }
        }
    }

    private void SetShaderParameters() {
        


        computeShader.SetMatrix("_EmitProjection", _emitSurface.localToWorldMatrix);
        computeShader.SetVector("_LightDirection", _directionalLight.transform.forward);
        computeShader.SetBuffer(0, "_MeshObjects", _meshObjectBuffer);



        computeShader.SetBuffer(0, "_Vertices", _vertexBuffer);
        computeShader.SetBuffer(0, "_Indices", _indexBuffer);
        computeShader.SetBuffer(0, "_Types", _typeBuffer);
        computeShader.SetInt("_MultiSample", _multiSample);

        _ScoreBuffer = new ComputeBuffer(1, sizeof(int) * 4);
        computeShader.SetBuffer(0, "_Score", _ScoreBuffer);
    }
    private void Render(out Texture2D tex,out float score1,out float score2) {
        InitRenderTexture();
        computeShader.SetTexture(0, "Result", result);
        
        computeShader.Dispatch(0, result.width / 32, result.height / 32, 1);//计算步骤
        //Graphics.Blit(result, destination);
        tex = toTexture2D(result);
        /*
        //方法1 从GPU获取score数据
        _ScoreBuffer.GetData(_score);//从compute shader 获取数据
        
        score1 = (float)_score[0] / (float)_score[3];
        score2 = (float)_score[1] / (float)_score[3];


        Debug.Log("GPU SCORE1:"+ score1 + "SCORE2:"+ score2);




        */

        //方法2 CPU计算得分
        AverageScoreFromTexture(tex,out score1,out score2);
        //Debug.Log("score2 : "+score2);

        
    }

    //private void OnRenderImage(RenderTexture source, RenderTexture destination) {

    //}

    private void InitRenderTexture() {
        if (result == null || Refresh) {
            Debug.Log("init render texture, size = " + size);
            result = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            result.enableRandomWrite = true;
            result.Create();
            
        }
    }

    private void InitCameraTexture() {
        if(TargetTexture == null || Refresh) {
            Debug.Log("init camera texture, size = " + size);
            TargetTexture = new RenderTexture(540, 540, 0);
            TargetTexture.enableRandomWrite = true;
            TargetTexture.Create();
            camera.targetTexture = TargetTexture;

        }
    }



    private void OnDisable() {
        ReleaseBuffer();
    }
    private void ReleaseBuffer() {
        if (_meshObjectBuffer != null)
            _meshObjectBuffer.Release();
        if (_ScoreBuffer != null)
            _ScoreBuffer.Release();

        if (_vertexBuffer != null)
            _vertexBuffer.Release();
        if (_indexBuffer != null)
            _indexBuffer.Release();
        if (_typeBuffer != null)
            _typeBuffer.Release();
        //if (_ScoreBuffer != null)
        //    _ScoreBuffer.Release();
    }

    public void RegisterObject(RayTracingObject obj) {
        _rayTracingObjects.Add(obj);
        _meshObjectsNeedRebuilding = true;
    }
    public void UnregisterObject(RayTracingObject obj) {
        _rayTracingObjects.Remove(obj);
        _meshObjectsNeedRebuilding = true;
    }
    private void RebuildMeshObjectBuffers() {
        //Debug.Log("Start Rebuild");
        if (!_meshObjectsNeedRebuilding) {
            return;
        }
        _meshObjectsNeedRebuilding = false;
        //_currentSample = 0;
        // Clear all lists
        _meshObjects.Clear();
        _vertices.Clear();
        _indices.Clear();
        _types.Clear();
        
        // Loop over all objects and gather their data
        foreach (RayTracingObject obj in _rayTracingObjects) {
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            // Add vertex data
            int firstVertex = _vertices.Count;
            _vertices.AddRange(mesh.vertices);
            // Add index data - if the vertex buffer wasn't empty before, the
            // indices need to be offset
            int firstIndex = _indices.Count;
            var indices = mesh.GetIndices(0);
            _indices.AddRange(indices.Select(index => index + firstVertex));
            // Add the object itself
            _meshObjects.Add(new MeshObject() {
                localToWorldMatrix = obj.transform.localToWorldMatrix,
                indices_offset = firstIndex,
                indices_count = indices.Length,
                
            }) ;
            _types.Add(obj.type);
            //Debug.Log(obj.type);
            
        }
        //Debug.Log("_meshObjects.Count"+_meshObjects.Count);
        _meshObjectBuffer = new ComputeBuffer(_meshObjects.Count, 72);
        _meshObjectBuffer.SetData(_meshObjects);

        _vertexBuffer = new ComputeBuffer(_vertices.Count, 12);
        _vertexBuffer.SetData(_vertices);

        _indexBuffer = new ComputeBuffer(_indices.Count, 4);
        _indexBuffer.SetData(_indices);
        //Debug.Log("_types.Count"+_types.Count);
        _typeBuffer = new ComputeBuffer(_types.Count, 4);
        _typeBuffer.SetData(_types);

    }

    private void UpdateEmitter() {
        var sunDir = SunManager.instance.GetSunDir();
        var pos = -sunDir * _emitDistance;

        _emitSurface.position = pos;
        _emitSurface.up = sunDir;
        _emitSurface.localScale = new Vector3(_emitRange, _emitRange, _emitRange);

    }

    Color32 AverageColorFromTexture(Texture2D tex) {

        Color32[] texColors = tex.GetPixels32();

        int total = texColors.Length;

        float r = 0;
        float g = 0;
        float b = 0;

        for (int i = 0; i < total; i++) {

            r += texColors[i].r;

            g += texColors[i].g;

            b += texColors[i].b;

        }

        return new Color32((byte)(r / total), (byte)(g / total), (byte)(b / total), 0);

    }

    void AverageScoreFromTexture(Texture2D tex,out float score1,out float score2) {

        //Color32[] texColors = tex.GetPixels32();
        Color[] texColors  = tex.GetPixels();

        int total = texColors.Length;

        
        //float r = 0;
        float g = 0;

        for (int i = 0; i < total; i++) {

            //r += texColors[i].r;
            g += texColors[i].g;
        }

        //score1 = r / total;
        score1 = 0;
        score2 = g / total;

        //return r/total;

    }

    Texture2D toTexture2D(RenderTexture rTex) {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    private float GetDirectHav(int dayIndex, int minuteIndex) {
        float index = (dayIndex * 24f + minuteIndex / 60f + DirectHav.Count) % DirectHav.Count;
        float index_f = Mathf.Floor(index);
        float index_c = Mathf.Ceil(index);
        if(DirectHav.Count <= (int)index_c) {
            Debug.LogWarning("direct hav list number not match");
            return 0;
        }
        return DirectHav[(int)(index_f)] * Mathf.Abs(index - index_f) + DirectHav[(int)(index_c)] * Mathf.Abs(index - index_c);
    }

    private float GetAvgDirectHav(int dayIndex,int minuteIndex,int dayStep) {
        float SumDirectHav = 0;
        int count = 0;
        for (int i = dayIndex - dayStep/2; i <= dayIndex+dayStep / 2; i++) {
            SumDirectHav += DirectHav[(i*24+ minuteIndex/60  + DirectHav.Count) % DirectHav.Count];
            count++;
        }

        return SumDirectHav / count;
    }
    private bool ReadBool(string name) {
        return PlayerPrefs.GetInt(name) == 1 ? true : false;
    }

    public float GetRestTime(float currentProgress) {
        var currentTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        var calTime = ((currentTime - startTime)/currentProgress)/1000;//单位为s
        var restTime = calTime * (1 - currentProgress);
        return restTime;
    }
}
