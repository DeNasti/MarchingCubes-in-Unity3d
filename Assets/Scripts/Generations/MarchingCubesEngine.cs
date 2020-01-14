using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Generations
{
    class MarchingCubesEngine : IVoxelGenerator
    {
        NoiseGenerator _noiseGenerator;
        public float Tollerance { get; set; }
        Vector3 _offsetVectorForNoise;
        Tables tables = new Tables();
        private bool smoother = false;

        int octaves { get; set; } = 2;  //2
        float persistance { get; set; } = 3f; //7


        public MarchingCubesEngine(NoiseGenerator noiseGenerator, float tollerance, Vector3 offsetVector)
        {
            _noiseGenerator = noiseGenerator;
            Tollerance = tollerance;
            _offsetVectorForNoise = offsetVector;
        }


        (CombineInstance? combineIstance, int vertexCount) IVoxelGenerator.GetCurrentVoxelCombineInstance(Vector3 position, GameObject voxelPrefab, GameObject voxelConteiner)
        {
            var mesh = new Mesh();




            //ottengo per ognuno degli 8 vertici se sono disegnabili o meno
            var val1 = _noiseGenerator.GetOctavePerlin3D(position + _offsetVectorForNoise + tables.offsetForVertexIndex[0], octaves, persistance) * 10;
            var val2 = _noiseGenerator.GetOctavePerlin3D(position + _offsetVectorForNoise + tables.offsetForVertexIndex[1], octaves, persistance) * 10;
            var val3 = _noiseGenerator.GetOctavePerlin3D(position + _offsetVectorForNoise + tables.offsetForVertexIndex[2], octaves, persistance) * 10;
            var val4 = _noiseGenerator.GetOctavePerlin3D(position + _offsetVectorForNoise + tables.offsetForVertexIndex[3], octaves, persistance) * 10;
            var val5 = _noiseGenerator.GetOctavePerlin3D(position + _offsetVectorForNoise + tables.offsetForVertexIndex[4], octaves, persistance) * 10;
            var val6 = _noiseGenerator.GetOctavePerlin3D(position + _offsetVectorForNoise + tables.offsetForVertexIndex[5], octaves, persistance) * 10;
            var val7 = _noiseGenerator.GetOctavePerlin3D(position + _offsetVectorForNoise + tables.offsetForVertexIndex[6], octaves, persistance) * 10;
            var val8 = _noiseGenerator.GetOctavePerlin3D(position + _offsetVectorForNoise + tables.offsetForVertexIndex[7], octaves, persistance) * 10;

 
            //trasformo questo valore in un byte che uso come indice sulla tabella di triangolazione,
            // questo restituisce che figura che sto cercando di disegnare
            bool[] boolValues = new bool[]{
                (val1 >= Tollerance),
                (val2>= Tollerance),
                (val3>= Tollerance),
                (val4 >= Tollerance),
                (val5>= Tollerance),
                (val6>= Tollerance),
                (val7>= Tollerance),
                (val8 >= Tollerance)
            };

            uint cubeIndex = 0;
            for (int i = 0; i < boolValues.Length; i++)
            {
                if (boolValues[i])
                    cubeIndex += (uint)(1 << i);
            }

            if (cubeIndex == 0 || cubeIndex == 255)
                return (null, 0);


            //ottengo la lista dei triangoli e dei vertici
            var vertices = new List<Vector3>();
            var triangleList = new List<int>();
            for (int i = 0; tables.triTable[cubeIndex][i] != -1; i += 3)
            {
                vertices.Add(GetVertex(position, tables.triTable[cubeIndex][i + 2]));
                vertices.Add(GetVertex(position, tables.triTable[cubeIndex][i + 1]));
                vertices.Add(GetVertex(position, tables.triTable[cubeIndex][i]));

                triangleList.Add(i + 2);
                triangleList.Add(i + 1);
                triangleList.Add(i);
            }

            mesh.SetVertices(vertices);
            mesh.triangles = triangleList.ToArray();
            mesh.RecalculateNormals();

            var originalPos = voxelConteiner.transform.position;
            voxelConteiner.transform.position = position;


            var combineIstance = new CombineInstance
            {
                mesh = mesh,
                transform = voxelConteiner.transform.localToWorldMatrix
            };

            voxelConteiner.transform.position = originalPos;

            return (combineIstance, vertices.Count); //TODO 
        }

        private Vector3 GetVertex(Vector3 position, int vertex)
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
                var noiseA = _noiseGenerator.GetOctavePerlin3D(position + _offsetVectorForNoise + relativeCoordinatesA, octaves, persistance) * 10;
                var noiseB = _noiseGenerator.GetOctavePerlin3D(position + _offsetVectorForNoise + relativeCoordinatesB, octaves, persistance) * 10;

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
