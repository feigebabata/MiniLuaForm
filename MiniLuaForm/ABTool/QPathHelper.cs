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

		if (!Directory.Exists (outPath)) 
		{
			Directory.CreateDirectory(outPath);
		}

        return outPath;
	}    

	/// <summary>
	/// 自动获取对应平台的路径
	/// </summary>
	/// <returns></returns>
	public static string GetPlatformPath()
	{
		string str="";
		#if UNITY_EDITOR
		str=Application.streamingAssetsPath;
		return str;
		#elif UNITY_ANDROID||UNITY_IOS
		str=Application.persistentDataPath;
		return str;
		#else
		return null;
		#endif

	}

	/// <summary>
	/// 获取对应平台的名字
	/// </summary>
	/// <returns></returns>
	public static string GetPlatformName()
	{
		#if UNITY_IOS
		return "ios";
		#elif UNITY_ANDROID
		return "android";
		#else
		return null;
		#endif
	}


    /// <summary>
    /// 获取WWW协议的路径
    /// </summary>
    public static string GetWWWPath()
    {
		return "file:///" + GetAssetBundleOutPath();
    }

	public static void ClearABDir()
	{
		string abPath = GetPlatformPath() + "/" + GetPlatformName();
		Directory.Delete (abPath);
		Directory.CreateDirectory (abPath);
	}
}

/// <summary>
/// 文件夹操作类
/// </summary>
public static class FolderHelper
{
	public static void CopyDirectory(string srcPath, string destPath)
	{
		Debug.Log ("源文件夹："+srcPath);
		Debug.Log ("目标文件夹："+destPath);
		if (Directory.Exists (srcPath)) 
		{
			Debug.Log ("存在："+srcPath);
		}
		else 
		{
			Debug.Log ("不存在："+srcPath);
		}
		if (Directory.Exists (destPath)) 
		{
			Debug.Log ("存在："+destPath);
		}
		else 
		{
			Debug.Log ("不存在："+destPath);
		}
		Debug.Log (1);
		DirectoryInfo dir = new DirectoryInfo(srcPath);
		Debug.Log (2);
		FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
		Debug.Log (3);
		foreach (FileSystemInfo i in fileinfo)
		{
			Debug.Log (4);
			if (i is DirectoryInfo)     //判断是否文件夹
			{
				Debug.Log (5);
				if (!Directory.Exists(destPath+"\\"+i.Name))
				{
					Directory.CreateDirectory(destPath + "\\" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
				}
				CopyDirectory(i.FullName, destPath + "\\" + i.Name);    //递归调用复制子文件夹
			}
			else
			{
				Debug.Log (6);
				File.Copy(i.FullName, destPath + "\\" + i.Name,true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
			}
		}
	}
}