using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Logic.Editor
{
    /// <summary>
    /// 每次资源变化调用
    /// </summary>
    public class LogicPostprocessor : AssetPostprocessor
    {
        [UnityEditor.Callbacks.DidReloadScripts()]
        static void OnScriptReload()
        {
            LogicEditorConfig.Refresh();
        }

        /// <summary>
        /// 所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
        /// </summary>
        /// <param name="importedAsset">导入的资源</param>
        /// <param name="deletedAssets">删除的资源</param>
        /// <param name="movedAssets">移动后资源路径</param>
        /// <param name="movedFromAssetPaths">移动前资源路径</param>
        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var str in movedFromAssetPaths)
            {
                //移动前资源路径
                string ext = Path.GetExtension(str);
                if (ext == ".asset")
                {
                    LogicEditorConfig.RemoveLogicGraph(str);
                }
            }
            foreach (var str in movedAssets)
            {
                //移动后资源路径
                string ext = Path.GetExtension(str);
                if (ext == ".asset")
                {
                    LogicEditorConfig.AddLogicGraph(str);
                    //Debug.LogError("importedAsset:" + str);
                }
            }

            foreach (string str in importedAsset)
            {
                string ext = Path.GetExtension(str);
                if (ext == ".asset")
                {
                    LogicEditorConfig.AddLogicGraph(str);
                    //Debug.LogError("importedAsset:" + str);
                }
            }

            foreach (string str in deletedAssets)
            {
                string ext = Path.GetExtension(str);
                if (ext == ".asset")
                {
                    LogicEditorConfig.RemoveLogicGraph(str);
                }
            }
        }
    }
}
