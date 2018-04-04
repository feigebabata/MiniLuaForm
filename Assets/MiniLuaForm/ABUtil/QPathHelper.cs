using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class QPathHelper
{
    /// <summary>
    /// 获取assetbundle的输出目录
    /// </summary>
    /// <returns></returns>
    public static string GetAssetBundleOutPath()
    {
        string outPath = GetPlatformPath() + "/" + GetPlatformName();

        if (!Directory.Exists(outPath))
            Directory.CreateDirectory(outPath);

        return outPath;
    }

    /// <summary>
    /// 自动获取对应平台的路径
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformPath()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                return Application.streamingAssetsPath;
            case RuntimePlatform.Android:
                //string str = Application.dataPath + "!/assets";
                string str = Application.persistentDataPath;
                return str;
            default:
                return null;
        }

    }

    /// <summary>
    /// 获取对应平台的名字
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformName()
    {
        return "windows";

        //switch (Application.platform)
        //{
        //    case RuntimePlatform.WindowsPlayer:
        //    case RuntimePlatform.WindowsEditor:
        //        return "windows";
        //    case RuntimePlatform.Android:
        //        return "android";
        //    case RuntimePlatform.OSXPlayer:
        //        return "ios";
        //    default:
        //        return null;
        //}
    }


    /// <summary>
    /// 获取WWW协议的路径
    /// </summary>
    public static string GetWWWPath()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                return "file:///" + GetAssetBundleOutPath();
            case RuntimePlatform.Android:
                //return "jar:file://" + GetAssetBundleOutPath();
                return "file:///" + GetAssetBundleOutPath();
            default:
                return null;
        }
    }
}
