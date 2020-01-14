using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Generations
{
    class CubeVoxelGenerator : IVoxelGenerator
    {
        NoiseGenerator _perlinNoiseGenerator;
        public float Tollerance { get; set; }

        float offsetX = 0;
        float offsetY = 0;
        float offsetZ = 0;

        public CubeVoxelGenerator(NoiseGenerator perlinNoiseGenerator, float tollerance, Vector3 offsetVector)
        {
            _perlinNoiseGenerator = perlinNoiseGenerator;
            Tollerance = tollerance;
            SetOffsets(offsetVector);
        }


        public void SetOffsets(Vector3 offsetVector)
        {
            offsetX = offsetVector.x;
            offsetY = offsetVector.y;
            offsetZ = offsetVector.z;
        }

        bool TryGetIfIsDrawable(Vector3 voxelPosition, out float curPerlinNoise)
        {
            var perlinCoordinates = new Vector3((voxelPosition.x + offsetX), (voxelPosition.y + offsetY), (voxelPosition.z + offsetZ));
            curPerlinNoise = _perlinNoiseGenerator.GetPerlinNoise3D(perlinCoordinates);

            if (curPerlinNoise < Tollerance || CheckIfUnderGround(perlinCoordinates))
                return false;

            return true;
        }



        /// <summary>
        /// Returns a CombineIstance rappresenting the mesh for the current position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        (CombineInstance? combineIstance, int vertexCount) IVoxelGenerator.GetCurrentVoxelCombineInstance(Vector3 position, GameObject voxelPrefab, GameObject voxelConteiner)
        {
            if (!TryGetIfIsDrawable(position, out _))
            {
                return (null, 0);
            }

            var block = UnityEngine.Object.Instantiate(voxelPrefab, position, Quaternion.identity, voxelConteiner.transform);
            var blockMesh = block.GetComponent<MeshFilter>();

            blockMesh.transform.position = position;

            var combineIstance = new CombineInstance
            {
                mesh = blockMesh.sharedMesh,
                transform = blockMesh.transform.localToWorldMatrix,
            };
            UnityEngine.Object.Destroy(block);

            return (combineIstance, 24);    //24 is the number of vertices per square
        }



        private bool CheckIfUnderGround(Vector3 perlinCoordinates)
        {
            var ListCheck = new float[6];

            ListCheck[0] = _perlinNoiseGenerator.GetPerlinNoise3D((perlinCoordinates + Vector3.right));
            ListCheck[1] = _perlinNoiseGenerator.GetPerlinNoise3D((perlinCoordinates + Vector3.left));
            ListCheck[2] = _perlinNoiseGenerator.GetPerlinNoise3D((perlinCoordinates + Vector3.up));
            ListCheck[3] = _perlinNoiseGenerator.GetPerlinNoise3D((perlinCoordinates + Vector3.down));
            ListCheck[4] = _perlinNoiseGenerator.GetPerlinNoise3D((perlinCoordinates + Vector3.forward));
            ListCheck[5] = _perlinNoiseGenerator.GetPerlinNoise3D((perlinCoordinates + Vector3.back));


            var checks = ListCheck.Select(x => x >= Tollerance).Distinct();

            //if any is under tollerance i'm under
            if (checks.Count() == 1 && checks.First())
            {
                return true;
            }

            return false;
        }


    }
}
