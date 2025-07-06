using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Mirror;

public class Electron : MonoBehaviour
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetCurrentProcess();

    [DllImport("psapi.dll", SetLastError = true)]
    static extern bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr[] lphModule, int cb, out int lpcbNeeded);

    [DllImport("psapi.dll")]
    static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpFilename, int nSize);

    static readonly string[] SafeModules = new string[]
    {
        "unityplayer.dll",
        "gameassembly.dll",
        "winhttp.dll",
        "dbghelp.dll",
        "physx3common_x64.dll",
        "physx3_x64.dll",
        "physx3characterkinematic_x64.dll",
        "kernel32.dll",
        "user32.dll",
        "ntdll.dll",
        "gdi32.dll",
        "advapi32.dll",
        "ole32.dll",
        "oleaut32.dll",
        "shell32.dll",
        "comdlg32.dll",
        "ws2_32.dll",
        "crypt32.dll",
        "sechost.dll",
        "msvcrt.dll",
        "shlwapi.dll",
        "uxtheme.dll",
        "rpcrt4.dll",
        "combase.dll",
        "imm32.dll",
        "setupapi.dll",
        "version.dll",
        "bcrypt.dll",
        "clbcatq.dll",
        "msctf.dll",
        "ucrtbase.dll",
        "vcruntime140.dll",
        "vcruntime140_1.dll",
        "d3d11.dll",
        "dxgi.dll",
        "opengl32.dll",
        "nvapi64.dll",
        "amdihk64.dll",
        "mscoree.dll",
        "winmm.dll",
        "shcore.dll",
        "wsock32.dll",
    };

    void Start()
    {
        StartCoroutine(ScanLoop());
    }

    IEnumerator ScanLoop()
    {
        while (true)
        {
            ScanForInjectedDLLs();
            yield return new WaitForSeconds(10f);
        }
    }

    void ScanForInjectedDLLs()
    {
        try
        {
            IntPtr processHandle = GetCurrentProcess();
            IntPtr[] modules = new IntPtr[1024];
            EnumProcessModules(processHandle, modules, modules.Length * IntPtr.Size, out int bytesNeeded);
            int count = bytesNeeded / IntPtr.Size;

            for (int i = 0; i < count; i++)
            {
                StringBuilder moduleName = new StringBuilder(1024);
                GetModuleFileNameEx(processHandle, modules[i], moduleName, moduleName.Capacity);

                string lowerModule = moduleName.ToString().ToLower();
                bool isSafe = false;

                foreach (string safe in SafeModules)
                {
                    if (lowerModule.Contains(safe.ToLower()))
                    {
                        isSafe = true;
                        break;
                    }
                }

                if (!isSafe)
                {
                    UnityEngine.Debug.LogError($"[Electron] Unknown DLL detected: {lowerModule}");

                    if (NetworkClient.isConnected)
                    {
                        NetworkManager.singleton.StopClient();
                        UnityEngine.Debug.LogWarning("Disconnected due to potential DLL injection.");
                        Application.Quit();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[Electron] Exception: {ex.Message}");
        }
    }
}