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
        "unity.exe",
        "unityeditor.exe",
        "unitycrashhandler64.exe",
        "unitycrashhandler32.exe",
        "unityeditor_data.dll",
        "unityengine.dll",
        "gameassembly.dll",
        "ispc_texcomp.dll",
        "s3tcompress.dll",
        "libfbxsdk.dll",
        "etccompress.dll",
        "winpixeventruntime.dll",
        "compress_bc7e.dll",
        "openrl.dll",
        "umbraoptimizer64.dll",
        "sketchupapi.dll",
        "sketchupcommonpreferences.dll",
        "nativeproxyhelper.dll",
        "embree.dll",
        "tbb.dll",
        "openrl_pthread.dll",
        "freeimage.dll",
        "nvunityplugin.dll",
        "astcenc-avx2.dll",
        "mono-2.0-bdwgc.dll",
        "mono-2.0-sgen.dll",
        "mono-2.0.dll",
        "mono.dll",
        "astcenc-avx2.dll",
        "physx3common_x64.dll",
        "physx3_x64.dll",
        "physx3characterkinematic_x64.dll",
        "physx3cooking_x64.dll",
        "physx3extensions_x64.dll",
        "physx3vehicle_x64.dll",
        "physx3common_x86.dll",
        "physx3_x86.dll",
        "physx3characterkinematic_x86.dll",
        "physx3cooking_x86.dll",
        "physx3extensions_x86.dll",
        "physx3vehicle_x86.dll",
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
        "wsock32.dll",
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
        "d3d9.dll",
        "d3d12.dll",
        "dxgi.dll",
        "opengl32.dll",
        "glu32.dll",
        "gdi32full.dll",
        "winmm.dll",
        "shcore.dll",
        "dbghelp.dll",
        "powrprof.dll",
        "windows.storage.dll",
        "kernel.appcore.dll",
        "profapi.dll",
        "propsys.dll",
        "wtsapi32.dll",
        "winhttp.dll",
        "wininet.dll",
        "cryptbase.dll",
        "ntasn1.dll",
        "cryptsp.dll",
        "msasn1.dll",
        "sspicli.dll",
        "oleacc.dll",
        "netapi32.dll",
        "mpr.dll",
        "svchost.exe",
        "taskhostw.exe",
        "explorer.exe",
        "kernelbase.dll",
        "win32u.dll",
        "msvcp_win.dll",
        "cfgmgr32.dll",
        "wintrust.dll",
        "hid.dll",
        "secur32.dll",
        "mswsock.dll",
        "dui70.dll",
        "comctl32.dll",
        "explorerframe.dll",
        "textshaping.dll",
        "textinputframework.dll",
        "coreuicomponents.dll",
        "coremessaging.dll",
        "wintypes.dll",
        "rsaenh.dll",
        "imagehlp.dll",
        "gpapi.dll",
        "cryptnet.dll",
        "winnsi.dll",
        "nsi.dll",
        "iconcodecservice.dll",
        "windowscodecs.dll",
        "netprofm.dll",
        "npmproxy.dll",
        "dhcpcsvc6.dll",
        "dhcpcsvc.dll",
        "napinsp.dll",
        "pnrpnsp.dll",
        "wshbth.dll",
        "nlaapi.dll",
        "winrnr.dll",
        "fwpuclnt.dll",
        "rasadhlp.dll",
        "mfplat.dll",
        "rtworkq.dll",
        "mfreadwrite.dll",
        "mf.dll",
        "mfcore.dll",
        "ksuser.dll",
        "umpdc.dll",
        "msvcp140.dll",
        "msvcp120.dll",
        "msvcp110.dll",
        "msvcp100.dll",
        "msvcr120.dll",
        "msvcr110.dll",
        "msvcr100.dll",
        "msvcr71.dll",
        "dinput8.dll",
        "dinput.dll",
        "d3dx9_43.dll",
        "d3dx11_43.dll",
        "d3dx10_43.dll",
        "xaudio2_9.dll",
        "xaudio2_8.dll",
        "xaudio2_7.dll",
        "openal32.dll",
        "nvapi64.dll",
        "amdihk64.dll",
        "amdihk32.dll",
        "atiadlxx.dll",
        "atidxx64.dll",
        "atidxx32.dll",
        "nvd3dumx.dll",
        "nvd3dum.dll",
        "nvspcap64.dll",
        "clr.dll",
        "mscoreei.dll",
        "mscoree.dll",
        "mscorlib.dll",
        "mscorjit.dll",
        "apphelp.dll",
        "dwmapi.dll",
        "bcryptprimitives.dll",
        "wldp.dll",
        "ntmarta.dll",
        "iphlpapi.dll",
        "dnsapi.dll",
        "olecli32.dll",
        "oledlg.dll",
        "actxprxy.dll",
        "dbgcore.dll",
        "dwmcore.dll",
        "wer.dll",
        "werfault.exe",
        "werconcpl.dll",
        "faultrep.dll"
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
                    //UnityEngine.Debug.LogError($"[Electron] Unknown DLL detected: {lowerModule}");

                    if (NetworkClient.isConnected)
                    {
                        //NetworkManager.singleton.StopClient(); // disabled anticheat
                        //UnityEngine.Debug.LogWarning("Disconnected due to potential DLL injection.");
                        //Application.Quit();
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