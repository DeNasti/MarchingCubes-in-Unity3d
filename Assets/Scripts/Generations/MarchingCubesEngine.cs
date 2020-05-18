using Assets.Scripts.Entities;
using Assets.Scripts.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Generations
{
    class MarchingCubesEngine : IVoxelGenerator
    {
        private readonly NoiseGenerator _noiseGenerator;
        public float Tollerance { get; set; }
        private Vector3 _offsetVectorForNoise;
        private readonly Tables tables = new Tables();
        private bool smoother = false;

        //int octaves { get; set; } = 2;  
        //float persistance { get; set; } = 3f; 

        public MarchingCubesEngine(NoiseGenerator noiseGenerator, float tollerance, Vector3 offsetVector)
        {
            _noiseGenerator = noiseGenerator;
            Tollerance = tollerance;
            _offsetVectorForNoise = offsetVector;
        }


        public (CombineInstance? combineIstance, int vertexCount) GetCurrentVoxelCombineInstance(Vector3 position, GameObject voxelConteiner)
        {
            var meshData = GetMeshDataPosition(position);

            if (meshData is null)
                return (null, 0);

            var mesh = new Mesh();
            mesh.SetVertices(meshData.Vertices);
            mesh.triangles = meshData.Triangles;
             mesh.RecalculateNormals();

            var originalPos = voxelConteiner.transform.position;
            voxelConteiner.transform.position = position;

            var combineIstance = new CombineInstance
            {
                mesh = mesh,
                transform = voxelConteiner.transform.localToWorldMatrix
            };

            voxelConteiner.transform.position = originalPos;

            return (combineIstance, meshData.Vertices.Count);
        }

        private MeshData GetMeshDataPosition(Vector3 position)
        {
            var positionAndNoise = position + _offsetVectorForNoise;

            var valueList = new List<float>(8)
            {
                _noiseGenerator.Get3dNoise(positionAndNoise + tables.offsetForVertexIndex[0]),
                _noiseGenerator.Get3dNoise(positionAndNoise + tables.offsetForVertexIndex[1]),
                _noiseGenerator.Get3dNoise(positionAndNoise + tables.offsetForVertexIndex[2]),
                _noiseGenerator.Get3dNoise(positionAndNoise + tables.offsetForVertexIndex[3]),
                _noiseGenerator.Get3dNoise(positionAndNoise + tables.offsetForVertexIndex[4]),
                _noiseGenerator.Get3dNoise(positionAndNoise + tables.offsetForVertexIndex[5]),
                _noiseGenerator.Get3dNoise(positionAndNoise + tables.offsetForVertexIndex[6]),
                _noiseGenerator.Get3dNoise(positionAndNoise + tables.offsetForVertexIndex[7])
            };

            //trasformo questo valore in un byte che uso come indice sulla tabella di triangolazione,
            // questo restituisce che figura che sto cercando di disegnare
            var boolValues = valueList.Select(val => val >= Tollerance).ToArray();

            uint cubeIndex = 0;

            for (int i = 0; i < 8; i++)
            {
                if (boolValues[i])
                    cubeIndex += (uint)(1 << i);
            }

            if (cubeIndex == 0 || cubeIndex == 255)
                return null;


            //ottengo la lista dei triangoli e dei vertici
            var vertices = new List<Vector3>();
            var triangleList = new List<int>();
            for (int i = 0; tables.triTable[cubeIndex][i] != -1; i += 3)
            {
                vertices.Add(GetVertexCoordinates(position, tables.triTable[cubeIndex][i + 2]));
                vertices.Add(GetVertexCoordinates(position, tables.triTable[cubeIndex][i + 1]));
                vertices.Add(GetVertexCoordinates(position, tables.triTable[cubeIndex][i]));

                triangleList.Add(i + 2);
                triangleList.Add(i + 1);
                triangleList.Add(i);
            }

            return new MeshData
            {
                Vertices = vertices,
                Triangles = triangleList.ToArray()
            };
        }


        private Vector3 GetVertexCoordinates(Vector3 position, int vertex)
        {
            //qui ho l'indice del vertice in questione e le coordinate del centro del cubo, devo restituire le coordinate del vertice
            var indexA = tables.CornerIndexFromEdge[vertex][0];
            var indexB = tables.CornerIndexFromEdge[vertex][1];

            var relativeCoordinatesA = tables.offsetForVertexIndex[indexA];
            var relativeCoordinatesB = tables.offsetForVertexIndex[indexB];

            var localVertex = (relativeCoordinatesA + relativeCoordinatesB);

            if (smoother)
            {
                //ottengo il peso dei due indici
                var noiseA = _noiseGenerator.Get3dNoise(position + _offsetVectorForNoise + relativeCoordinatesA) * 10;
                var noiseB = _noiseGenerator.Get3dNoise(position + _offsetVectorForNoise + relativeCoordinatesB) * 10;

                var difference = noiseA - noiseB;

                if (difference != 0)
                {
                    var surfaceLevel = Tollerance - noiseB;

                    if (surfaceLevel != 0)
                    {
                        var percentage = difference / surfaceLevel; //(*100)
                        localVertex = localVertex * 2 / percentage;
                    }
                }

            }

            return position + localVertex;
        }

        public void SetOffsets(Vector3 offsetVector)
        {
            _offsetVectorForNoise = offsetVector;
        }


        public void SetSmoother(bool on)
        {
            smoother = on;
        }
    }
}
