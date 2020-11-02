using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NoiseTexGenerator
{
    public class SaveTextureUtils
    {
        
        //Saves a given 2D texture as a PNG, with the given file path. Returns the full asset path of the texture
        public string SaveTexture2D(Texture2D tex, string directory, string fileName)
        {
            //Make full asset path from file path and filename
            string assetPath = directory + "/" + fileName;

            SaveTexToFile(tex, directory, fileName);

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();

            return assetPath;
        }

        public string SaveTexture3D(Texture3D tex, string directory, string fileName)
        {
            //Make full asset path from file path and filename
            string assetPath = directory + "/" + fileName;

            AssetDatabase.CreateAsset(tex, assetPath);

            return assetPath;
        }


        private void SaveTexToFile(Texture2D tex, string directory, string fileName)
        {
            if (!System.IO.Directory.Exists(directory))
            {
                Debug.LogError("Save directory " + directory + " doesn't exist! Texture will not be saved");
            }
            else
            {
                System.IO.File.WriteAllBytes(directory + "/" + fileName, tex.EncodeToPNG());
                Debug.Log("Texture saved: " + directory + "/" + fileName);
                AssetDatabase.Refresh(); //if saving to the asset folder, need to scan for modified assets
            }
        }
    }
}