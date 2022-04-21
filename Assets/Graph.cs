using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

public class Graph : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private Font font;
    [SerializeField] private TMP_Dropdown sourceGraphDropdown;
    [SerializeField] private TMP_Dropdown graphTypeDropdown;
    private RectTransform graphContainer;

    private List<GameObject> Dots = new List<GameObject>();
    private List<GameObject> Lines = new List<GameObject>();
    private List<GameObject> Lables = new List<GameObject>();
    private List<GameObject> Grids = new List<GameObject>();
    private List<GameObject> Bars = new List<GameObject>();
    
    private float XMin, XMax,YMin,YMax;
    private int Xdiv, Ydiv;
    private float XSize, YSize;
    private float BarWidth;
    private int div;
    private int count;

    private List<Color> ColorList = new List<Color>();
    public bool FirstTimeInADay = true;
    public int acceptGraphType;
    public int graphSource;

    private List<string> FilePathList = new List<string>();
    private List<string> FileNameList = new List<string>();
    private List<Data> DataList = new List<Data>();

    [HideInInspector] public static List<Graph> instanceList;
    private void Awake() {
        
        if(Graph.instanceList == null) {
            Graph.instanceList = new List<Graph>();
        }
        Graph.instanceList.Add(this);
    }

    
    // Start is called before the first frame update
    void Start()
    {
        graphContainer = GetComponent<RectTransform>();
        ColorList.Add(new Color(1, 1, 1));//确保不会没有颜色
        FilePathList.Add("");
        DataList.Add(null);
    }

    // Update is called once per frame
    void Update()
    {
        XSize = GetComponent<RectTransform>().rect.width;
        YSize = GetComponent<RectTransform>().rect.height;
        
    }
    
    private void CreateCircle(Vector2 anchoredPos,int dataGroup,bool addConnection) {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        Color color = ColorList[dataGroup % ColorList.Count];
        gameObject.GetComponent<Image>().color = color;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        Dots.Add(gameObject);
        if(Dots.Count >= 2 && addConnection) {
            CreateDotConnection(Dots[Dots.Count - 1].GetComponent<RectTransform>().anchoredPosition, Dots[Dots.Count - 2].GetComponent<RectTransform>().anchoredPosition, color);
        }

    }

    private void CreateBar(Vector2 pos,int dataGroup,float alpha) {
        GameObject gameObject = new GameObject("Bar", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        var color = ColorList[dataGroup % ColorList.Count] * alpha;
        gameObject.GetComponent<Image>().color = color;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        if(this.count <= dataGroup) {
            this.count = dataGroup + 1;
            //redraw
        }

        var barWidth =  XSize / Xdiv / count;
        Debug.Log("COUNT = " + count + "BAR WIDTH = " + barWidth);
        rectTransform.anchoredPosition = new Vector2(pos.x + (dataGroup - count/2)* barWidth, pos.y / 2f);

        barWidth = barWidth > 1 ? barWidth : 1;
        rectTransform.sizeDelta = new Vector2(barWidth, pos.y);
        Bars.Add(gameObject);


    }

    private void CreateDotConnection(Vector2 a,Vector2 b,Color color) {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        float distance = Vector2.Distance(a, b);
        Vector2 dir = (b - a).normalized;


        rectTransform.sizeDelta = new Vector2(distance, 2);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        rectTransform.anchoredPosition = a + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, (Mathf.Atan2(dir.y, dir.x) * 180 / Mathf.PI));
        Lines.Add(gameObject);

    }

    private void CreateLable(Vector2 pos,string content) {
        
        GameObject gameObject = new GameObject("Lable", typeof(Text));
        gameObject.transform.SetParent(graphContainer, false);
        var text = gameObject.GetComponent<Text>();
        text.color = new Color(1,1,1);
        text.font = font;
        text.text = content;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 14;
        var rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchoredPosition = pos;

        Lables.Add(gameObject);

    }

    private void CreateGrid(Vector2 a, Vector2 b) {

        GameObject gameObject = new GameObject("grid", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1f,1f,1f,0.1f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        float distance = Vector2.Distance(a, b);
        Vector2 dir = (b - a).normalized;

        rectTransform.sizeDelta = new Vector2(distance, 2);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        rectTransform.anchoredPosition = a + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, (Mathf.Atan2(dir.y, dir.x) * 180 / Mathf.PI));
        Grids.Add(gameObject);

    }
    

    public void SetXRange(float min, float max,int div) {
        if(div > 12) {
            div /= 2;
        }
        if(div %2 != 0) {
            div -= 1;
        }

        if(min < max) {
            this.XMin = min;
            this.XMax = max;
        } else {
            this.XMin = max;
            this.XMax = min;
        }
        foreach(var lable in Lables) {
            Destroy(lable);
        }
        foreach(var grid in Grids) {
            Destroy(grid);
        }
        Lables.Clear();
        Grids.Clear();

        var step = (XMax - XMin) / div;
        for (int i = 0; i < div + 1; i++) {
            var xPos = (float)(i + 0) / (float)div * XSize;
            CreateLable(new Vector2(xPos, -20),(XMin + (i +0) * step).ToString("0.0"));
            CreateGrid(new Vector2(xPos, 0), new Vector2(xPos, YSize));
        }
        this.Xdiv = div;

    }

    
    public void SetYRange(float min, float max,int div) {
        if(min < max) {
            this.YMin = min;
            this.YMax = max;
        } else {
            this.YMin = max;
            this.YMax = min;
        }

        var step = (YMax - YMin) / div;
        for (int i = 0; i < div + 1; i++) {
            var yPos = (float)(i + 0) / (float)div * YSize;
            CreateLable(new Vector2(-20, yPos), (YMin + (i + 0) * step).ToString("0.0"));
            CreateGrid(new Vector2(0, yPos), new Vector2(XSize, yPos));

        }
        this.Ydiv = div;

    }

    public void GenerateColor(int count) {
        if(ColorList.Count > count) {
            return;
        }
        int gap = count - ColorList.Count;
        for (int i = 0; i < gap; i++) {
            ColorList.Add(new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f)));
        }
        this.count = ColorList.Count;
    }

    /// <summary>
    /// copy the color pool of another graph
    /// </summary>
    /// <param name="graph"></param>
    public void GetColorFromGraph(Graph graph) {
        this.ColorList = graph.ColorList;
        this.count = ColorList.Count;
    }


    public void ClearGraph() {
        foreach(var dot in Dots) {
            Destroy(dot);

        }

        foreach(var line in Lines) {
            Destroy(line);
        }

        foreach(var bar in Bars) {
            Destroy(bar);
        }

        Dots.Clear();
        Lines.Clear();
        Bars.Clear();
    }

    public void AddData(float XValue,float YValue,int dataGroup,bool addConnection) {
        var x = (XValue - XMin) / (XMax - XMin) * XSize;

        var y = (YValue - YMin) / (YMax - YMin) * YSize;
        CreateCircle(new Vector2(x, y), dataGroup, addConnection);

        
    }
    public void AddBar(float XValue, float YValue, int dataGroup) {

        var x = (XValue - XMin) / (XMax - XMin) * XSize;
        var y = (YValue - YMin) / (YMax - YMin) * YSize;
        CreateBar(new Vector2(x, y), dataGroup, 1);



    }
    public void AddBar(float XValue, float YValue, int dataGroup,float alpha) {
        
        var x = (XValue - XMin) / (XMax - XMin) * XSize;
        var y = (YValue - YMin) / (YMax - YMin) * YSize;
        CreateBar(new Vector2(x, y), dataGroup, alpha);




    }

    /// <summary>
    /// 更改图表类型
    /// </summary>
    /// <param name="change">drop down组件</param>
    public void ChangeAcceptGraphType(TMP_Dropdown change) {
        this.acceptGraphType = change.value;
        try {
            LoadFromData(DataList[sourceGraphDropdown.value], true, false, false);
        } catch { }
        
    }

    /// <summary>
    /// 更改图表来源
    /// </summary>
    /// <param name="change">drop down组件</param>
    public void ChangeSourceGraph(TMP_Dropdown change) {
        var valueIndex = change.value;
        this.graphSource = change.value;
        Debug.Log("graphSource changed to" + this.graphSource);
        

        string fileName;
        string path = "";
        if(change.value == change.options.Count - 1) {
            var extensions = new[] {
            new ExtensionFilter("Csv File", "csv" ),
            new ExtensionFilter("All Files", "*" ),
            };
            var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
            
            path = paths[0];
            var fileNames = path.Split("\\");
            fileName = fileNames[fileNames.Length - 1];
            fileName = fileName.Split(".")[0];

            FilePathList.Add(path);
            FileNameList.Add(fileName);

            Debug.Log("fileName:"+fileName);


            change.ClearOptions();
            change.AddOptions(new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Internal")});
            foreach(var name in FileNameList) {
                change.AddOptions(new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData(name) });
            }
            change.AddOptions(new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("From File...") });

            
            Debug.Log(path);
            var Data = FileOperator.ReadCsv(path);
            this.LoadFromData(Data, true, false, false);

        } else if(change.value != 0) {
            path = FilePathList[change.value];
            Debug.Log(path);
            var Data = FileOperator.ReadCsv(path);
            this.LoadFromData(Data, true, false, false);
            DataList.Add(Data);
        } else {
            ClearGraph();
        }
        change.value = valueIndex;



    }

    public void LoadFromData(Data data,bool clearGraph,bool keepOrgXRange,bool keepOrgYRange) {

        if (clearGraph) {
            this.ClearGraph();
        }

        //计算dataGroup
        int dataGroup = 0;
        for (int i = 0; i < data.minuteIndexList.Count; i++) {

            var currentDate = data.dateIndexList[i];
            if (i != 0) {
                var lastDate = data.dateIndexList[i - 1];
                if (currentDate != lastDate) {
                    dataGroup++;
                }
            }
        }


        GenerateColor(dataGroup);
        
        
        if (!keepOrgXRange) {
            List<int> newMinuteList = new List<int>(data.minuteIndexList.ToArray());
            newMinuteList.Sort();
            var min = newMinuteList[0];
            var max = newMinuteList[newMinuteList.Count - 1];
            int DefaultXdiv = 0;
            for (int i = 1; i < data.minuteIndexList.Count; i++) {
                if(data.minuteIndexList[i] != data.minuteIndexList[i - 1]) {
                    DefaultXdiv++;
                } else {
                    break;
                }
            }
            this.SetXRange(min, max, DefaultXdiv);
            if (keepOrgYRange) {
                this.SetYRange(this.YMin, this.YMax, this.Ydiv == 0 ? 10 : this.Ydiv);
            }
            
            
        }
        List<float> value;
        if(acceptGraphType == 0) {
            //best input var 
            value = data.bestInputVarList;
        }else if(acceptGraphType == 1) {
            //best score
            value = data.bestTotalScoreList;
        } else if(acceptGraphType == 2) {
            //best value
            value = data.bestTotalValueList;
        }else if(acceptGraphType == 3) {
            value = data.bestInputVarList2;
        } else {
            value = new List<float>();
        }

        if (!keepOrgYRange) {
            List<float> newValueList = new List<float>(value.ToArray());
            newValueList.Sort();
            var min = newValueList[0];
            var max = newValueList[newValueList.Count - 1];
            var gap = max - min;
            if (acceptGraphType == 1 || acceptGraphType == 2) {
                //pass
                
            } else{
                //不是score 或value
                min -= 0.2f * gap;
            }
            max += 0.2f * gap;
            if (keepOrgXRange) {
                this.SetXRange(this.XMin, this.XMax, this.Xdiv == 0? 10:this.Xdiv);
            }

            this.SetYRange(min, max, 10);
        }

        if(acceptGraphType == 0) {
            //best input var 
            dataGroup = 0;
            for (int i = 0; i <data.minuteIndexList.Count; i++) {
                var currentDate = data.dateIndexList[i];
                var currentMinute = data.minuteIndexList[i];
                bool addConnection = true;
                if (i != 0) {
                    var lastDate = data.dateIndexList[i - 1];
                    if(currentDate != lastDate ) {
                        dataGroup++;
                    }
                    addConnection = currentDate == lastDate && data.needToCalList[i-1] == true;
                }
                if (data.needToCalList[i] == true) {
                    this.AddData(currentMinute, data.bestInputVarList[i], dataGroup, addConnection);
                }
                    
            }
        }else if(acceptGraphType == 1) {
            //draw best score
            dataGroup = 0;
            for (int i = 0; i < data.minuteIndexList.Count; i++) {
                bool connect = true;
                var currentDate = data.dateIndexList[i];
                var currentMinute = data.minuteIndexList[i];
                if (i != 0) {
                    var lastDate = data.dateIndexList[i - 1];
                    if (currentDate != lastDate) {
                        dataGroup++;
                    }
                    connect = currentDate == lastDate && data.needToCalList[i - 1] == true;
                }
                if (data.needToCalList[i] == true) {
                    this.AddBar(currentMinute, data.bestTotalScoreList[i], dataGroup, 0.8f);
                    this.AddBar(currentMinute, data.bestReflectScoreList[i], dataGroup);
                    this.AddData(currentMinute, data.bestPVScoreList[i], dataGroup, connect);
                }
                    

            }
        }else if(acceptGraphType == 2) {
            //draw best value
            dataGroup = 0;
            for (int i = 0; i < data.minuteIndexList.Count; i++) {
                bool connect = true;
                var currentDate = data.dateIndexList[i];
                var currentMinute = data.minuteIndexList[i];
                if (i != 0) {
                    var lastDate = data.dateIndexList[i - 1];
                    if (currentDate != lastDate) {
                        dataGroup++;
                        
                    }
                    connect = currentDate == lastDate && data.needToCalList[i - 1] == true;
                }
                if (data.needToCalList[i] == true) {
                    this.AddBar(currentMinute, data.bestTotalValueList[i], dataGroup, 0.8f);
                    this.AddBar(currentMinute, data.bestReflectValueList[i], dataGroup);
                    this.AddData(currentMinute, data.bestPVValueList[i], dataGroup, connect);
                }
                    

            }
        }else if(acceptGraphType == 3) {
            //best input var 2
            dataGroup = 0;
            for (int i = 0; i < data.minuteIndexList.Count; i++) {
                var currentDate = data.dateIndexList[i];
                var currentMinute = data.minuteIndexList[i];
                
                bool addConnection = true;
                if (i != 0) {
                    var lastDate = data.dateIndexList[i - 1];
                    if (currentDate != lastDate) {
                        dataGroup++;
                    }
                    addConnection = currentDate == lastDate && data.needToCalList[i - 1] == true;
                }

                if(data.needToCalList[i] == true) {
                    this.AddData(currentMinute, data.bestInputVarList2[i], dataGroup, addConnection);
                }
                
            }
        }


    }
    public void SetInternalData(Data data) {
        this.DataList[0] = data;
    }

}
