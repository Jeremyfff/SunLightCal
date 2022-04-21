using UnityEditor;
using UnityEngine;



public class CalculateStucture : MonoBehaviour
{
    [SerializeField] private bool DrawGizmos = true;
    [SerializeField] private float ȫ��Gizmo��С = 0.005f;
    [SerializeField] private Color ����Gizmo��ɫ = Color.white;
    [SerializeField] private Color ����Gizmo��ɫ = Color.grey;
    [SerializeField] private Color ���Gizmo��ɫ = Color.red;
    [SerializeField] private Color ����Gizmo��ɫ = Color.yellow;
    [SerializeField] private Transform ��ֱ������Start;
    [SerializeField] private Transform ��ֱ������End;
    [Range(0f,1f)]
    [SerializeField] private float ��ֱ���λ�ðٷֱ�;
    

    [SerializeField] private Transform RootPos;

    [SerializeField] private float ������Ч����;
    [SerializeField] private float �������˳���;
    [SerializeField] private bool �������˷�ת;
    [SerializeField] private float ��������������������֮���ƫ�ƾ���;
    [SerializeField] private bool �������λ�÷�ת;
    [SerializeField] private float �����������rootStart�ľ���;
    [SerializeField] private float ��������������;
    [Range(0.0f,1.0f)]
    [SerializeField] private float �������λ�ðٷֱ�;
    [SerializeField] private float ����������˳���;
    [SerializeField] private bool ����������˷�ת;

    [SerializeField] private float ��һָ�ڿ�ʼλ��Zƫ��;
    [SerializeField] private float ��һָ�ڿ�ʼλ��Yƫ��;
    //��������ƫ�ƶ��ǻ���root������ϵ���ģ��������rootEnd��ƫ��

    [SerializeField] private float ָ�ڽڵ�1��Ч�뾶;
    [SerializeField] private float ��һָ����link1End�н�;
    [SerializeField] private float ��һָ�ڳ���;
    [SerializeField] private float ��һָ�����˳���;

    [SerializeField] private float Link2Start�ĽǶ�λ��;
    [SerializeField] private float ָ�ڽڵ�2��Ч�뾶;
    [SerializeField] private float �ڶ�ָ����link2End�н�;
    [SerializeField] private float �ڶ�ָ�ڳ���;
    [SerializeField] private float �ڶ�ָ�����˳���;

    [SerializeField] private float Link3Start�ĽǶ�λ��;
    [SerializeField] private float ָ�ڽڵ�3��Ч�뾶;
    [SerializeField] private float ����ָ����link3End�н�;
    [SerializeField] private float ����ָ�ڳ���;
    [SerializeField] private float ����ָ�����˳���;


    [SerializeField] private float ��һָ�ڷ��������ƫ�ƾ���;
    [SerializeField] private float ��һָ�ڷ�������˳���;
    [SerializeField] private float ��һָ�ڷ���峤��;


    [SerializeField] private float �ڶ�ָ�ڷ�������˳���;
    [SerializeField] private float �ڶ�ָ�ڷ���峤��;

    [SerializeField] private float ����ָ�ڷ��������ƫ�ƾ���;
    [SerializeField] private float ����ָ�ڷ�������˳���;
    [SerializeField] private float ����ָ�ڷ���峤��;

    private bool buildError = false;

    [SerializeField] private Transform PV�ײ�;
    [SerializeField] private Transform PV����;
    [SerializeField] private float PV���;

    [SerializeField] private bool calReflection;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos() {
        var sunManager = SunManager.instance;
        if(sunManager == null) {
            calReflection = false;
        }
        
        buildError = false;
        if (DrawGizmos) {
            var motor1Pos = DrawMotor1();
            Handles.Label(motor1Pos, "motor1Pos");

            var rootEnd = RootPos.position;
            Handles.Label(RootPos.position, "rootEnd");

            var rootStart = DrawRoot(motor1Pos);
            Handles.Label(rootStart, "rootStart");

            var motor2Pos = DrawMotor2(rootStart, rootEnd);
            Handles.Label(motor2Pos, "motor2Pos");


            //����features

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

            //�Ӿ�Ч��
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


            //=============================

            var PV_Plane = DrawPV();
            if (calReflection) {
                //var sunDir = new Vector3(0.1f, -1f, 1f);
                var sunDir = sunManager.GetSunDir();
                //Debug.Log(sunDir);

                Vector3[] pts_A = { pt_A1, pt_A2, pt_A3, pt_A4 };
                var castPoints_A = DrawLightCast(sunDir, pts_A, PV_Plane);
                Vector3[] pts_B = { pt_B1, pt_B2, pt_B3, pt_B4 };
                var castPoints_B = DrawLightCast(sunDir, pts_B, PV_Plane);
                Vector3[] pts_C = { pt_C1, pt_C2, pt_C3, pt_C4 };
                var castPoints_C = DrawLightCast(sunDir, pts_C, PV_Plane);

                CalIntersectArea(castPoints_A, PV����.position,PV�ײ�.position);





            }

        }
        if (buildError) {
            Debug.Log("Build Error");
            
        }

    }

    private void DrawStructure() {

    }

    //----------------------------------------------------------------------

    private Vector3 DrawMotor1() {

        var motorPos = Vector3.Lerp(��ֱ������Start.position, ��ֱ������End.position, ��ֱ���λ�ðٷֱ�);
        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(��ֱ������Start.position, ��ֱ������End.position);
            Gizmos.color = ���Gizmo��ɫ;
            Gizmos.DrawSphere(motorPos, ȫ��Gizmo��С);
        }

        return motorPos;
    }

    private Vector3 DrawRoot(Vector3 Motor1Pos) {
        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(RootPos.position + Vector3.left * 10, RootPos.position + Vector3.right * 10);
            Gizmos.DrawSphere(RootPos.position, ȫ��Gizmo��С);
        }

        var point1 = Motor1Pos;
        var point2 = RootPos.position;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, �������˳���, ������Ч����, �������˷�ת);

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, ȫ��Gizmo��С);
        }

        return point3;
    }

    private Vector3 DrawMotor2(Vector3 rootStart,Vector3 rootEnd) {
        Vector3 rootDir = rootEnd - rootStart;
        Vector3 shiftDir = GetVertical(rootDir, �������λ�÷�ת);
        shiftDir = shiftDir.normalized * ��������������������֮���ƫ�ƾ���;
        
        var motor2Start = rootStart + shiftDir + rootDir.normalized * �����������rootStart�ľ���;
        var motor2End = motor2Start + rootDir.normalized * ��������������;

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(motor2Start, motor2End);//������ƶ���Χ��line
        }

        var motor2Pos = Vector3.Lerp(motor2Start, motor2End, �������λ�ðٷֱ�);

        if (DrawGizmos) {
            Gizmos.color = ���Gizmo��ɫ;
            Gizmos.DrawSphere(motor2Pos, ȫ��Gizmo��С);
        }


        return motor2Pos;
    }


    private Vector3 DrawFingle1Start(Vector3 rootStart,Vector3 rootEnd) {
        var Zdir = (rootEnd - rootStart).normalized;
        var Ydir = GetVertical(Zdir, false).normalized;
        Zdir *= ��һָ�ڿ�ʼλ��Zƫ��;
        Ydir *= ��һָ�ڿ�ʼλ��Yƫ��;

        var result = rootEnd + Zdir + Ydir;
        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawSphere(result, ȫ��Gizmo��С);
            Gizmos.DrawLine(result, rootEnd);
        }
        return result;

    }


    private Vector3 DrawLink1(Vector3 motor2Pos,Vector3 rootEnd) {
        var point1 = motor2Pos;
        var point2 = rootEnd;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, ����������˳���, ָ�ڽڵ�1��Ч�뾶, ����������˷�ת);

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, ȫ��Gizmo��С);
        }


        return point3;
    }

    private Vector3 DrawNode1(Vector3 center,Vector3 link1End) {
        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawWireSphere(center, ָ�ڽڵ�1��Ч�뾶);
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

        var dB = ��һָ����link1End�н� * Mathf.PI / 180f + dA;

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
        var dB = Link2Start�ĽǶ�λ�� * Mathf.PI / 180f + dA;

        var newDir = new Vector3(0, Mathf.Sin(dB), Mathf.Cos(dB));

        var result = center + newDir * ָ�ڽڵ�1��Ч�뾶;

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawSphere(result, ȫ��Gizmo��С);
        }


        return result;

    }

    private Vector3 DrawFingle1(Vector3 start,Vector3 dir) {
        var end = start + dir * ��һָ�ڳ���;

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(start, end);
        }

        return end;
    }

    private Vector3 DrawLink2(Vector3 link2Start,Vector3 fingle1End) {
        var point1 = link2Start;
        var point2 = fingle1End;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, ��һָ�����˳���, ָ�ڽڵ�2��Ч�뾶, false);

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, ȫ��Gizmo��С);
        }


        return point3;
    }

    private Vector3 DrawNode2(Vector3 center, Vector3 link2End) {
        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawWireSphere(center, ָ�ڽڵ�2��Ч�뾶);
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

        var dB = �ڶ�ָ����link2End�н� * Mathf.PI / 180f + dA;

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
        var dB = Link3Start�ĽǶ�λ�� * Mathf.PI / 180f + dA;

        var newDir = new Vector3(0, Mathf.Sin(dB), Mathf.Cos(dB));

        var result = center + newDir * ָ�ڽڵ�2��Ч�뾶;

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawSphere(result, ȫ��Gizmo��С);
        }


        return result;

    }

    private Vector3 DrawFingle2(Vector3 start, Vector3 dir) {
        var end = start + dir * �ڶ�ָ�ڳ���;

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(start, end);
        }


        return end;
    }

    private Vector3 DrawLink3(Vector3 link3Start, Vector3 fingle2End) {
        var point1 = link3Start;
        var point2 = fingle2End;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, �ڶ�ָ�����˳���, ָ�ڽڵ�3��Ч�뾶, false);

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, ȫ��Gizmo��С);
        }


        return point3;
    }

    private Vector3 DrawNode3(Vector3 center, Vector3 link3End) {
        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawWireSphere(center, ָ�ڽڵ�3��Ч�뾶);
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

        var dB = ����ָ����link3End�н� * Mathf.PI / 180f + dA;

        var newDir = new Vector3(0, Mathf.Sin(dB), Mathf.Cos(dB));
        return newDir;
    }

    private Vector3 DrawFingle3(Vector3 start, Vector3 dir) {
        var end = start + dir * ����ָ�ڳ���;
        Gizmos.color = ����Gizmo��ɫ;
        Gizmos.DrawLine(start, end);
        return end;
    }


    //----------------------------------------------------------------------

    private Vector3 DrawFingle2Rod(Vector3 fingle2Center,Vector3 fingle2Dir,bool flip) {
        var rodDir = GetVertical(fingle2Dir, flip).normalized * �ڶ�ָ�ڷ�������˳���;
        var rodEnd = fingle2Center + rodDir;

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(fingle2Center, rodEnd);
            Gizmos.DrawSphere(rodEnd, ȫ��Gizmo��С);
        }

        return fingle2Center + rodDir;

    }

    private void DrawReflectBoard2(Vector3 rod2End, Vector3 fingle2Dir,out Vector3 boardStart,out Vector3 boardEnd) {
        var halfDir = fingle2Dir.normalized * �ڶ�ָ�ڷ���峤�� * 0.5f;
        boardStart = rod2End - halfDir;
        boardEnd = rod2End + halfDir;

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(boardStart, boardEnd);
        }

    }

    private Vector3 DrawFingle1Rod(Vector3 fingle1Start,Vector3 fingle1End,Vector3 board2Start,bool flip) {
        var center = (fingle1Start + fingle1End)/ 2f;
        var shiftDir = GetVertical((fingle1End - fingle1Start), true).normalized * ��һָ�ڷ��������ƫ�ƾ���;

        var point1 = center + shiftDir;
        var point2 = board2Start;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, ��һָ�ڷ�������˳���, ��һָ�ڷ���峤��/2f, flip);

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawSphere(point1, ȫ��Gizmo��С);
            Gizmos.DrawLine(point1, center);
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, ȫ��Gizmo��С);
        }


        return point3;//����rod1end

    }
    private Vector3 DrawReflectBoard1(Vector3 board2Start,Vector3 rod1End) {
        var dir = (rod1End - board2Start).normalized * ��һָ�ڷ���峤��;
        var board1Start = board2Start + dir;

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(board1Start, board2Start);
        }


        return board1Start;

    }


    private Vector3 DrawFingle3Rod(Vector3 fingle3Start,Vector3 fingle3End,Vector3 board2End,bool flip) {
        var center = (fingle3Start + fingle3End) / 2f;
        var shiftDir = GetVertical((fingle3End - fingle3Start), true).normalized * ����ָ�ڷ��������ƫ�ƾ���;

        var point1 = center + shiftDir;
        var point2 = board2End;
        var point3 = CalculatePointFromPointsAndLength(point1, point2, ����ָ�ڷ�������˳���, ����ָ�ڷ���峤�� / 2f, flip);

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawSphere(point1, ȫ��Gizmo��С);
            Gizmos.DrawLine(point1, center);
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(point1, point3);
            Gizmos.DrawLine(point2, point3);
            Gizmos.DrawSphere(point3, ȫ��Gizmo��С);
        }


        return point3;//����rod3end
    }

    private Vector3 DrawReflectBoard3(Vector3 board2End,Vector3 rod3End) {
        var dir = (rod3End - board2End).normalized * ����ָ�ڷ���峤��;
        var board3End = board2End + dir;

        if (DrawGizmos) {
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(board2End, board3End);
        }

        return board3End;
    }


    //----------------------------------------------------------------------


    private NGlbPlane DrawPV() {
        var top = PV����.position;
        var bot = PV�ײ�.position;
        var p_x = new Vector3(PV���/2f, 0, 0);
        var pt_P1 = bot - p_x;
        var pt_P2 = bot + p_x;
        var pt_P3 = top + p_x;
        var pt_P4 = top - p_x;

        if (DrawGizmos) {
            
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(bot, top);
            Gizmos.color = ����Gizmo��ɫ;
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
            Gizmos.color = ����Gizmo��ɫ;
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
            Gizmos.color = ����Gizmo��ɫ;
            Gizmos.DrawLine(result[0], result[1]);
            Gizmos.DrawLine(result[1], result[2]);
            Gizmos.DrawLine(result[2], result[3]);
            Gizmos.DrawLine(result[3], result[0]);

        }

        return result;
    }

    private void CalIntersectArea(Vector3[] castPoints,Vector3 PV_top,Vector3 PV_bot) {
/*        //��������PVƽ��
        var p_x = new Vector3(1, 0, 0);
        var p_z = new Vector3(0, 0, 1);
        var vituralPlane_top = new NGlbPlane(new NGlbVec3d(PV_top) , new NGlbVec3d(PV_top+p_x), new NGlbVec3d(PV_top+p_z));
        var vituralPlane_bot = new NGlbPlane(new NGlbVec3d(PV_bot), new NGlbVec3d(PV_bot + p_x), new NGlbVec3d(PV_bot + p_z));

        NGlbVec3d[] ln1 = { new NGlbVec3d(castPoints[0]), new NGlbVec3d(castPoints[3]) };
        NGlbVec3d[] ln2 = { new NGlbVec3d(castPoints[1]), new NGlbVec3d(castPoints[2]) };

        NGlbVec3d hit1_top; NGlbVec3d hit1_bot; NGlbVec3d hit2_top; NGlbVec3d hit2_bot;
        var hit1_top_isHit = IsLineInterPlane(ln1, vituralPlane_top, out hit1_top);
        var hit1_bot_isHit = IsLineInterPlane(ln1, vituralPlane_bot, out hit1_bot);
        var hit2_top_isHit = IsLineInterPlane(ln2, vituralPlane_top, out hit2_top);
        var hit2_bot_isHit = IsLineInterPlane(ln2, vituralPlane_bot, out hit2_bot);

        Vector3 ln1_top = castPoints[3];
        Vector3 ln1_bot = castPoints[0];

        Vector3 ln2_top = castPoints[2];
        Vector3 ln2_bot = castPoints[3];


        Gizmos.color = ����Gizmo��ɫ;
        if (hit1_top_isHit) {
            ln1_top = hit1_top.toVector3();
            Gizmos.DrawSphere(ln1_top, ȫ��Gizmo��С);
        }
        if (hit1_bot_isHit) {
            ln1_bot = hit1_bot.toVector3();
            Gizmos.DrawSphere(ln1_bot, ȫ��Gizmo��С);
        }
        if (hit2_top_isHit) {
            ln2_top = hit2_top.toVector3();
            Gizmos.DrawSphere(ln2_top, ȫ��Gizmo��С);
        }
        if (hit2_bot_isHit) {
            ln2_bot = hit2_bot.toVector3();
            Gizmos.DrawSphere(ln2_bot, ȫ��Gizmo��С);
        }


        var height = Vector3.Distance(ln1_top, ln1_bot);
        var ����CubeSize = new Vector3(0.1f, 0.1f, 0.1f);
*/


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
        
        //�ж�P1 P2��λ�ù�ϵ������dAC�ķ��źʹ�С
        if (point1.z > point2.z) {
            //���1��2���ҷ�
            if (point1.y > point2.y) {
                //���1��2���Ϸ�
                dAB = dAB;
            } else {
                //���1��2���·�
                dAB = -dAB;
            }
        } else {
            //���1��2����
            if (point1.y > point2.y) {
                //���1��2���Ϸ�
                dAB = 180-dAB;
            } else {
                //���1��2���·�
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

    // ������ln[2] ��ƽ��plane[4]�Ľ��� interPt
    private bool IsLineInterPlane(NGlbVec3d[] ln, NGlbPlane plane,out NGlbVec3d interPt) {
        interPt = new NGlbVec3d();
        // ֱ�߷���P(t) = Q + tV
        NGlbVec3d Q = ln[0];
        NGlbVec3d V = ln[1] - ln[0];
        V.normalize();

        // ƽ�淽�� N * P(x,y,z) + D = 0
        NGlbVec3d N = new NGlbVec3d(plane.A, plane.B, plane.C);
        //N.normalize();
        float D = plane.D;

        float s = N * V;

        if (s == 0.0) // ֱ����ƽ��ƽ��
            return false;

        float q = -D - N * Q;
        float t = q / s;
        // ��t����ֱ�߷���P(t) = Q + tV,�Ϳɵõ�ֱ����ƽ��Ľ���
        interPt.x = Q.x + t * V.x;
        interPt.y = Q.y + t * V.y;
        interPt.z = Q.z + t * V.z;
        return true;
    }


}

public class NGlbVec3d {// ��ά��
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
public class NGlbPlane {// ƽ��
    public float A, B, C, D;
    public NGlbPlane() {

    }
    public NGlbPlane(float a, float b, float c, float d) {
        A = a; B = b; C = c; D = d;
    }
    public NGlbPlane(NGlbVec3d v1, NGlbVec3d v2, NGlbVec3d v3) {// �������������ƽ�淽�� A,B,C,D
        NGlbVec3d v = (v3 - v1) ^ (v2 - v1);
        v.normalize();
        A = v.x;
        B = v.y;
        C = v.z;
        D = -(A * v1.x + B * v1.y + C * v1.z);
    }
}


