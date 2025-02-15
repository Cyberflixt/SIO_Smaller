using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public static class DebugPlus
{
    // Public
    public static Color debugColor = Color.red;

    // Private
    private static List<Vector3> line_list = new List<Vector3>();
    private static List<Color> line_color = new List<Color>();

    private static List<Vector3> label_pos = new List<Vector3>();
    private static List<string> label_text = new List<string>();
    private static List<Color> label_color = new List<Color>();

    private static List<Vector3> sphere_pos = new List<Vector3>();
    private static List<float> sphere_radius = new List<float>();
    private static List<Color> sphere_color = new List<Color>();

    private static List<GameObject> temp_obj = new List<GameObject>();


    // POSITIONS

    /// <summary>
    /// Draw sphere at a given position, and given scale (default 1)
    /// </summary>
    /// <param name="pos">Position</param>
    /// <param name="scale">Sphere's scale</param>
    public static void Draw(Vector3 pos, float radius = 1){
        sphere_pos.Add(pos);
        sphere_radius.Add(radius);
        sphere_color.Add(debugColor);
    } public static void Draw(Vector3[] posList, float radius = 1){
        foreach(var pos in posList){
            Draw(pos, radius);
        }
    } public static void Draw(List<Vector3> posList, float radius = 1){
        foreach(var pos in posList){
            Draw(pos, radius);
        }
    }

    // LINE

    /// <summary>
    /// Draw a line start at a given position and ending at another given position
    /// </summary>
    /// <param name="from">Start position</param>
    /// <param name="to">End position</param>
    public static void DrawLine(Vector3 from, Vector3 to){
        line_list.Add(from);
        line_list.Add(to);
        line_color.Add(debugColor);
    } public static void DrawLine(Vector3[] from, Vector3[] to){
        for (int i = 0; i < from.Length; i++){
            DrawLine(from[i], to[i]);
        }
    } public static void DrawLine(List<Vector3> from, List<Vector3> to){
        for (int i = 0; i < from.Count; i++){
            DrawLine(from[i], to[i]);
        }
    }

    // Arrow

    public static void DrawArrowVector(Vector3 pos, Vector3 vector)
    {
        // Draw an arrow using a line and multiple spheres
        DrawVector(pos, vector);
        
        float headRes = 10f;
        float headSpread = .2f;
        float headScale = vector.magnitude*.2f;
        for (int x=0; x<headRes; x++){
            float alpha = x/headRes;
            Draw(pos + vector * (alpha * headSpread + 1-headSpread), (1-alpha) * headScale);
        }
    } public static void DrawArrowTo(Vector3 a, Vector3 b) {
        DrawArrowVector(a, b-a);
    }

    // VECTOR
    /// <summary>
    /// Draw a direction vector at a given position
    /// </summary>
    /// <param name="pos">Starting position</param>
    /// <param name="vec">Direction vector</param>
    public static void DrawVector(Vector3 pos, Vector3 vec){
        line_list.Add(pos);
        line_list.Add(pos+vec);
        line_color.Add(debugColor);
    } public static void DrawVector(Vector3[] pos, Vector3[] vec){
        for (int i = 0; i < pos.Length; i++){
            DrawVector(pos[i], vec[i]);
        }
    } public static void DrawVector(List<Vector3> pos, List<Vector3> vec){
        for (int i = 0; i < pos.Count; i++){
            DrawVector(pos[i], vec[i]);
        }
    }

    public static void DrawCube(Vector3 position, Vector3 size, Quaternion rotation){
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.rotation = rotation;
        cube.transform.localScale = size;

        MeshRenderer render = cube.GetComponent<MeshRenderer>();
        render.material = DebugInit.instance.material;

        Collider col = cube.GetComponent<Collider>();
        if (col) GameObject.Destroy(col);

        temp_obj.Add(cube.gameObject);
        UnityEngine.Object.Destroy(cube.gameObject, .2f);
    }

    // Draw text

    /// <summary>
    /// Draw text label at given position
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="text"></param>
    public static void DrawText(Vector3 pos, string text){
        label_pos.Add(pos);
        label_text.Add(text);
        label_color.Add(debugColor);
    } public static void DrawText(Vector3 pos, params object[] args){
        string str = ArrayToString(args);
        DrawText(pos, str);
    } public static void DrawText(Vector3[] posArr, params object[] args){
        string str = ArrayToString(args);
        foreach (Vector3 pos in posArr){
            DrawText(pos, str);
        }
    } public static void DrawText(List<Vector3> posList, params object[] args){
        string str = ArrayToString(args);
        foreach (Vector3 pos in posList){
            DrawText(pos, str);
        }
    }

    // Draw triangle

    /// <summary>
    /// Draw a triangle with 3 given positions
    /// </summary>
    /// <param name="a">First position</param>
    /// <param name="b">Second position</param>
    /// <param name="c">Third position</param>
    /// <param name="recursion"></param>
    public static void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, int recursion = 0){
        // Use recursion parameter to "fill" the triangle

        DrawLine(a,b);
        DrawLine(b,c);
        DrawLine(c,a);

        if (recursion > 0){
            Vector3 ab = (a+b)/2f;
            Vector3 bc = (b+c)/2f;
            Vector3 ca = (c+a)/2f;

            DrawTriangle(a, ab, ca, recursion-1);
            DrawTriangle(b, bc, ab, recursion-1);
            DrawTriangle(c, ca, bc, recursion-1);
            DrawTriangle(ab, bc, ca, recursion-1);
        }
    }

    // Draw cycle

    public static void DrawCycle(List<Vector3> vertices){
        if (vertices.Count < 2) return;

        for (int i = 0; i<vertices.Count-1; i++){
            DrawLine(vertices[i], vertices[i+1]);
        }
        DrawLine(vertices[vertices.Count-1], vertices[0]);
    } public static void DrawCycle(List<Vector3> vertices, List<int> indices){
        if (indices.Count < 2) return;

        for (int i = 0; i<indices.Count-1; i++){
            DrawLine(vertices[indices[i]], vertices[indices[i+1]]);
        }
        DrawLine(vertices[indices[indices.Count-1]], vertices[indices[0]]);
    }

    // Misc

    /// <summary>
    /// Set a random color to the debug elements
    /// </summary>
    /// <returns></returns>
    public static Color RandomColor(){
        // Random full saturated color
        debugColor = Color.HSVToRGB(UnityEngine.Random.value, 1, 1);
        return debugColor;
    }

    // System
    public static void UpdateDebug(){
        // Refresh the display on all gizmos
		# if UNITY_EDITOR
        for (int i=0; i<sphere_pos.Count; i++){
            Gizmos.color = sphere_color[i];
            Gizmos.DrawSphere(sphere_pos[i], .1f * sphere_radius[i]);
        }
        
        for (int i=0; i<line_color.Count; i++){
            Gizmos.color = line_color[i];
            Gizmos.DrawLine(line_list[i*2], line_list[i*2+1]);
        }

        for (int i=0; i<label_text.Count; i++){
            Gizmos.color = label_color[i];
            Handles.Label(label_pos[i], label_text[i]);
        }
        
        for (int i=0; i<sphere_pos.Count; i++){
            Gizmos.color = sphere_color[i];
            Gizmos.DrawSphere(sphere_pos[i], sphere_radius[i]);
        }
		#endif
    }

    public static void ClearDebug(){
        // Removes all current debug displays

        sphere_pos = new List<Vector3>();
        sphere_radius = new List<float>();
        sphere_color = new List<Color>();
        
        line_list = new List<Vector3>();
        line_color = new List<Color>();

        label_pos = new List<Vector3>();
        label_text = new List<string>();
        label_color = new List<Color>();

        foreach (GameObject obj in temp_obj){
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }

    // Printing

    private static string ArrayToStringPrefix(Array arr){
        // Array to string: with prefix indicating types
        // Res: int[#6]{1, 2, 3, 4, 5, 6}
        
        Type type = arr.GetType().GetElementType();
        return type.ToString()+"[#"+arr.Length+"]{"+ArrayToStringSeparator(", ",arr)+"}";
    }
    private static string DictToString<TA, TB>(Dictionary<TA, TB> dict){
        // Dictionary to string
        // Res: Dict<string, int>{{Lorem, 1},{Ipsum, 2}}

        int len = dict.Count;
        string res = "Dict<"+typeof(TA).ToString()+", "+typeof(TB).ToString()+">[#"+len+"]{";

        int i = 0;
        foreach(var kv in dict){
            res += "{"+AnyToString(kv.Key)+", "+AnyToString(kv.Value)+"}";

            i++;
            if (i<len) res += ",   ";
        }
        return res + "}";
    }

    private static string AnyToString(object any){
        // Anything to string: array, dictionaries, etc...

        switch (any) {
        case Array arr:
            return ArrayToStringPrefix(arr);
        case IEnumerable<object> ie:
            return ArrayToStringPrefix(ie.ToArray());
        case IList list:
            // Process IList (including List)
            Array copy = new object[list.Count];
            list.CopyTo(copy, 0);
            return ArrayToStringPrefix(copy);
        case null:
            return "Null";
        default:
            // Handle unknown other types
            return any.ToString();
        }
    }

    public static string ArrayToStringSeparator(string separator, Array args){
        int len = args.Length;
        string str = "";
        for (int i=0; i<len; i++){
            var v = args.GetValue(i);
            str += AnyToString(v);
            if (i<len-1)
                str += separator;
        }
        
        return str;
    }

    public static string ArrayToString(Array args){
        // Array to string separated by spaces
        return ArrayToStringSeparator(" ", args);
    }

    private static string LogString(string message){
        // Add the script name before the message
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
        System.Diagnostics.StackFrame[] stackFrames = stackTrace.GetFrames();

        if (stackFrames.Length > 1)
        {
            // Get the caller's frame (the one before DebugPlus)
            System.Diagnostics.StackFrame callerFrame = stackFrames[1];
            string callerFile = callerFrame.GetFileName();
            int callerLine = callerFrame.GetFileLineNumber();
            
            // Keep only the last file name
            callerFile = callerFile.Replace(@"\", "/");
            string[] arr = callerFile.Split("/");
            callerFile = arr[arr.Length-1];

            // formatted custom log output
            string formattedMessage = $"<color=cyan>[{callerFile}:{callerLine}]</color> {message}";
            Debug.Log(formattedMessage);
        }
        else
        {
            Debug.Log(message);
        }
        return message;
    }

    public static string Log(params object[] args){
        return LogString(ArrayToString(args));
    } public static string Log<TA, TB>(Dictionary<TA, TB> dict, params object[] args){
        // Fuck dictionaries
        return LogString(DictToString(dict) + " " + ArrayToString(args));
    }
    public static string Log(this object obj, params object[] args){
        return LogString(AnyToString(obj)+" -> "+ArrayToString(args));
    }
    public static int CountTrue(params bool[] args) {
        return args.Count(t => t);
    }
}
