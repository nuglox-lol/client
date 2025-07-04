using UnityEngine;

public class LuaDataService
{
    public static bool LoadLocal()
    {
        return DataService.LuaLoadLocal(Application.persistentDataPath + "/SaveFile.bpf");
    }

    public static bool LoadInternet(string url)
    {
        return DataService.LuaLoadURL(url);
    }
}
