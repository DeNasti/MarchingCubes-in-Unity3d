using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using Assets.Scripts.Utils;
using Assets.Scripts.Generations;

public class GenerateTerrain : MonoBehaviour
{
    [SerializeField]
    private float chunkSize = 124;

    [SerializeField]
    private float verticalChunkSize = 32;


    public bool changeOffsetOnRefresh;


    // [Range(0f, 0.9f)]
    public float tollerance = 0.5f;

    public GameObject voxelPrefab;
    public GameObject voxelConteiner;

    public Material material;

    private List<GameObject> meshes = new List<GameObject>();
    NoiseGenerator noiseGenerator;

    public float scale = 20f;

    private float offsetX = 0;
    private float offsetY = 0;
    private float offsetZ = 0;

    private int maxVertices = 30000;//65500;

    private IVoxelGenerator voxelGen;

    // Start is called before the first frame update
    void Start()
    {
        noiseGenerator = new NoiseGenerator(scale);
        voxelGen = new MarchingCubesEngine(noiseGenerator, tollerance, new Vector3(offsetX, offsetY, offsetZ));//CubeVoxelGenerator(perlinNoiseGenerator, tollerance, new Vector3(offsetX, offsetY, offsetZ));
        GenerateRandomOffsets();
        StartCoroutine("Generate");

    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("space");
            GenerateRandomOffsets();
            StartCoroutine("Generate");
        }
    }

    IEnumerator Generate()
    {
        CleanMeshList();

        var blockData = new List<CombineInstance>();

        noiseGenerator.ClearCache();

        int totVerticisOfThisMesh = 0;

        for (int x = 0; x <= chunkSize; x++)
        {
            for (int y = 0; y <= verticalChunkSize; y++)
            {
                for (int z = 0; z <= chunkSize; z++)
                {
                    var thisPos = new Vector3(x, y, z);

                   var (combineIstance, curVerticesCount) = voxelGen.GetCurrentVoxelCombineInstance(thisPos, voxelPrefab, voxelConteiner);

                    if(combineIstance != null)
                        blockData.Add((CombineInstance)combineIstance);

                    totVerticisOfThisMesh += curVerticesCount;
                    
                    //if there are enough vertices, draw this mesh and reset counters
                    if (totVerticisOfThisMesh >= maxVertices )
                    {
                        GenerateSingleMeshFromCombineIstanceArray(blockData.ToArray());
                        blockData = new List<CombineInstance>();
                        totVerticisOfThisMesh = 0;
                        yield return null;
                    }
                }
            }
        }

        //draw last piece if exists
        if (blockData.Any())
        {
            GenerateSingleMeshFromCombineIstanceArray(blockData.ToArray());
        }

        Debug.Log("ended");
        yield return null;
    }

    private void GenerateSingleMeshFromCombineIstanceArray(CombineInstance[] combineList)
    {
        var g = new GameObject("MeshContainer");
        g.transform.parent = voxelConteiner.transform;
        MeshFilter mf = g.AddComponent<MeshFilter>();
        MeshRenderer mr = g.AddComponent<MeshRenderer>();
        mr.material = material;
        mf.mesh.CombineMeshes(combineList);
        meshes.Add(g);
        g.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;//setting colliders takes more time. disabled for testing.
    }




    void CleanMeshList()
    {
        foreach (var msh in meshes)
        {
            Destroy(msh);
        }
    }


    void GenerateRandomOffsets()
    {
        offsetX = UnityEngine.Random.Range(0f, 300f);
        offsetY = UnityEngine.Random.Range(0f, 300f);
        offsetZ = UnityEngine.Random.Range(0f, 300f);

       if(changeOffsetOnRefresh)
            voxelGen.SetOffsets(new Vector3(offsetX, offsetY, offsetZ));

        voxelGen.Tollerance = tollerance;
        noiseGenerator.Scale = scale;
    }
}
