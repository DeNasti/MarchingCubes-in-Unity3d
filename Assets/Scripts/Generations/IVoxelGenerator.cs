using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Generations
{
    interface IVoxelGenerator
    {
        float Tollerance { get; set; }

        (CombineInstance? combineIstance, int vertexCount) GetCurrentVoxelCombineInstance(Vector3 position, GameObject voxelPrefab, GameObject voxelConteiner);
        void SetOffsets(Vector3 offsetVector);

    }
}