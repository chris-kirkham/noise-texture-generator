using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering; //for TextureDimension
using UnityEngine;

namespace NoiseTexGenerator
{
    public class TexGenerator : MonoBehaviour
    {
        //compute shader info
        public ComputeShader texGenerator2D;
        private int texGenKernel2D;
        private const int GROUP_SIZE_2D = 8;

        public ComputeShader texGenerator3D;
        private int texGenKernel3D;
        private const int GROUP_SIZE_3D = 4;

        //texture params
        public Vector3Int textureSize;

        //noise params
        public float noiseMultiplier = 1;
        public float noiseOffset = 0f;
        //public bool useTimeAsNoiseOffset = true;
        //public float timeMultiplier;

        void Start()
        {
            texGenKernel2D = texGenerator2D.FindKernel("Generate");
            texGenKernel3D = texGenerator3D.FindKernel("Generate");
        }

        private void GenerateTexture2D(Texture2D texture, Vector2Int textureSize)
        {
            //Create 2D RenderTexture to be modified by compute shader
            RenderTextureDescriptor desc = new RenderTextureDescriptor(textureSize.x, textureSize.y, RenderTextureFormat.ARGBFloat);
            desc.dimension = TextureDimension.Tex2D;
            desc.enableRandomWrite = true;
            RenderTexture rTexture = new RenderTexture(desc);
            rTexture.Create();

            //assign parameters to compute shader 
            texGenerator2D.SetTexture(texGenKernel2D, "Result", rTexture);
            texGenerator2D.SetFloat("multiplier", noiseMultiplier);
            texGenerator2D.SetFloat("offset", noiseOffset);

            //dispatch with correct number of groups for group size
            int numGroupsX = textureSize.x / GROUP_SIZE_2D;
            int numGroupsY = textureSize.y / GROUP_SIZE_2D;
            texGenerator2D.Dispatch(texGenKernel2D, numGroupsX, numGroupsY, 1);

        }

        private void GenerateTexture3D(Texture3D texture, Vector3Int textureSize)
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
            texGenerator3D.SetFloat("multiplier", noiseMultiplier);
            texGenerator3D.SetFloat("offset", noiseOffset);

            //dispatch with correct number of groups for group size
            int numGroupsX = textureSize.x / GROUP_SIZE_3D;
            int numGroupsY = textureSize.y / GROUP_SIZE_3D;
            int numGroupsZ = textureSize.z / GROUP_SIZE_3D;
            texGenerator3D.Dispatch(texGenKernel3D, numGroupsX, numGroupsY, numGroupsZ);
        }


    }
}