using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MoonSharp.Interpreter.Loaders
{
    /// <summary>
    /// A script loader which can load scripts from assets in Unity3D.
    /// Scripts should be saved as .txt files in a subdirectory of Assets/Resources.
    /// 
    /// When MoonSharp is activated on Unity3D and the default script loader is used,
    /// scripts should be saved as .txt files in Assets/Resources/MoonSharp/Scripts.
    /// </summary>
    public class UnityAssetsScriptLoader : ScriptLoaderBase
    {
        Dictionary<string, string> m_Resources = new Dictionary<string, string>();

        /// <summary>
        /// The default path where scripts are meant to be stored (if not changed)
        /// </summary>
        public const string DEFAULT_PATH = "MoonSharp/Scripts";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityAssetsScriptLoader"/> class.
        /// </summary>
        /// <param name="assetsPath">The path, relative to Assets/Resources. For example
        /// if your scripts are stored under Assets/Resources/Scripts, you should
        /// pass the value "Scripts". If null, "MoonSharp/Scripts" is used. </param>
        public UnityAssetsScriptLoader(string assetsPath = null)
        {
            assetsPath = assetsPath ?? DEFAULT_PATH;
            LoadResources(assetsPath);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UnityAssetsScriptLoader"/> class.
        /// </summary>
        /// <param name="scriptToCodeMap">A dictionary mapping filenames to the proper Lua script code.</param>
        public UnityAssetsScriptLoader(Dictionary<string, string> scriptToCodeMap)
        {
            m_Resources = scriptToCodeMap;
        }

        void LoadResources(string assetsPath)
        {
#if UNITY_WEBGL
            UnityEngine.TextAsset[] array = UnityEngine.Resources.LoadAll<UnityEngine.TextAsset>(assetsPath);
            for (int i = 0; i < array.Length; i++)
            {
                UnityEngine.TextAsset text = array[i];
                m_Resources.Add(text.name, text.text);
            }
#else
            LoadResourcesWithReflection(assetsPath);
#endif
        }

        void LoadResourcesWithReflection(string assetsPath)
        {
            try
            {
                Type resourcesType = Type.GetType("UnityEngine.Resources, UnityEngine");
                Type textAssetType = Type.GetType("UnityEngine.TextAsset, UnityEngine");

                MethodInfo textAssetNameGet = textAssetType.GetProperty("name").GetGetMethod();
                MethodInfo textAssetTextGet = textAssetType.GetProperty("text").GetGetMethod();

                MethodInfo loadAll = resourcesType.GetMethod("LoadAll",
                    new Type[] { typeof(string), typeof(Type) });

                Array array = (Array)loadAll.Invoke(null, new object[] { assetsPath, textAssetType }); //Unity5.4 + WebGL, it will crash in web browser

                for (int i = 0; i < array.Length; i++)
                {
                    object o = array.GetValue(i);

                    string name = textAssetNameGet.Invoke(o, null) as string;
                    string text = textAssetTextGet.Invoke(o, null) as string;

                    m_Resources.Add(name, text);
                }
            }
            catch (Exception ex)
            {
#if !PCL
                Console.WriteLine("Error initializing UnityScriptLoader : {0}", ex);
#endif
            }
        }

        private string GetFileName(string filename)
        {
            int b = Math.Max(filename.LastIndexOf('\\'), filename.LastIndexOf('/'));

            if (b > 0)
                filename = filename.Substring(b + 1);

            return filename;
        }

        /// <summary>
        /// Opens a file for reading the script code.
        /// It can return either a string, a byte[] or a Stream.
        /// If a byte[] is returned, the content is assumed to be a serialized (dumped) bytecode. If it's a string, it's
        /// assumed to be either a script or the output of a string.dump call. If a Stream, autodetection takes place.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="globalContext">The global context.</param>
        /// <returns>
        /// A string, a byte[] or a Stream.
        /// </returns>
        /// <exception cref="System.Exception">UnityAssetsScriptLoader.LoadFile : Cannot load  + file</exception>
        public override object LoadFile(string file, Table globalContext)
        {
            file = GetFileName(file);

            if (m_Resources.ContainsKey(file))
                return m_Resources[file];
            else
            {
                var error = string.Format(
@"Cannot load script '{0}'. By default, scripts should be .txt files placed under a Assets/Resources/{1} directory.
If you want scripts to be put in another directory or another way, use a custom instance of UnityAssetsScriptLoader or implement
your own IScriptLoader (possibly extending ScriptLoaderBase).", file, DEFAULT_PATH);

                throw new Exception(error);
            }
        }

        /// <summary>
        /// Checks if a given file exists
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public override bool ScriptFileExists(string file)
        {
            file = GetFileName(file);
            return m_Resources.ContainsKey(file);
        }


        /// <summary>
        /// Gets the list of loaded scripts filenames (useful for debugging purposes).
        /// </summary>
        /// <returns></returns>
        public string[] GetLoadedScripts()
        {
            return m_Resources.Keys.ToArray();
        }



    }
}
