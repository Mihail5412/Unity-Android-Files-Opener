 using UnityEngine;
public class DebugLogger : MonoBehaviour
{
    public static DebugLogger single;

    string log;
    Rect rect;
    Texture2D texture;
    //TextMesh textMesh;

    private GUIStyle myStyle;

    private void Awake()
    {
        if (single == null)
            single = this;
        else
            return;
    }

    // Use this for initialization
    void Start()
    {
        log = "";
        rect = new Rect(10, 10, 400, 1080);
        texture = new Texture2D(1,1);
        texture.SetPixel(0, 0, new Color(0,0,0,0.5f));
        texture.Apply();
        //    textMesh = gameObject.GetComponentInChildren<TextMesh>();
        myStyle = new GUIStyle();
        myStyle.fontSize = 15;
        myStyle.normal.textColor = Color.white;
    }

    void OnEnable()
    {
        Application.logMessageReceived += LogMessage;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogMessage;
    }

    private void OnGUI()
    {
        GUI.DrawTexture(rect, texture);
        GUI.TextField(rect, log, myStyle);
    }

    public void LogMessage(string message, string stackTrace, LogType type)
    {
        if (log.Length > 300)
        {
            log = message + "\n";
        }
        else
        {
            log = message + "\n"+ log;
        }
        log = log.Length > 50000 ? log.Substring(0, 50000) : log;
    }

}
