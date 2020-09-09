using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Unity.Jobs;
using UnityEditor.SceneManagement;

namespace NoiseTexGenerator
{
    public class NoiseTexGeneratorUI : EditorWindow
    {
        private SaveTextureUtils texUtils = new SaveTextureUtils();

        private string[] texDimensions = new string[2] { "2D", "3D" };
        private int selectedTexDimension = 0; //0 = 2D, 1 = 3D

        private Vector2Int texSize2D;
        private Vector3Int texSize3D;

        /* GUI LABEL STYLES */
        //these can't be initialised inline because they're ScriptableObjects??
        private GUIStyle headerLabelStyle = new GUIStyle();
        private void InitHeaderLabelStyle()
        {
            headerLabelStyle.fontSize = 14;
            //headerLabelStyle.fontStyle = FontStyle.Bold;
        }

        [MenuItem("Window/NoiseTexGenerator")]
        public static void OpenUI()
        {
            GetWindow<NoiseTexGeneratorUI>("Noise Texture Generator");
        }

        void OnEnable()
        {
            InitHeaderLabelStyle();
            //InitSubheaderLabelStyle();
        }

        void OnGUI()
        {
            //EditorGUIUtility.labelWidth = 100;
            //EditorGUIUtility.fieldWidth = 10;
            //minSize = new Vector2(264, 512);
            
            selectedTexDimension = EditorGUILayout.Popup("Texture dimension", selectedTexDimension, texDimensions);
            if(selectedTexDimension == 0) //2D
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Vector2IntField("Texture size", texSize2D);
                if (EditorGUI.EndChangeCheck())
                {
                    texSize2D.x = FindNearestPowerOf2(texSize2D.x);
                    texSize2D.y = FindNearestPowerOf2(texSize2D.y);
                }

                if(GUILayout.Button("Generate noise texture"))
                {
                    //create and save new (black i.e. all base texture) blend texture
                    string newTexPath = EditorUtility.SaveFilePanelInProject("Save new blend map", "BlendTex.png", "png", "");
                    string newTexName = Path.GetFileName(newTexPath);
                    string newTexDirectory = Path.GetDirectoryName(newTexPath);
                    string newTexAsset = texUtils.CreateAndSaveTex2D
                    (
                        texSize2D,
                        newTexDirectory,
                        newTexName
                    );
                }
                
            }
            else //3D
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Vector3IntField("Texture size", texSize3D);
                if (EditorGUI.EndChangeCheck())
                {
                    texSize3D.x = FindNearestPowerOf2(texSize3D.x);
                    texSize3D.y = FindNearestPowerOf2(texSize3D.y);
                    texSize3D.z = FindNearestPowerOf2(texSize3D.z);
                }

                if (GUILayout.Button("Generate noise texture"))
                {
                    //create and save new (black i.e. all base texture) blend texture
                    string newTexPath = EditorUtility.SaveFilePanelInProject("Save new blend map", "BlendTex.png", "png", "");
                    string newTexName = Path.GetFileName(newTexPath);
                    string newTexDirectory = Path.GetDirectoryName(newTexPath);
                    string newTexAsset = texUtils.CreateAndSaveTex3D
                    (
                        texSize3D,
                        newTexDirectory,
                        newTexName
                    );
                }
                
            }

                
        }

        private int FindNearestPowerOf2(int n)
        {
            if (IsPowerOf2(n)) return n;
            if (n <= 0) return 1;

            //get the powers of 2 immediately below and above n 
            int upperPower = 1;
            while (upperPower < n)
            {
                upperPower *= 2;
            }
            int lowerPower = upperPower / 2;

            int lowerDiff = n - lowerPower;
            int upperDiff = upperPower - n;

            return lowerDiff < upperDiff ? lowerPower : upperPower;
        }

        private bool IsPowerOf2(int n)
        {
            return n != 0 && (n & n - 1) == 0;
        }
    }
}