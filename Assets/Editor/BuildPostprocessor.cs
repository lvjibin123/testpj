/**
 * 文件名:BuildPostprocessor.cs
 * Des:在导出Android工程之后对assets/bin/Data/Managed/Assembly-CSharp.dll进行加密
 * **/
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class BuildPostprocessor
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.Android && (!pathToBuiltProject.EndsWith(".apk")))
        {
            Debug.Log("target: " + target.ToString());
            Debug.Log("pathToBuiltProject: " + pathToBuiltProject);
            Debug.Log("productName: " + PlayerSettings.productName);

            string dllPath = pathToBuiltProject + "/" + PlayerSettings.productName + "/" + "assets/bin/Data/Managed/Assembly-CSharp.dll";

            if (File.Exists(dllPath))
            {
                //加密 Assembly-CSharp.dll;
                Debug.Log("Encrypt assets/bin/Data/Managed/Assembly-CSharp.dll Start");

                byte[] bytes = File.ReadAllBytes(dllPath);
                bytes[0] += 1;
                File.WriteAllBytes(dllPath, bytes);

                Debug.Log("Encrypt assets/bin/Data/Managed/Assembly-CSharp.dll Success");

                Debug.Log("Encrypt libmono.so Start !!");

                Debug.Log("Current is : " + EditorUserBuildSettings.development.ToString());

                //替换 libmono.so;
                if (EditorUserBuildSettings.development)
                {
                    string armv7a_so_path = pathToBuiltProject + "/" + PlayerSettings.productName + "/" + "libs/armeabi-v7a/libmono.so";
                    File.Copy(Application.dataPath + "/Editor/libs/development/armeabi-v7a/libmono.so", armv7a_so_path, true);

                    string x86_so_path = pathToBuiltProject + "/" + PlayerSettings.productName + "/" + "libs/x86/libmono.so";
                    File.Copy(Application.dataPath + "/Editor/libs/development/x86/libmono.so", x86_so_path, true);
                }
                else
                {
                    string armv7a_so_path = pathToBuiltProject + "/" + PlayerSettings.productName + "/" + "libs/armeabi-v7a/libmono.so";
                    File.Copy(Application.dataPath + "/Editor/libs/release/armeabi-v7a/libmono.so", armv7a_so_path, true);

                    string x86_so_path = pathToBuiltProject + "/" + PlayerSettings.productName + "/" + "libs/x86/libmono.so";
                    File.Copy(Application.dataPath + "/Editor/libs/release/x86/libmono.so", x86_so_path, true);
                }

                Debug.Log("Encrypt libmono.so Success !!");
            }
            else
            {
                Debug.LogError(dllPath+ "  Not Found!!");
            }
        }
    }
}