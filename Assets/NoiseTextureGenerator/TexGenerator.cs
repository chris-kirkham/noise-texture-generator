using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering; //for TextureDimension
using UnityEngine;

namespace NoiseTexGenerator
{
    public class TexGenerator
    {
        //compute shader info
        private ComputeShader texGenerator2D;
        private int texGenKernel2D;
        private const int GROUP_SIZE_2D = 8;

        private ComputeShader texGenerator3D;
        private int texGenKernel3D, texGenKernel3DSlice;
        private const int GROUP_SIZE_3D = 4;

        public TexGenerator(ComputeShader texGenerator2D, ComputeShader texGenerator3D)
        {
            this.texGenerator2D = texGenerator2D;
            this.texGenerator3D = texGenerator3D;
            texGenKernel2D = texGenerator2D.FindKernel("Generate");
            texGenKernel3D = texGenerator3D.FindKernel("Generate");
            texGenKernel3DSlice = texGenerator3D.FindKernel("GenerateSlice");
        }

        public Texture2D GenerateTexture2D(Vector2Int textureSize, float noiseMultiplier, float noiseOffset, float noiseIntensity)
        {
            //Create 2D RenderTexture to be modified by compute shader
            RenderTextureDescriptor desc = new RenderTextureDescriptor(textureSize.x, textureSize.y, RenderTextureFormat.ARGBFloat);
            desc.dimension = TextureDimension.Tex2D;
            desc.enableRandomWrite = true;
            RenderTexture rTexture = new RenderTexture(desc);
            rTexture.Create();

            //assign parameters to compute shader 
            texGenerator2D.SetTexture(texGenKernel2D, "Result", rTexture);
            texGenerator2D.SetFloats("texSize", new float[2] { textureSize.x, textureSize.y });
            texGenerator2D.SetFloat("multiplier", noiseMultiplier);
            texGenerator2D.SetFloat("offset", noiseOffset);
            texGenerator2D.SetFloat("intensity", noiseIntensity);

            //dispatch with correct number of groups for group size
            int numGroupsX = Mathf.Max(1, textureSize.x / GROUP_SIZE_2D);
            int numGroupsY = Mathf.Max(1, textureSize.y / GROUP_SIZE_2D);
            texGenerator2D.Dispatch(texGenKernel2D, numGroupsX, numGroupsY, 1);

            //send RenderTexture data to a Texture2D and return it
            Texture2D resultTex = new Texture2D(textureSize.x, textureSize.y);
            RenderTexture.active = rTexture;
            resultTex.ReadPixels(new Rect(0, 0, textureSize.x, textureSize.y), 0, 0);
            resultTex.Apply();

            return resultTex;
        }

        //https://answers.unity.com/questions/840983/how-do-i-copy-a-3d-rendertexture-isvolume-true-to.html
        /*
        public RenderTexture GenerateTexture3D(Vector3Int textureSize, float noiseMultiplier, float noiseOffset, float noiseIntensity)
        {
            //Create 3D RenderTexture to be modified by compute shader
            RenderTextureDescriptor desc = new RenderTextureDescriptor(textureSize.x, textureSize.y, RenderTextureFormat.ARGBFloat);
            desc.depthBufferBits = 0;
            desc.dimension = TextureDimension.Tex3D;
            desc.volumeDepth = textureSize.z;
            desc.enableRandomWrite = true;
            RenderTexture rTexture = new RenderTexture(desc);
            rTexture.Create();

            //assign parameters to compute shader 
            texGenerator3D.SetTexture(texGenKernel3D, "Result", rTexture);
            texGenerator3D.SetTexture(texGenKernel3D, "ResultSlices", rTexture);
            texGenerator3D.SetFloats("texSize", new float[3] { textureSize.x, textureSize.y, textureSize.z });
            texGenerator3D.SetFloat("multiplier", noiseMultiplier);
            texGenerator3D.SetFloat("offset", noiseOffset);
            texGenerator3D.SetFloat("intensity", noiseIntensity);

            //dispatch with correct number of groups for group size
            int numGroupsX = Mathf.Max(1, textureSize.x / GROUP_SIZE_3D);
            int numGroupsY = Mathf.Max(1, textureSize.y / GROUP_SIZE_3D);
            int numGroupsZ = Mathf.Max(1, textureSize.z / GROUP_SIZE_3D);
            texGenerator3D.Dispatch(texGenKernel3D, numGroupsX, numGroupsY, numGroupsZ);

            //send RenderTexture data to a Texture3D and return it
            Texture3D resultTex = new Texture3D(textureSize.x, textureSize.y, textureSize.z, TextureFormat.ARGB32, false);
            RenderTexture.active = rTexture;
            
            //resultTex.SetPixels(new Rect(0, 0, textureSize.x, textureSize.y), 0, 0);
            resultTex.Apply();

            return rTexture;
        }
        */

        public Texture2D GenerateTextureSlice(Vector2Int textureSize, int slice, float noiseMultiplier, float noiseOffset, float noiseIntensity)
        {
            RenderTexture rt = new RenderTexture(textureSize.x, textureSize.y, 0, RenderTextureFormat.ARGB32);
            rt.dimension = TextureDimension.Tex2D;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.enableRandomWrite = true;
            rt.Create();

            texGenerator3D.SetTexture(texGenKernel3DSlice, "ResultSlice", rt);
            texGenerator3D.SetInt("slice", slice);
            texGenerator3D.SetInts("texSize", new int[3] { textureSize.x, textureSize.y, 0 });
            texGenerator3D.SetFloat("multiplier", noiseMultiplier);
            texGenerator3D.SetFloat("offset", noiseOffset);
            texGenerator3D.SetFloat("intensity", noiseIntensity);

            int numGroupsX = Mathf.Max(1, textureSize.x / GROUP_SIZE_3D);
            int numGroupsY = Mathf.Max(1, textureSize.y / GROUP_SIZE_3D);
            texGenerator3D.Dispatch(texGenKernel3DSlice, numGroupsX, numGroupsY, 1);

            Texture2D resultTex = new Texture2D(textureSize.x, textureSize.y);
            RenderTexture.active = rt;
            resultTex.ReadPixels(new Rect(0, 0, textureSize.x, textureSize.y), 0, 0);
            resultTex.Apply();

            return resultTex;
        }

        public Texture3D GenerateTexture3D(Vector3Int textureSize, float noiseMultiplier, float noiseOffset, float noiseIntensity)
        {
            Texture3D result = new Texture3D(textureSize.x, textureSize.y, textureSize.z, TextureFormat.ARGB32, false);

            //generate slices
            Texture2D[] slices = new Texture2D[textureSize.z];
            for(int i = 0; i < textureSize.z; i++)
            {
                slices[i] = GenerateTextureSlice(new Vector2Int(textureSize.x, textureSize.y), i, noiseMultiplier, noiseOffset, noiseIntensity);
            }

            Color[] resultPixels = result.GetPixels();
            for(int k = 0; k < textureSize.x; k++)
            {
                Color[] slicePixels = slices[k].GetPixels();
                for(int i = 0; i < textureSize.y; i++)
                {
                    for(int j = 0; j < textureSize.z; j++)
                    {
                        resultPixels[i + j * textureSize.z + k * textureSize.x * textureSize.y] = slicePixels[i + j * textureSize.x]; 
                    }
                }
            }

            result.SetPixels(resultPixels);
            result.Apply();

            return result;
        }

    }
}