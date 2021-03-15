using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    public static class LogicUtils
    {
        private static Texture2D scriptIcon = (EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D);

        #region 拓展
        /// <summary>
        /// 加载并添加样式
        /// </summary>
        /// <param name="visualElement"></param>
        /// <param name="sheetPath">样式路径</param>
        public static void LoadAndAddStyleSheet(this VisualElement visualElement, string sheetPath)
        {
            StyleSheet styleSheet = Resources.Load(sheetPath, typeof(StyleSheet)) as StyleSheet;
            if ((UnityEngine.Object)styleSheet == (UnityEngine.Object)null)
                Debug.LogWarning((object)string.Format("Style sheet not found for path \"{0}\"", (object)sheetPath));
            else
                visualElement.styleSheets.Add(styleSheet);
        }

        /// <summary>
        /// 根据窗体获取位置
        /// </summary>
        /// <param name="window"></param>
        /// <param name="localPos">本地位置</param>
        public static Vector2 GetScreenPosition(this UnityEditor.EditorWindow window, Vector2 localPos)
        {
            return window.position.position + localPos;
        }
        #endregion

        /// <summary>
        /// 刷新
        /// </summary>
        [MenuItem("Framework/逻辑图/刷新逻辑图")]
        private static void RefreshLogic()
        {
            LogicEditorConfig.Refresh();
            LogicEditorConfig.ConfigData.LogicGraphs.Clear();
            string[] strs = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);
            foreach (var item in strs)
            {
                string fileName = item.Replace(Application.dataPath, "Assets");
                LogicGraphBase logicGraph = AssetDatabase.LoadAssetAtPath<LogicGraphBase>(fileName);
                if (logicGraph)
                {

                    LogicEditorConfigData.LogicGraphSummary summary = new LogicEditorConfigData.LogicGraphSummary();
                    summary.AssetPath = fileName.Replace('\\', '/');
                    summary.FileName = Path.GetFileNameWithoutExtension(summary.AssetPath);
                    summary.LogicName = logicGraph.LogicGraphData.Title;
                    summary.GraphClassName = logicGraph.GetType().FullName;
                    LogicEditorConfig.ConfigData.LogicGraphs.Add(summary);
                }
            }
            EditorUtility.SetDirty(LogicEditorConfig.ConfigData);
            AssetDatabase.SaveAssets();

        }

        /// <summary>
        /// 创建节点
        /// </summary>
        [MenuItem("Assets/Create/LogicGraph/Node C# Script", false, 89)]
        private static void CreateNode()
        {
            string[] guids = AssetDatabase.FindAssets("LogicNodeTemplate.cs");
            if (guids.Length == 0)
            {
                Debug.LogWarning("LogicNodeTemplate.cs.txt not found in asset database");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CreateFromTemplate<DoCreateNodeCodeFile>(
                "NewNode.cs",
                path
            );
        }
        /// <summary>
        /// 创建逻辑图
        /// </summary>
        [MenuItem("Assets/Create/LogicGraph/Graph C# Script", false, 89)]
        private static void CreateGraph()
        {
            string[] guids = AssetDatabase.FindAssets("LogicGraphTemplate.cs");
            if (guids.Length == 0)
            {
                Debug.LogWarning("LogicNodeTemplate.cs.txt not found in asset database");
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CreateFromTemplate<DoCreateGraphCodeFile>(
                "NewGraph.cs",
                path
            );
        }
        public static void CreateFromTemplate<T>(string initialName, string templatePath) where T : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<T>(),
                initialName,
                scriptIcon,
                templatePath
            );
        }

        /// <summary>
        /// 创建脚本
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="templatePath">模板路径</param>
        /// <returns></returns>
        internal static UnityEngine.Object CreateScript(string className, string pathName, string templatePath)
        {
            string templateText = string.Empty;

            UTF8Encoding encoding = new UTF8Encoding(true, false);

            if (File.Exists(templatePath))
            {
                /// Read procedures.
                StreamReader reader = new StreamReader(templatePath);
                templateText = reader.ReadToEnd();
                reader.Close();

                templateText = templateText.Replace("{CLASS_NAME}", className);

                StreamWriter writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
                writer.Write(templateText);
                writer.Close();

                AssetDatabase.ImportAsset(pathName);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
            }
            else
            {
                Debug.LogError(string.Format("The template file was not found: {0}", templatePath));
                return null;
            }
        }
        private class DoCreateGraphCodeFile : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Object o = CreateScript(Path.GetFileNameWithoutExtension(pathName).Replace(" ", string.Empty), pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }
        private class DoCreateNodeCodeFile : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string className = Path.GetFileNameWithoutExtension(pathName).Replace(" ", string.Empty);
                Object o = CreateScript(className, pathName, resourceFile);
                string fileName = Path.GetFileNameWithoutExtension(pathName);
                string tempPath = Path.Combine(Path.GetDirectoryName(resourceFile), "LogicNodeViewTemplate.cs.txt");
                string viewPath = Path.Combine(Path.GetDirectoryName(pathName), $"{fileName}View.cs");
                CreateScript(className, viewPath, tempPath);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }
    }
}
