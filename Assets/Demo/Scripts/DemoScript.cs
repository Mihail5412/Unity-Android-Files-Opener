using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityAndroidOpenUrl;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    private const string MAIN_DIR = "/storage/emulated/0";
    private const string testFolderName = "Test Documents";
    private const string templatePDFName = "template.pdf";

    private static string pathToTestFolder;
    private static string pathToTemplatePDF;

    private void Start()
    {
        pathToTestFolder = Path.Combine(MAIN_DIR, testFolderName);
        if (!Directory.Exists(pathToTestFolder))
        {
            Directory.CreateDirectory(pathToTestFolder);
        }

        StartCoroutine(SaveTemplatePDF());
    }

    private IEnumerator SaveTemplatePDF()
    {
        string localPathToTemplatePDF = Path.Combine(Application.streamingAssetsPath, templatePDFName);
        WWW www = new WWW(localPathToTemplatePDF);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("Error while loading template PDF: " + www.error);
            yield break;
        }
        pathToTemplatePDF = Path.Combine(pathToTestFolder, templatePDFName);
        File.WriteAllBytes(pathToTemplatePDF, www.bytes);

        AndroidOpenUrl.OpenFile(pathToTemplatePDF);
    }
}
