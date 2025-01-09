using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

public class RC_AvatarVenv : EditorWindow
{
    private VRCAvatarDescriptor targetAvatar;
    private string targetPath; // 복사할 Material을 찾을 경로
    private bool isCopySubmenus;

    [MenuItem("ReraC/Avatar Venv System")]
    public static void ShowWindow()
    {
        RC_AvatarVenv window = GetWindow<RC_AvatarVenv>();
        window.titleContent = new GUIContent("Avatar Venv System");
        window.Show();
    }

    private bool mkdir(string path)
    {
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            if (isCopySubmenus)
            {
                Directory.CreateDirectory(path + "/submenus");
            }
            /*Directory.CreateDirectory(path + "/mat");
            Directory.CreateDirectory(path + "/vrc");*/

            
            return true;
        }
        return false;

    }
    private void OnGUI()
    {
        GUI.skin.label.fontSize = 25;
        GUILayout.Label("Avatar Venv System.");
        GUI.skin.label.fontSize = 10;

        GUI.skin.label.alignment = TextAnchor.MiddleRight;
        GUILayout.Label("V1.5 by Rera*C");
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

        EditorGUILayout.Space(10);
        targetAvatar = EditorGUILayout.ObjectField("Target GameObject", targetAvatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
        if (targetAvatar == null)
        {
            return;
        }
        var targetGameObject = targetAvatar.gameObject;
        targetPath = EditorGUILayout.TextField("Target Path", targetPath);

        //checkbox
        isCopySubmenus = EditorGUILayout.Toggle("Copy Submenus", isCopySubmenus);


        EditorGUILayout.Space(10);
        if (GUILayout.Button("Make Venv"))
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is not set.");
                return;
            }

            if (string.IsNullOrEmpty(targetPath))
            {
                Debug.LogError("Target Path is not set.");
                return;
            }
            if (mkdir("Assets/" + targetPath))
            {
                CopyVRChatExpressions(targetAvatar, "Assets/" + targetPath);
                //CopyVRChatExpressions(targetAvatar, "Assets/" + targetPath + "/vrc");
                //CopyMaterialsRecursive(targetGameObject, "Assets/" + targetPath + "/mat");

                AssetDatabase.Refresh();
            }
            Debug.Log("End");

            //messagebox

        }
    }

    private VRCExpressionsMenu ProcessSubmenu(string path, VRCExpressionsMenu menu)
    {
        Debug.Log("Processing Submenu : " + menu.name);

        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(menu), path + "/" + menu.name + ".asset");
        VRCExpressionsMenu processedMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(path + "/" + menu.name + ".asset");

        Directory.CreateDirectory(path + "/" + processedMenu.name);
        for (int i = 0; i < processedMenu.controls.Count; i++)
        {
            if (processedMenu.controls[i].type == VRCExpressionsMenu.Control.ControlType.SubMenu)
            {
                processedMenu.controls[i].subMenu = ProcessSubmenu(path + "/" + processedMenu.name, processedMenu.controls[i].subMenu);
            }
        }

        EditorUtility.SetDirty(processedMenu);
        AssetDatabase.SaveAssets();

        return processedMenu;
    }

    private void CopyVRChatExpressions(VRCAvatarDescriptor ad, string path)
    {
        // ad.baseAnimationLayers[].animatorController
        // ad.specialAnimationLayers[].animatorController
        // ad.expressionsMenu
        // ad.expressionsParameter

        if (ad != null)
        {
            for (int i = 0; i < ad.baseAnimationLayers.Length; i++)
            {
                    //asset.copy 이용
                    if (ad.baseAnimationLayers[i].animatorController != null)
                    {
                        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ad.baseAnimationLayers[i].animatorController), path + "/" + ad.baseAnimationLayers[i].animatorController.name + ".controller");
                        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path + "/" + ad.baseAnimationLayers[i].animatorController.name + ".controller");
                        EditorUtility.SetDirty(controller);
                        AssetDatabase.SaveAssets();

                        ad.baseAnimationLayers[i].animatorController = controller;
                    }
            }

            for (int i = 0; i < ad.specialAnimationLayers.Length; i++)
            {
                    //asset.copy 이용
                    if (ad.specialAnimationLayers[i].animatorController != null)
                    {
                        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ad.specialAnimationLayers[i].animatorController), path + "/" + ad.specialAnimationLayers[i].animatorController.name + ".controller");
                        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path + "/" + ad.specialAnimationLayers[i].animatorController.name + ".controller");
                        EditorUtility.SetDirty(controller);
                        AssetDatabase.SaveAssets();

                        ad.specialAnimationLayers[i].animatorController = controller;
                    }
            }

            if (ad.expressionsMenu != null)
            {
                //asset.copy 이용
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ad.expressionsMenu), path + "/" + ad.expressionsMenu.name + ".asset");
                VRCExpressionsMenu menu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(path + "/" + ad.expressionsMenu.name + ".asset");
                if (isCopySubmenus)
                {
                    for (int i = 0; i<menu.controls.Count; i++)
                    {
                        if(menu.controls[i].type == VRCExpressionsMenu.Control.ControlType.SubMenu)
                        {
                            menu.controls[i].subMenu = ProcessSubmenu(path + "/submenus", menu.controls[i].subMenu);
                        }
                    }
                }

                EditorUtility.SetDirty(menu);
                AssetDatabase.SaveAssets();

                
                ad.expressionsMenu = menu;
            }

            if (ad.expressionParameters != null)
            {
                //asset.copy 이용
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ad.expressionParameters), path + "/" + ad.expressionParameters.name + ".asset");
                VRCExpressionParameters param = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(path + "/" + ad.expressionParameters.name + ".asset");
                EditorUtility.SetDirty(param);
                AssetDatabase.SaveAssets();

                ad.expressionParameters = param;
            }
        }

    }

    private void CopyMaterialsRecursive(GameObject go, string path)
    {
        //Broken

        // 현재 GameObject의 Renderer 컴포넌트를 가져옴
        Renderer renderer = go.GetComponent<Renderer>();

        if (renderer != null)
        {
            //renderer.sharedMaterials
            for(int i=0; i<renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] != null)
                {
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(renderer.sharedMaterials[i]), path + "/" + renderer.sharedMaterials[i].name + ".mat");
                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(path + "/" + renderer.sharedMaterials[i].name + ".mat");
                    EditorUtility.SetDirty(mat);
                    AssetDatabase.SaveAssets();

                    renderer.sharedMaterials[i] = mat;
                }
            }
            
        }

        // 하위 GameObject들에 대해 재귀적으로 호출
        for (int i = 0; i < go.transform.childCount; i++)
        {
            CopyMaterialsRecursive(go.transform.GetChild(i).gameObject, path);
        }
    }
}
