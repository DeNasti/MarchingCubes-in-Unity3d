using UnityEngine;

namespace Assets.Scripts.Generations
{
    interface IVoxelGenerator
    {
        float Tollerance { get; set; }

        (CombineInstance? combineIstance, int vertexCount) GetCurrentVoxelCombineInstance(Vector3 position, GameObject voxelConteiner);
        void SetOffsets(Vector3 offsetVector);

    }
}