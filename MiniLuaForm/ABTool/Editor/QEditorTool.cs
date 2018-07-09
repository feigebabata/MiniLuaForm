using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class QEditorTool
{
	static string Modules = "hall";//当前打包模块名称
	static BuildTarget target = BuildTarget.iOS;

    [MenuItem("QTool/Data/清除数据")]
    static void ClearData()
    {
        PlayerPrefs.DeleteAll();
    }

    #region AB相关
    [MenuItem("QTool/AB/Set Label")]
    static void AutoSetLabel()
    {
        //移除所有没有使用的标记
        AssetDatabase.RemoveUnusedAssetBundleNames();

        //1.找到资源保存的文件夹
		string assetDirectory = Application.dataPath + "/ABRes/"+Modules;
        //Debug.Log(assetDirectory);

        DirectoryInfo directoryInfo = new DirectoryInfo(assetDirectory);
        DirectoryInfo[] sceneDirectories = directoryInfo.GetDirectories();
        //2.遍历里面的每个场景文件夹
        foreach (DirectoryInfo tmpDirectoryInfo in sceneDirectories)
        {
            string sceneDirectory = assetDirectory + "/" + tmpDirectoryInfo.Name;
            DirectoryInfo sceneDirectoryInfo = new DirectoryInfo(sceneDirectory);
            //错误检测
            if (sceneDirectoryInfo == null)
            {
                Debug.LogError(sceneDirectory + " 不存在!");
                return;
            }
            else
            {
                Dictionary<string, string> namePahtDict = new Dictionary<string, string>();

                //3.遍历场景文件夹里的所有文件系统
                //sceneDirectory
                int index = sceneDirectory.LastIndexOf("/");
                string sceneName = sceneDirectory.Substring(index + 1);
                onSceneFileSystemInfo(sceneDirectoryInfo, sceneName, namePahtDict);
            }
        }

        AssetDatabase.Refresh();

		Debug.Log(Modules+"设置成功");
    }
    [MenuItem("QTool/AB/Bundle AB")]
    static void BundleAndroidAB()
    {
        string outPath = QPathHelper.GetAssetBundleOutPath();
//        Debug.Log("path: " + outPath);
		BuildPipeline.BuildAssetBundles(outPath, 0, target);
		Debug.Log("打包成功");
        AssetDatabase.Refresh();
    }
    [MenuItem("QTool/AB/Creat AB Config")]
    static void CreatFiles()
    {
        //创建各个文件的版本标识
		string outPath = QPathHelper.GetAssetBundleOutPath()+"/" +Modules;
		string file = outPath +"/files.txt";

        if (File.Exists(file))
            File.Delete(file);

        List<string> pathList = new List<string>();

        DirectoryInfo Direinfo = new DirectoryInfo(outPath);
		GetAllFileName(Direinfo, ref pathList, "concretegame", new List<string>() { "ModulesConfig.txt","concretegameConfig.txt"});

        FileStream fs = new FileStream(file, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
		Hashtable jd = new Hashtable ();
		Hashtable files = new Hashtable ();
        foreach (var info in pathList)
        {
            string md5 = GetFileMd5(info);

            string fileName = info.Replace(outPath + "/", string.Empty);
			fileName = Modules+"/"+fileName;
			files[fileName] = md5;
        }
		files[Modules+"/files.txt"]=System.DateTime.Now.ToString();
		files["ModulesConfig.txt"] = System.DateTime.Now.ToString();
		jd ["files"] = files;
		sw.Write (MiniJSON.jsonEncode(jd));
        sw.Close();
        fs.Close();

		Debug.Log(Modules+"创建文本标识成功");
        AssetDatabase.Refresh();
    }
    [MenuItem("QTool/AB/Del Ab")]
    static void DelAllRes()
    {
        ClearAllInfoDown();

        Debug.Log("删除成功");

        AssetDatabase.Refresh();
	}

	/// <summary>
	/// 删除所有可下载信息
	/// </summary>
	public static void ClearAllInfoDown()
	{
		string outPath =QPathHelper.GetAssetBundleOutPath()+"/"+Modules;
		Directory.Delete(outPath, true);
		File.Delete(outPath + ".meta");
	}
    /// <summary>
    /// 遍历场景文件夹里的所有文件系统
    /// </summary>
    private static void onSceneFileSystemInfo(FileSystemInfo fileSystemInfo, string sceneName, Dictionary<string, string> namePahtDict)
    {
        if (!fileSystemInfo.Exists)
        {
            Debug.LogError(fileSystemInfo.FullName + " 不存在!");
            return;
        }

        DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
        FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
        foreach (var tmpFileSystemInfo in fileSystemInfos)
        {
            FileInfo fileInfo = tmpFileSystemInfo as FileInfo;
            if (fileInfo == null)
            {
                //代表强转失败，不是文件 就是文件夹
                //如果访问的是文件夹：再继续访问里面的所有文件系统，直到找到 文件 （递归）
                onSceneFileSystemInfo(tmpFileSystemInfo, sceneName, namePahtDict);
            }
            else
            {
                //就是文件
                //5.找到文件 就要修改他的 assetbundle labels
                setLabels(fileInfo, sceneName, namePahtDict);
            }
        }
    }

    /// <summary>
    /// 修改资源文件的 assetbundle labels
    /// </summary>
    private static void setLabels(FileInfo fileInfo, string sceneName, Dictionary<string, string> namePahtDict)
    {
        //对unity自身生成的meta文件 无视它
        if (fileInfo.Extension == ".meta")
            return;

		string bundleName =Modules+"/"+ getBundleName(fileInfo, sceneName);
        int index = fileInfo.FullName.IndexOf("Assets");
        string assetPath = fileInfo.FullName.Substring(index);
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
        //用 AssetImporter 类 修改名称和后缀
        assetImporter.assetBundleName = bundleName.ToLower();
        if (fileInfo.Extension == ".unity")
            assetImporter.assetBundleVariant = "u3d";
        else
            assetImporter.assetBundleVariant = "assetbundle";

        string folderName = "";
        //添加到字典里
        if (bundleName.Contains("/"))
            folderName = bundleName.Split('/')[1];
        else
            folderName = bundleName;

        string bundlePath = assetImporter.assetBundleName + "." + assetImporter.assetBundleVariant;
        if (!namePahtDict.ContainsKey(folderName))
            namePahtDict.Add(folderName, bundlePath);
    }

    /// <summary>
    /// 获取包名
    /// </summary>
    private static string getBundleName(FileInfo fileInfo, string sceneName)
    {
        string windowsPath = fileInfo.FullName;
        //转换成unity可识别的路径
        string unityPath = windowsPath.Replace(@"\", "/");

        int index = unityPath.IndexOf(sceneName) + sceneName.Length;

        string bundlePath = unityPath.Substring(index + 1);

        if (bundlePath.Contains("/"))
        {
            string[] tmp = bundlePath.Split('/');
            return sceneName + "/" + tmp[0];
        }
        else
        {
            //Scene1.unity
            return sceneName;
        }
    }

        //获得_Info下面所有文件的文件全路径
    static void GetAllFileName(FileSystemInfo _Info, ref List<string> _PathList,string _NoFolderName="",List<string> _NoFileName=null)
    {
        DirectoryInfo direInfo = _Info as DirectoryInfo;

        FileSystemInfo[] infos = direInfo.GetFileSystemInfos();

        for (int i = 0; i < infos.Length; i++)
        {
            FileInfo info = infos[i] as FileInfo;

            if (info == null)
            {
                if (!infos[i].Name.Equals(_NoFolderName))
                {
                    //——NoName下面的文件夹不标记
                    GetAllFileName(infos[i], ref _PathList, _NoFolderName, _NoFileName);
                }
            }
            else
            {
                string fullName = info.FullName;
                string nofilename = info.Name;

                string str = Path.GetExtension(fullName);
                fullName = fullName.Replace("\\", "/");
                if (!str.Equals(".meta"))
                {
                    if (_NoFileName == null || !_NoFileName.Contains(nofilename))
                    {
                        _PathList.Add(fullName);
                    }
                }
            }
        }

    }

    static string GetFileMd5(string _Path)
    {
        FileStream fs = new FileStream(_Path, FileMode.Open);
        long size = fs.Length;
        byte[] byteArray = new byte[size];
        fs.Read(byteArray, 0, byteArray.Length);

        fs.Close();

        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] outPut = md5.ComputeHash(byteArray);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < outPut.Length; i++)
        {
            sb.Append(outPut[i].ToString("x2"));
        }
        return sb.ToString();
    } 
    #endregion

}
