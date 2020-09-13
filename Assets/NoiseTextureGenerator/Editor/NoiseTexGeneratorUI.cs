using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

namespace NoiseTexGenerator
{
    public class NoiseTexGeneratorUI : EditorWindow
    {
        private SaveTextureUtils texUtils = new SaveTextureUtils();
        private TexGenerator texGenerator;
        private Texture generatedTex;

        [SerializeField] private string[] texDimensions = new string[2] { "2D", "3D" };
        [SerializeField] private int selectedTexDimension = 0; //0 = 2D, 1 = 3D

        [SerializeField] [Min(1)] private int texWidth = 256, texHeight = 256, texDepth = 256;
        [SerializeField] private float noiseOffset = 0f, noiseMultiplier = 0.01f, noiseIntensity = 1f;

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
            //initialise tex generator compute shaders and script
            ComputeShader texGeneratorCompute2D = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/NoiseTextureGenerator/NoiseTexGenerator2D.compute");
            ComputeShader texGeneratorCompute3D = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/NoiseTextureGenerator/NoiseTexGenerator3D.compute");
            texGenerator = new TexGenerator(texGeneratorCompute2D, texGeneratorCompute3D);

            InitHeaderLabelStyle();
            //InitSubheaderLabelStyle();
        }

        void OnGUI()
        {
            EditorGUIUtility.labelWidth = 10;
            EditorGUIUtility.fieldWidth = 10;
            //minSize = new Vector2(264, 512);

            selectedTexDimension = EditorGUILayout.Popup("Texture dimension", selectedTexDimension, texDimensions);
            if(selectedTexDimension == 0) //2D
            {
                EditorGUI.BeginChangeCheck(); //changing any parameters causes the texture to be regenerated

                EditorGUILayout.LabelField("Texture size");
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 10;
                texWidth = FindNearestPowerOf2(EditorGUILayout.IntField("x", texWidth));
                texHeight = FindNearestPowerOf2(EditorGUILayout.IntField("y", texHeight));
                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = 100;
                noiseOffset = EditorGUILayout.FloatField("Offset", noiseOffset);
                noiseMultiplier = EditorGUILayout.FloatField("Multiplier", noiseMultiplier);
                noiseIntensity = EditorGUILayout.Slider("Intensity", noiseIntensity, 0f, 1f);

                if (GUILayout.Button("Save generated texture"))
                {
                    string newTexPath = EditorUtility.SaveFilePanelInProject("Save new blend map", "Noise.png", "png", "");
                    string newTexName = Path.GetFileName(newTexPath);
                    string newTexDirectory = Path.GetDirectoryName(newTexPath);

                    texUtils.SaveTexture((Texture2D)generatedTex, newTexDirectory, newTexName);
                }

                if(EditorGUI.EndChangeCheck())
                {
                    generatedTex = texGenerator.GenerateTexture2D(new Vector2Int(texWidth, texHeight), noiseMultiplier, noiseOffset, noiseIntensity);
                }

                //draw texture preview
                Rect texPreviewRect = EditorGUILayout.GetControlRect(false, 128, GUILayout.MinHeight(32), GUILayout.MaxHeight(texWidth), GUILayout.MinWidth(32), GUILayout.MaxWidth(texHeight));
                EditorGUI.DrawPreviewTexture(texPreviewRect, generatedTex);
            }
            else //3D
            {
                EditorGUILayout.LabelField("Texture size");

                EditorGUILayout.BeginHorizontal();
                texWidth = FindNearestPowerOf2(EditorGUILayout.IntField("x", texWidth));
                texHeight = FindNearestPowerOf2(EditorGUILayout.IntField("y", texHeight));
                texDepth = FindNearestPowerOf2(EditorGUILayout.IntField("z", texDepth));
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Generate noise texture"))
                {
                    string newTexPath = EditorUtility.SaveFilePanelInProject("Save new blend map", "BlendTex.png", "png", "");
                    string newTexName = Path.GetFileName(newTexPath);
                    string newTexDirectory = Path.GetDirectoryName(newTexPath);
                    string newTexAsset = texUtils.CreateAndSaveTex3D
                    (
                        texWidth,
                        texHeight,
                        texDepth,
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