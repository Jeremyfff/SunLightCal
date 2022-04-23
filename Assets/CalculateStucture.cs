
using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CalculateStucture : MonoBehaviour
{
    [SerializeField] private bool DrawGizmos = true;
    [SerializeField] private bool calReflection;

    [SerializeField] private float 全局Gizmo大小 = 0.005f;
    [SerializeField] private Color 基础Gizmo颜色 = Color.white;
    [SerializeField] private Color 隐藏Gizmo颜色 = Color.grey;
    [SerializeField] private Color 电机Gizmo颜色 = Color.red;
    [SerializeField] private Color 高亮Gizmo颜色 = Color.yellow;
    [SerializeField] private Transform 垂直电机轨道Start;
    [SerializeField] private Transform 垂直电机轨道End;
    [Range(0f,1f)]
    [SerializeField] private float 垂直电机位置百分比;
    


    [SerializeField] private Transform RootPos;

    [SerializeField] private float 根部有效长度;
    [SerializeField] private float 根部连杆长度;
    [SerializeField] private bool 根部连杆翻转;
    [SerializeField] private float 根部中轴与根部电机中轴之间的偏移距离;
    [SerializeField] private bool 根部电机位置翻转;
    [SerializeField] private float 根部电机距离rootStart的距离;
    [SerializeField] private float 根部电机轨道长度;
    [Range(0.0f,1.0f)]
    [SerializeField] private float 根部电机位置百分比;
    [SerializeField] private float 根部电机连杆长度;
    [SerializeField] private bool 根部电机连杆翻转;

    [SerializeField] private float 第一指节开始位置Z偏移;
    [SerializeField] private float 第一指节开始位置Y偏移;
    //以上两个偏移都是基于root的坐标系来的，是相对于rootEnd的偏移

    [SerializeField] private float link1End半径;
    [SerializeField] private float 第一指节与link1End夹角;
    [SerializeField] private float 第一指节长度;
    [SerializeField] private float 第一指节连杆长度;

    [SerializeField] private float Link2Start半径;
    [SerializeField] private float Link2Start的角度位置;

    [SerializeField] private float link2End半径;
    [SerializeField] private float 第二指节与link2End夹角;
    [SerializeField] private float 第二指节长度;
    [SerializeField] private float 第二指节连杆长度;

    [SerializeField] private float Link3Start半径;
    [SerializeField] private float Link3Start的角度位置;

    [SerializeField] private float link3End半径;
    [SerializeField] private float 第三指节与link3End夹角;
    [SerializeField] private float 第三指节长度;
    [SerializeField] private float 第三指节连杆长度;


    [SerializeField] private float 第一指节反射板连杆偏移距离;
    [SerializeField] private float 第一指节反射板连杆长度;
    [SerializeField] private float 第一指节反射板长度;


    [SerializeField] private float 第二指节反射板连杆长度;
    [SerializeField] private float 第二指节反射板长度;

    [SerializeField] private float 第三指节反射板连杆偏移距离;
    [SerializeField] private float 第三指节反射板连杆长度;
    [SerializeField] private float 第三指节反射板长度;

    private bool buildError = false;

    [SerializeField] private Transform PV底部;
    [SerializeField] private Transform PV顶部;
    [SerializeField] private float PV宽度;

    

    [SerializeField] private bool 校准屏幕显示;
    [SerializeField] private float screenScale;
    [SerializeField] private float textPadding;

    private SunManager sunManager;
    private ArrayList infoList = new ArrayList();
    private long lastTime = 0;
    private int count = 0;
    //data
    private List<float> DirectHav;

    // Start is called before the first frame update
    void Start()
    {
        sunManager = SunManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos() {
        //Debug.Log("drawing gizmos" + count.ToString());
        //count++;
        //计算屏幕显示相关数值
        var sceneCamera = SceneView.currentDrawingSceneView.camera;
        var sceneCameraPos = sceneCamera.transform.position;
        var sceneCameraDir = sceneCamera.transform.forward;

        var screenCenter = sceneCameraPos + sceneCameraDir;
        var heightDir = sceneCamera.transform.up * sceneCamera.pixelHeight / sceneCamera.pixelWidth * screenScale;
        var widthDir = sceneCamera.transform.right  *screenScale;
        var top_left = screenCenter - widthDir / 2 + heightDir / 2;
        if (校准屏幕显示) {
            Gizmos.DrawLine(top_left, screenCenter + widthDir / 2 - heightDir / 2); 
        }

        //初始化info panel
        var commonText = "Info Panel";
        
        if(sunManager == null) {
            calReflection = false;
            commonText = commonText + "   Tip:运行后才能打开阳光计算";
        }

        infoList = new ArrayList();
        infoList.Add(commonText);

        //初始化data
        if(DirectHav == null) {
            DirectHav = FileOperator.ReadDirectHav();
        }

        buildError = false;

        
        //开始计算
        
        var motor1Pos = DrawMotor1();
        Handles.Label(motor1Pos, "motor1Pos");

        var rootEnd = RootPos.position;
        Handles.Label(RootPos.position, "rootEnd");

        var rootStart = DrawRoot(motor1Pos);
        Handles.Label(rootStart, "rootStart");

        var motor2Pos = DrawMotor2(rootStart, rootEnd);
        Handles.Label(motor2Pos, "motor2Pos");


        //更改features

        var fingle1Start = DrawFingle1Start(rootStart, rootEnd);
        Handles.Label(fingle1Start, "fingle1Start");


        var link1End =  DrawLink1(motor2Pos, fingle1Start);
        Handles.Label(link1End, "link1End");

        var fingle1Dir = DrawNode1(fingle1Start, link1End);


        var fingle1End = DrawFingle1(fingle1Start, fingle1Dir);
        Handles.Label(fingle1End, "fingle1End");

        //-------------------------------------------------

        var link2Start = DrawLink2Start(rootEnd - rootStart, fingle1Start);
        Handles.Label(link2Start, "link2Start");

        var link2End = DrawLink2(link2Start, fingle1End);
        Handles.Label(link2End, "link2End");

        var fingle2Dir = DrawNode2(fingle1End, link2End);

        var fingle2End = DrawFingle2(fingle1End, fingle2Dir);
        Handles.Label(fingle2End, "fingle2End");

        //-------------------------------------------------

        var link3Start = DrawLink3Start(fingle1End - rootEnd, fingle1End);
        Handles.Label(link3Start, "link3Start");

        var link3End = DrawLink3(link3Start, fingle2End);
        Handles.Label(link3End, "link3End");

        var fingle3Dir = DrawNode3(fingle2End, link3End);


        var fingle3End = DrawFingle3(fingle2End, fingle3Dir);
        Handles.Label(fingle3End, "fingle3End");

        //-------------------------------------------------
        //-------------------------------------------------

        var rod2End =  DrawFingle2Rod((fingle1End + fingle2End) / 2, fingle2Dir, true);
        Vector3 board2Start;
        Vector3 board2End;
        DrawReflectBoard2(rod2End, fingle2Dir, out board2Start, out board2End);

        var rod1End = DrawFingle1Rod(fingle1Start, fingle1End, board2Start, true);
        var board1Start = DrawReflectBoard1(board2Start, rod1End);

        var rod3End = DrawFingle3Rod(fingle2End,fingle3End, board2End, false);
        var board3End = DrawReflectBoard3(board2End, rod3End);
            
        //-------------------------------------------------

        //ref board
        var p_x = new Vector3(1, 0, 0);
        var n_x = new Vector3(-1, 0, 0);
        var pt_A1 = board1Start + n_x;
        var pt_A2 = board1Start + p_x;
        var pt_A3 = board2Start + p_x;
        var pt_A4 = board2Start + n_x;

        var pt_B1 = board2Start + n_x;
        var pt_B2 = board2Start + p_x;
        var pt_B3 = board2End + p_x;
        var pt_B4 = board2End + n_x;

        var pt_C1 = board2End + n_x;
        var pt_C2 = board2End + p_x;
        var pt_C3 = board3End + p_x;
        var pt_C4 = board3End + n_x;

        var board1Normal = -Vector3.Cross(pt_A2 - pt_A1, pt_A2 - pt_A3).normalized;
        var board2Normal = -Vector3.Cross(pt_B2 - pt_B1, pt_B2 - pt_B3).normalized;
        var board3Normal = -Vector3.Cross(pt_C2 - pt_C1, pt_C2 - pt_C3).normalized;
        if (DrawGizmos) {
            Gizmos.DrawLine(pt_A1, pt_A2);
            Gizmos.DrawLine(pt_A3, pt_A4);
            Gizmos.DrawLine(pt_A1, pt_A4);
            Gizmos.DrawLine(pt_A3, pt_A2);


            Gizmos.DrawLine(pt_B1, pt_B2);
            Gizmos.DrawLine(pt_B3, pt_B4);
            Gizmos.DrawLine(pt_B1, pt_B4);
            Gizmos.DrawLine(pt_B3, pt_B2);

            Gizmos.DrawLine(pt_C1, pt_C2);
            Gizmos.DrawLine(pt_C3, pt_C4);
            Gizmos.DrawLine(pt_C1, pt_C4);
            Gizmos.DrawLine(pt_C3, pt_C2);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pt_A1, pt_A1 + board1Normal * 0.1f);
            Gizmos.DrawLine(pt_B1, pt_B1 + board2Normal * 0.1f);
            Gizmos.DrawLine(pt_C1, pt_C1 + board3Normal * 0.1f);
        }

        //=============================

        var PV_Plane = DrawPV();
        if (calReflection) {
            //var sunDir = new Vector3(0.1f, -1f, 1f);
            var sunDir = sunManager.GetSunDir();
            if (DrawGizmos) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(-sunDir * 10, Vector3.zero);
            }
            //Debug.Log(sunDir);
            
            Vector3[] pts_A = { pt_A1, pt_A2, pt_A3, pt_A4 };
            var castPoints_A = DrawLightCast(sunDir, pts_A, PV_Plane);
            Vector3[] pts_B = { pt_B1, pt_B2, pt_B3, pt_B4 };
            var castPoints_B = DrawLightCast(sunDir, pts_B, PV_Plane);
            Vector3[] pts_C = { pt_C1, pt_C2, pt_C3, pt_C4 };
            var castPoints_C = DrawLightCast(sunDir, pts_C, PV_Plane);

            float area_A;float percentage_A;
            CalIntersectArea(castPoints_A, PV顶部.position, PV底部.position, out area_A, out percentage_A);
            float area_B; float percentage_B;
            CalIntersectArea(castPoints_B, PV顶部.position, PV底部.position, out area_B, out percentage_B);
            float area_C; float percentage_C;
            CalIntersectArea(castPoints_C, PV底部.position, PV底部.position, out area_C, out percentage_C);

            infoList.Add("反光板A光斑面积：" + area_A.ToString() + "， 利用率：" + percentage_A * 100 + "%");
            infoList.Add("反光板B光斑面积：" + area_B.ToString() + "， 利用率：" + percentage_B * 100 + "%");
            infoList.Add("反光板C光斑面积：" + area_C.ToString() + "， 利用率：" + percentage_C * 100 + "%");

            infoList.Add("-----------------" );

            var refBoard1CastArea = calRefBoardCastArea(sunDir, board1Normal, Vector3.Distance(pt_A1, pt_A2) * Vector3.Distance(pt_A2, pt_A3));
            var refBoard2CastArea = calRefBoardCastArea(sunDir, board2Normal, Vector3.Distance(pt_B1, pt_B2) * Vector3.Distance(pt_B2, pt_B3));
            var refBoard3CastArea = calRefBoardCastArea(sunDir, board3Normal, Vector3.Distance(pt_C1, pt_C2) * Vector3.Distance(pt_C2, pt_C3));

            infoList.Add("反光板A阳光投影面积：" + refBoard1CastArea.ToString());
            infoList.Add("反光板B阳光投影面积：" + refBoard2CastArea.ToString());
            infoList.Add("反光板C阳光投影面积：" + refBoard3CastArea.ToString());
            infoList.Add("-----------------");

            var realCastArea1 = refBoard1CastArea * percentage_A;
            var realCastArea2 = refBoard2CastArea * percentage_B;
            var realCastArea3 = refBoard3CastArea * percentage_C;

            infoList.Add("反光板A等效辐射面积：" + realCastArea1.ToString());
            infoList.Add("反光板B等效辐射面积：" + realCastArea2.ToString());
            infoList.Add("反光板C等效辐射面积：" + realCastArea3.ToString());
            infoList.Add("-----------------");

        }




            
            
        
        
        long currentTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        long fps = 1000 /(currentTime - lastTime);
        lastTime = currentTime;
        infoList.Add("FPS:" + fps);


        ShowInfoOnScreen(top_left, -textPadding * 0.1f * sceneCamera.transform.up);

        if (buildError) {
            Debug.Log("Build Error");
            
        }

    }

    private void SingleCal(out float[] realCastArea) {
        realCastArea = new float[3];
        buildError = false;
        //开始计算
        var motor1Pos = DrawMotor1();var rootEnd = RootPos.position;var rootStart = DrawRoot(motor1Pos);
        var motor2Pos = DrawMotor2(rootStart, rootEnd); var fingle1Start = DrawFingle1Start(rootStart, rootEnd);
        var link1End = DrawLink1(motor2Pos, fingle1Start);var fingle1Dir = DrawNode1(fingle1Start, link1End);
        var fingle1End = DrawFingle1(fingle1Start, fingle1Dir);var link2Start = DrawLink2Start(rootEnd - rootStart, fingle1Start);
        var link2End = DrawLink2(link2Start, fingle1End);var fingle2Dir = DrawNode2(fingle1End, link2End);
        var fingle2End = DrawFingle2(fingle1End, fingle2Dir);var link3Start = DrawLink3Start(fingle1End - rootEnd, fingle1End);
        var link3End = DrawLink3(link3Start, fingle2End); var fingle3Dir = DrawNode3(fingle2End, link3End);
        var fingle3End = DrawFingle3(fingle2End, fingle3Dir);var rod2End = DrawFingle2Rod((fingle1End + fingle2End) / 2, fingle2Dir, true);
        Vector3 board2Start;Vector3 board2End;DrawReflectBoard2(rod2End, fingle2Dir, out board2Start, out board2End);
        var rod1End = DrawFingle1Rod(fingle1Start, fingle1End, board2Start, true); var board1Start = DrawReflectBoard1(board2Start, rod1End);
        var rod3End = DrawFingle3Rod(fingle2End, fingle3End, board2End, false);var board3End = DrawReflectBoard3(board2End, rod3End);
        //-------------------------------------------------
        //ref board
        var p_x = new Vector3(1, 0, 0);var n_x = new Vector3(-1, 0, 0);
        var pt_A1 = board1Start + n_x;var pt_A2 = board1Start + p_x;var pt_A3 = board2Start + p_x;var pt_A4 = board2Start + n_x;
        var pt_B1 = board2Start + n_x;var pt_B2 = board2Start + p_x;var pt_B3 = board2End + p_x;var pt_B4 = board2End + n_x;
        var pt_C1 = board2End + n_x;var pt_C2 = board2End + p_x;var pt_C3 = board3End + p_x;var pt_C4 = board3End + n_x;

        var board1Normal = -Vector3.Cross(pt_A2 - pt_A1, pt_A2 - pt_A3).normalized;
        var board2Normal = -Vector3.Cross(pt_B2 - pt_B1, pt_B2 - pt_B3).normalized;
        var board3Normal = -Vector3.Cross(pt_C2 - pt_C1, pt_C2 - pt_C3).normalized;

        //=============================

        var PV_Plane = DrawPV();
        var sunDir = sunManager.GetSunDir();
        Vector3[] pts_A = { pt_A1, pt_A2, pt_A3, pt_A4 };var castPoints_A = DrawLightCast(sunDir, pts_A, PV_Plane);
        Vector3[] pts_B = { pt_B1, pt_B2, pt_B3, pt_B4 }; var castPoints_B = DrawLightCast(sunDir, pts_B, PV_Plane);
        Vector3[] pts_C = { pt_C1, pt_C2, pt_C3, pt_C4 };var castPoints_C = DrawLightCast(sunDir, pts_C, PV_Plane);

        float area_A; float percentage_A;
        CalIntersectArea(castPoints_A, PV顶部.position, PV底部.position, out area_A, out percentage_A);
        float area_B; float percentage_B;
        CalIntersectArea(castPoints_B, PV顶部.position, PV底部.position, out area_B, out percentage_B);
        float area_C; float percentage_C;
        CalIntersectArea(castPoints_C, PV底部.position, PV底部.position, out area_C, out percentage_C);


        var refBoard1CastArea = calRefBoardCastArea(sunDir, board1Normal, Vector3.Distance(pt_A1, pt_A2) * Vector3.Distance(pt_A2, pt_A3));
        var refBoard2CastArea = calRefBoardCastArea(sunDir, board2Normal, Vector3.Distance(pt_B1, pt_B2) * Vector3.Distance(pt_B2, pt_B3));
        var refBoard3CastArea = calRefBoardCastArea(sunDir, board3Normal, Vector3.Distance(pt_C1, pt_C2) * Vector3.Distance(pt_C2, pt_C3));


        realCastArea[0] = refBoard1CastArea * percentage_A;
        realCastArea[1] = refBoard2CastArea * percentage_B;
        realCastArea[2] = refBoard3CastArea * percentage_C;

    }

    //----------------------------------------------------------------------

    private Vector3 DrawMotor1() {

        var motorPos = Vector3.Lerp(垂直电机轨道Start.position, 垂直电机轨道End.position, 垂直电机位置百分比);
        if (DrawGizmos) {
            Gizmos.color = 隐藏Gizmo颜色;
            Gizmos.DrawLine(垂直电机轨道Start.position, 垂直电机轨道End.position);
            Gizmos.color = 电机Gizmo颜色;
            Gizmos.DrawSphere(motorPos, 全局Gizmo大小);
        }

        return motorPos;
    }

    private Vector3 DrawRoot(Vector3 Motor1Pos) {
        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawLine(RootPos.position + Vector3.left * 10, RootPos.position + Vector3.right * 10);
            Gizmos.DrawSphere(RootPos.position, 全局Gizmo大小);
        }

        var point1 = Motor1Pos;
        var point2 = RootPos.position;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, 根部连杆长度, 根部有效长度, 根部连杆翻转);

        if (DrawGizmos) {
            Gizmos.color = 高亮Gizmo颜色;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, 全局Gizmo大小);
        }

        return point3;
    }

    private Vector3 DrawMotor2(Vector3 rootStart,Vector3 rootEnd) {
        Vector3 rootDir = rootEnd - rootStart;
        Vector3 shiftDir = GetVertical(rootDir, 根部电机位置翻转);
        shiftDir = shiftDir.normalized * 根部中轴与根部电机中轴之间的偏移距离;
        
        var motor2Start = rootStart + shiftDir + rootDir.normalized * 根部电机距离rootStart的距离;
        var motor2End = motor2Start + rootDir.normalized * 根部电机轨道长度;

        if (DrawGizmos) {
            Gizmos.color = 隐藏Gizmo颜色;
            Gizmos.DrawLine(motor2Start, motor2End);//电机可移动范围的line
        }

        var motor2Pos = Vector3.Lerp(motor2Start, motor2End, 根部电机位置百分比);

        if (DrawGizmos) {
            Gizmos.color = 电机Gizmo颜色;
            Gizmos.DrawSphere(motor2Pos, 全局Gizmo大小);
        }


        return motor2Pos;
    }


    private Vector3 DrawFingle1Start(Vector3 rootStart,Vector3 rootEnd) {
        var Zdir = (rootEnd - rootStart).normalized;
        var Ydir = GetVertical(Zdir, false).normalized;
        Zdir *= 第一指节开始位置Z偏移;
        Ydir *= 第一指节开始位置Y偏移;

        var result = rootEnd + Zdir + Ydir;
        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawSphere(result, 全局Gizmo大小);
            Gizmos.DrawLine(result, rootEnd);
        }
        return result;

    }


    private Vector3 DrawLink1(Vector3 motor2Pos,Vector3 rootEnd) {
        var point1 = motor2Pos;
        var point2 = rootEnd;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, 根部电机连杆长度, link1End半径, 根部电机连杆翻转);

        if (DrawGizmos) {
            Gizmos.color = 高亮Gizmo颜色;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, 全局Gizmo大小);
        }


        return point3;
    }

    private Vector3 DrawNode1(Vector3 center,Vector3 link1End) {
        if (DrawGizmos) {
            Gizmos.color = 隐藏Gizmo颜色;
            Gizmos.DrawWireSphere(center, Link2Start半径);
        }


        var dir = link1End - center;
        dir = dir.normalized;
        var cosA = dir.z;
        var sinA = dir.y;
        var dA = Mathf.Acos(cosA);
       // Debug.Log(dA);
        
        if(sinA < 0) {
            dA = -dA;
        }

        var dB = 第一指节与link1End夹角 * Mathf.PI / 180f + dA;

        var newDir = new Vector3(0, Mathf.Sin(dB), Mathf.Cos(dB));
        return newDir;
    }

    private Vector3 DrawLink2Start(Vector3 rootDir,Vector3 center) {
        var dir = rootDir.normalized;
        var cosA = dir.z;
        var sinA = dir.y;
        var dA = Mathf.Acos(cosA);
        // Debug.Log(dA);

        if (sinA < 0) {
            dA = -dA;
        }
        var dB = Link2Start的角度位置 * Mathf.PI / 180f + dA;

        var newDir = new Vector3(0, Mathf.Sin(dB), Mathf.Cos(dB));

        var result = center + newDir * Link2Start半径;

        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawSphere(result, 全局Gizmo大小);
        }


        return result;

    }

    private Vector3 DrawFingle1(Vector3 start,Vector3 dir) {
        var end = start + dir * 第一指节长度;

        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawLine(start, end);
        }

        return end;
    }

    private Vector3 DrawLink2(Vector3 link2Start,Vector3 fingle1End) {
        var point1 = link2Start;
        var point2 = fingle1End;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, 第一指节连杆长度, link2End半径, false);

        if (DrawGizmos) {
            Gizmos.color = 高亮Gizmo颜色;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, 全局Gizmo大小);
        }


        return point3;
    }

    private Vector3 DrawNode2(Vector3 center, Vector3 link2End) {
        if (DrawGizmos) {
            Gizmos.color = 隐藏Gizmo颜色;
            Gizmos.DrawWireSphere(center, Link3Start半径);
        }


        var dir = link2End - center;
        dir = dir.normalized;
        var cosA = dir.z;
        var sinA = dir.y;
        var dA = Mathf.Acos(cosA);
        // Debug.Log(dA);

        if (sinA < 0) {
            dA = -dA;
        }

        var dB = 第二指节与link2End夹角 * Mathf.PI / 180f + dA;

        var newDir = new Vector3(0, Mathf.Sin(dB), Mathf.Cos(dB));
        return newDir;
    }

    private Vector3 DrawLink3Start(Vector3 fingle1Dir, Vector3 center) {
        var dir = fingle1Dir.normalized;
        var cosA = dir.z;
        var sinA = dir.y;
        var dA = Mathf.Acos(cosA);
        // Debug.Log(dA);

        if (sinA < 0) {
            dA = -dA;
        }
        var dB = Link3Start的角度位置 * Mathf.PI / 180f + dA;

        var newDir = new Vector3(0, Mathf.Sin(dB), Mathf.Cos(dB));

        var result = center + newDir * Link3Start半径;

        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawSphere(result, 全局Gizmo大小);
        }


        return result;

    }

    private Vector3 DrawFingle2(Vector3 start, Vector3 dir) {
        var end = start + dir * 第二指节长度;

        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawLine(start, end);
        }


        return end;
    }

    private Vector3 DrawLink3(Vector3 link3Start, Vector3 fingle2End) {
        var point1 = link3Start;
        var point2 = fingle2End;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, 第二指节连杆长度, link3End半径, false);

        if (DrawGizmos) {
            Gizmos.color = 高亮Gizmo颜色;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, 全局Gizmo大小);
        }


        return point3;
    }

    private Vector3 DrawNode3(Vector3 center, Vector3 link3End) {
        if (DrawGizmos) {
            Gizmos.color = 隐藏Gizmo颜色;
            Gizmos.DrawWireSphere(center, link3End半径);
        }


        var dir = link3End - center;
        dir = dir.normalized;
        var cosA = dir.z;
        var sinA = dir.y;
        var dA = Mathf.Acos(cosA);
        // Debug.Log(dA);

        if (sinA < 0) {
            dA = -dA;
        }

        var dB = 第三指节与link3End夹角 * Mathf.PI / 180f + dA;

        var newDir = new Vector3(0, Mathf.Sin(dB), Mathf.Cos(dB));
        return newDir;
    }

    private Vector3 DrawFingle3(Vector3 start, Vector3 dir) {
        var end = start + dir * 第三指节长度;
        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawLine(start, end);
        }

        return end;
    }


    //----------------------------------------------------------------------

    private Vector3 DrawFingle2Rod(Vector3 fingle2Center,Vector3 fingle2Dir,bool flip) {
        var rodDir = GetVertical(fingle2Dir, flip).normalized * 第二指节反射板连杆长度;
        var rodEnd = fingle2Center + rodDir;

        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawLine(fingle2Center, rodEnd);
            Gizmos.DrawSphere(rodEnd, 全局Gizmo大小);
        }

        return fingle2Center + rodDir;

    }

    private void DrawReflectBoard2(Vector3 rod2End, Vector3 fingle2Dir,out Vector3 boardStart,out Vector3 boardEnd) {
        var halfDir = fingle2Dir.normalized * 第二指节反射板长度 * 0.5f;
        boardStart = rod2End - halfDir;
        boardEnd = rod2End + halfDir;

        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawLine(boardStart, boardEnd);
        }

    }

    private Vector3 DrawFingle1Rod(Vector3 fingle1Start,Vector3 fingle1End,Vector3 board2Start,bool flip) {
        var center = (fingle1Start + fingle1End)/ 2f;
        var shiftDir = GetVertical((fingle1End - fingle1Start), true).normalized * 第一指节反射板连杆偏移距离;

        var point1 = center + shiftDir;
        var point2 = board2Start;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, 第一指节反射板连杆长度, 第一指节反射板长度/2f, flip);

        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawSphere(point1, 全局Gizmo大小);
            Gizmos.DrawLine(point1, center);
            Gizmos.color = 高亮Gizmo颜色;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, 全局Gizmo大小);
        }


        return point3;//返回rod1end

    }
    private Vector3 DrawReflectBoard1(Vector3 board2Start,Vector3 rod1End) {
        var dir = (rod1End - board2Start).normalized * 第一指节反射板长度;
        var board1Start = board2Start + dir;

        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawLine(board1Start, board2Start);
        }


        return board1Start;

    }


    private Vector3 DrawFingle3Rod(Vector3 fingle3Start,Vector3 fingle3End,Vector3 board2End,bool flip) {
        var center = (fingle3Start + fingle3End) / 2f;
        var shiftDir = GetVertical((fingle3End - fingle3Start), true).normalized * 第三指节反射板连杆偏移距离;

        var point1 = center + shiftDir;
        var point2 = board2End;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, 第三指节反射板连杆长度, 第三指节反射板长度 / 2f, flip);

        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawSphere(point1, 全局Gizmo大小);
            Gizmos.DrawLine(point1, center);
            Gizmos.color = 高亮Gizmo颜色;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, 全局Gizmo大小);
        }


        return point3;//返回rod3end
    }

    private Vector3 DrawReflectBoard3(Vector3 board2End,Vector3 rod3End) {
        var dir = (rod3End - board2End).normalized * 第三指节反射板长度;
        var board3End = board2End + dir;

        if (DrawGizmos) {
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawLine(board2End, board3End);
        }

        return board3End;
    }


    //----------------------------------------------------------------------


    private NGlbPlane DrawPV() {
        var top = PV顶部.position;
        var bot = PV底部.position;
        var p_x = new Vector3(PV宽度/2f, 0, 0);
        var pt_P1 = bot - p_x;
        var pt_P2 = bot + p_x;
        var pt_P3 = top + p_x;
        var pt_P4 = top - p_x;

        if (DrawGizmos) {
            
            Gizmos.color = 隐藏Gizmo颜色;
            Gizmos.DrawLine(bot, top);
            Gizmos.color = 基础Gizmo颜色;
            Gizmos.DrawLine(pt_P1, pt_P2);
            Gizmos.DrawLine(pt_P3, pt_P4);
            Gizmos.DrawLine(pt_P1, pt_P4);
            Gizmos.DrawLine(pt_P3, pt_P2);
        }
        var pt1 = new NGlbVec3d(pt_P1);
        var pt2 = new NGlbVec3d(pt_P2);
        var pt3 = new NGlbVec3d(pt_P3);
        var plane = new NGlbPlane(pt1, pt2, pt3);
        return plane;
    }

    private Vector3[] DrawLightCast(Vector3 SunLightDir, Vector3[] pts, NGlbPlane targetPlane) {
        if(pts.Length != 4) {
            return null;
        }
        var normal = GetNormal(pts[0] - pts[1], pts[1] - pts[2], false);
        var reflectDir = Reflect(SunLightDir, normal) * -1000f;

        if (DrawGizmos) {
            Gizmos.color = 隐藏Gizmo颜色;
            for (int i = 0; i < 4; i++) {
                Gizmos.DrawRay(new Ray(pts[i], reflectDir));
            }
        }

        Vector3[] result = new Vector3[4];
        for (int i = 0; i < 4; i++) {
            NGlbVec3d[] ln = { new NGlbVec3d(pts[i]), new NGlbVec3d(pts[i]+reflectDir) };
            NGlbVec3d hit;
            var intersect =IsLineInterPlane(ln, targetPlane,out hit);
            result[i] = hit.toVector3();
            if (!intersect) {
                buildError = true;
            }
        }

        //display
        if (!buildError && DrawGizmos) {
            Gizmos.color = 高亮Gizmo颜色;
            Gizmos.DrawLine(result[0], result[1]);
            Gizmos.DrawLine(result[1], result[2]);
            Gizmos.DrawLine(result[2], result[3]);
            Gizmos.DrawLine(result[3], result[0]);

        }

        return result;
    }

    private void CalIntersectArea(Vector3[] castPoints,Vector3 PV_top,Vector3 PV_bot,out float area,out float percentage) {
        var orgDist = Vector3.Distance(castPoints[0],castPoints[3]);
        var y2dis = Vector3.Distance(PV_top, PV_bot) / Mathf.Abs(PV_top.y - PV_bot.y);

        area = 0f;
        percentage = 0f;

        if(castPoints[0].y > PV_top.y) {
            return;
        }
        if(castPoints[3].y < PV_bot.y)
        {
            return;
        }
        var bot_y = PV_bot.y;
        if(castPoints[0].y > PV_bot.y)
        {
            bot_y = castPoints[0].y;
        }
        var top_y = PV_top.y;
        if (castPoints[3].y < PV_top.y)
        {
            top_y = castPoints[3].y;
        }

        var castYDist = Mathf.Abs(top_y - bot_y) * y2dis;
        var castXDist = Mathf.Abs(castPoints[0].x - castPoints[1].x);

        percentage = castYDist / orgDist;
        area = castYDist * castXDist;
        return;
    }

    private float calRefBoardCastArea(Vector3 sunDir,Vector3 refBoardNormal,float refBoardArea) {
        if(Vector3.Dot(sunDir,refBoardNormal) > 0) {
            return 0f;
        }
        float angleInD  = Vector3.Angle(-sunDir, refBoardNormal);
        float angleInR = angleInD / 180f * Mathf.PI;

        var result = refBoardArea * Mathf.Cos(angleInR);
        return result;

    }
    


    //----------------------------------------------------------------------

    private float GetDAngleOfAFromLength(float a,float b,float c) {
        var cosA = (b * b + c * c - a * a) / (2 * b * c);
        float dAngle = 180f / Mathf.PI * Mathf.Acos(cosA);
        if(cosA > 1 || cosA < -1) {
            this.buildError = true;
        }
        return dAngle;
    }

    private Vector3 CalculatePointFromPointsAndLength(Vector3 point1,Vector3 point2,float length1,float length2,bool direction) {
        var b = length2;
        var a = length1;
        var c = Vector3.Distance(point1, point2);

        var dA = GetDAngleOfAFromLength(a, b, c);



        if (!direction) {
            dA = -dA;
        }
        

        //Debug.Log("dA:" + dA);
        Vector3 AB = point1 - point2;
        var a2 = Mathf.Abs(AB.y);
        var b2 = Mathf.Abs(AB.z);
        var c2 = c;
        var dAB = GetDAngleOfAFromLength(a2, b2, c2);
        
        //判断P1 P2的位置关系，更改dAC的符号和大小
        if (point1.z > point2.z) {
            //如果1在2的右方
            if (point1.y > point2.y) {
                //如果1在2的上方
                dAB = dAB;
            } else {
                //如果1在2的下方
                dAB = -dAB;
            }
        } else {
            //如果1在2的左方
            if (point1.y > point2.y) {
                //如果1在2的上方
                dAB = 180-dAB;
            } else {
                //如果1在2的下方
                dAB = 180+dAB;
            }
        }
        

        var dAC = Mathf.PI * (dAB + dA) / 180f;
        

        var C = new Vector3(0, Mathf.Sin(dAC) * length2, Mathf.Cos(dAC) * length2) + point2;
        return C;
    }

    private Vector3 GetVertical(Vector3 vec,bool flip) {
        var result = new Vector3(0, vec.z, -vec.y);
        if (!flip) {
            return result;
        } else {
            return -result;
        }

    }


    //----------------------------------------------------------------------

    private Vector3 GetNormal(Vector3 vec1,Vector3 vec2,bool flip) {
        var normal = Vector3.Cross(vec1, vec2).normalized;
        if (flip) {
            normal = -normal;
        }
        return normal;
    }

    private Vector3 Reflect(Vector3 ray,Vector3 normal) {
        var result = -ray + 2 * normal * Vector3.Dot(ray, normal);
        return result.normalized;
    }

    // 计算线ln[2] 与平面plane[4]的交点 interPt
    private bool IsLineInterPlane(NGlbVec3d[] ln, NGlbPlane plane,out NGlbVec3d interPt) {
        interPt = new NGlbVec3d();
        // 直线方程P(t) = Q + tV
        NGlbVec3d Q = ln[0];
        NGlbVec3d V = ln[1] - ln[0];
        V.normalize();

        // 平面方程 N * P(x,y,z) + D = 0
        NGlbVec3d N = new NGlbVec3d(plane.A, plane.B, plane.C);
        //N.normalize();
        float D = plane.D;

        float s = N * V;

        if (s == 0.0) // 直线与平面平行
            return false;

        float q = -D - N * Q;
        float t = q / s;
        // 将t带入直线方程P(t) = Q + tV,就可得到直线与平面的交点
        interPt.x = Q.x + t * V.x;
        interPt.y = Q.y + t * V.y;
        interPt.z = Q.z + t * V.z;
        return true;
    }

    //===========================

    private void ShowInfoOnScreen(Vector3 basePoint,Vector3 shift) {
        var point = basePoint;
        foreach(var info in infoList) {
            Handles.Label(point, (string)info);
            point += shift;
        }

    }



}

public class NGlbVec3d {// 三维点
    public float x, y, z;
    public NGlbVec3d() {
    }
    public NGlbVec3d(Vector3 vector) {
        x = vector.x;y = vector.y;z = vector.z;
    }
    public NGlbVec3d(float vx, float vy, float vz) {
        x = vx; y = vy; z = vz;
    }
    public static float operator *(NGlbVec3d a, NGlbVec3d b) {
        return (a.x * b.x + a.y * b.y + a.z * b.z);
    }
    public static NGlbVec3d operator -(NGlbVec3d a, NGlbVec3d b) {
        NGlbVec3d t = new NGlbVec3d();
        t.x = a.x - b.x;
        t.y = a.y - b.y;
        t.z = a.z - b.z;
        return t;
    }
    public static NGlbVec3d operator ^(NGlbVec3d a, NGlbVec3d b) {
        NGlbVec3d t = new NGlbVec3d();
        t.x = a.y * b.z - a.z * b.y;
        t.y = a.z * b.x - a.x * b.z;
        t.z = a.x * b.y - a.y * b.x;
        return t;
    }
    public void set(float vx, float vy, float vz) {
        x = vx; y = vy; z = vz;
    }
    public void normalize() {
        float t = Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2) + Mathf.Pow(z, 2));
        if (t == 0.0) return;
        x = x / t; y = y / t; z = z / t;
    }

    public Vector3 toVector3() {
        return new Vector3(x, y, z);
    }
}
public class NGlbPlane {// 平面
    public float A, B, C, D;
    public NGlbPlane() {

    }
    public NGlbPlane(float a, float b, float c, float d) {
        A = a; B = b; C = c; D = d;
    }
    public NGlbPlane(NGlbVec3d v1, NGlbVec3d v2, NGlbVec3d v3) {// 根据三个点计算平面方程 A,B,C,D
        NGlbVec3d v = (v3 - v1) ^ (v2 - v1);
        v.normalize();
        A = v.x;
        B = v.y;
        C = v.z;
        D = -(A * v1.x + B * v1.y + C * v1.z);
    }
}


