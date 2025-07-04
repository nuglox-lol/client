using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Threading;

public class OpenScript : MonoBehaviour
{
    private string tempFilePath;
    private GameObject currentScriptObject;
    private bool shouldUpdateScript = false;
    private string updatedScriptContent;

    public void OpenNotepad(GameObject scriptObject)
    {
        currentScriptObject = scriptObject;
        tempFilePath = Path.Combine(Application.persistentDataPath, scriptObject.name + ".lua");
        ScriptInstanceMain scriptComp = scriptObject.GetComponent<ScriptInstanceMain>();
        if (scriptComp != null)
        {
            File.WriteAllText(tempFilePath, scriptComp.Script);
        }
        else
        {
            File.WriteAllText(tempFilePath, "");
        }
        Process notepadProcess = new Process();
        notepadProcess.StartInfo.FileName = "notepad.exe";
        notepadProcess.StartInfo.Arguments = "\"" + tempFilePath + "\"";
        notepadProcess.EnableRaisingEvents = true;
        notepadProcess.Exited += (sender, e) => OnNotepadClosed();
        notepadProcess.Start();
    }

    private void OnNotepadClosed()
    {
        if (currentScriptObject != null && File.Exists(tempFilePath))
        {
            updatedScriptContent = File.ReadAllText(tempFilePath);
            shouldUpdateScript = true;
            File.Delete(tempFilePath);
        }
    }

    private void Update()
    {
        if (shouldUpdateScript)
        {
            shouldUpdateScript = false;
            ScriptInstanceMain scriptComp = currentScriptObject.GetComponent<ScriptInstanceMain>();
            if (scriptComp == null)
            {
                scriptComp = currentScriptObject.AddComponent<ScriptInstanceMain>();
            }
            scriptComp.Script = updatedScriptContent;
        }
    }
}
