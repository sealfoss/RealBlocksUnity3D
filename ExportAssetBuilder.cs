﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class ExportAssetBuilder : MonoBehaviour {

    HashSet<MeshFilter> meshFilters = new HashSet<MeshFilter>();
    Dictionary<Transform, MeshFilter> transformWithMesh = new Dictionary<Transform, MeshFilter>();
    HashSet<Transform> transforms = new HashSet<Transform>();
    string meshString;
    public bool go = false;
    private static int StartIndex = 0;
    string fileName;
    string meshName;
    public bool ignoreFloor = false;


    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        if (go) { Go(); }
	}

    void Go()
    {
        ChildMeshSeek(this.transform);
        Debug.Log("Added " + transforms.Count + " children to transforms.");

        if (transforms.Count > 0)
        {
            Debug.Log("Found " + transforms.Count + " meshes to export!");
            string meshString = BuildObjString();
            string filename = "BlocksOBJexport" + Time.time + ".obj";
            WriteToFile(meshString, filename);
        }
        else
        {
            Debug.Log("No meshes found!");
        }

        go = false;
    }


    public void DoExport()
    {
        //string meshName = Selection.gameObjects[0].name;
        meshName = this.GetComponent<GameObject>().name;
        fileName = meshName + DateTime.Now;
        StartIndex = 0;

        StringBuilder meshString = new StringBuilder();

        meshString.Append("#" + meshName + ".obj"
                            + "\n#" + System.DateTime.Now.ToLongDateString()
                            + "\n#" + System.DateTime.Now.ToLongTimeString()
                            + "\n#-------"
                            + "\n\n");

        //Transform t = Selection.gameObjects[0].transform;
        Transform t = this.transform;


        Vector3 originalPosition = t.position;
        t.position = Vector3.zero;

        meshString.Append("g ").Append(t.name).Append("\n");

        meshString.Append(processTransform(t, true));

        WriteToFile(meshString.ToString(), fileName);

        t.position = originalPosition;

        StartIndex = 0;
        Debug.Log("Exported Mesh: " + fileName);
    }


    void ChildMeshSeek(Transform parent)
    {
        transforms = new HashSet<Transform>();
        Component[] children = GetComponentsInChildren(typeof(Transform));

        if(children != null)
        {
            Debug.Log("Found " + children.Length + " children.");

            foreach(Transform child in children)
            {
                //GameObject childObject = child.GetComponent<GameObject>();

                MeshFilter filter = child.gameObject.GetComponent<MeshFilter>();
                ObjectHighlightController highlight = child.gameObject.GetComponent<ObjectHighlightController>();

                if(filter != null && highlight == null)
                {
                    if(ignoreFloor)
                    {
                        BlockFloorController floorController = child.gameObject.GetComponent<BlockFloorController>();

                        if(floorController == null) { transforms.Add(child); }
                    }
                    else { transforms.Add(child); }
                }
            }

            //Debug.Log("Added " + transforms.Count + " children to transforms.");
        }
        else
        {
            Debug.Log("No children found.");
        }
       
    }

    string BuildObjString()
    {
        StringBuilder objStringBuilder = new StringBuilder();
        int blockCount = 1;

        //foreach(MeshFilter filter in meshFilters)
        foreach(Transform trans in transforms)
        {
            objStringBuilder.Append("g Block ").Append(blockCount).Append("\n");
            objStringBuilder.Append(MeshToString(trans.GetComponent<MeshFilter>(), trans));
        }

        return objStringBuilder.ToString();
    }


    static string processTransform(Transform t, bool makeSubmeshes)
    {
        StringBuilder meshString = new StringBuilder();

        meshString.Append("#" + t.name
                        + "\n#-------"
                        + "\n");

        if (makeSubmeshes)
        {
            meshString.Append("g ").Append(t.name).Append("\n");
        }

        MeshFilter mf = t.GetComponent<MeshFilter>();
        if (mf)
        {
            meshString.Append(MeshToString(mf, t));
        }

        for (int i = 0; i < t.childCount; i++)
        {
            meshString.Append(processTransform(t.GetChild(i), makeSubmeshes));
        }

        return meshString.ToString();
    }



    void WriteToFile(string s, string filename)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(s);
        }
    }

    public static string MeshToString(MeshFilter mf, Transform t)
    {
        Vector3 s = t.localScale;
        Vector3 p = t.localPosition;
        Quaternion r = t.localRotation;


        int numVertices = 0;
        Mesh m = mf.sharedMesh;

        if (!m)
        {
            return "####Error####";
        }

        //Material[] mats = mf.renderer.sharedMaterials;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

        StringBuilder sb = new StringBuilder();

        foreach (Vector3 vv in m.vertices)
        {
            Vector3 v = t.TransformPoint(vv);
            numVertices++;
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 nn in m.normals)
        {
            Vector3 v = r * nn;
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }
        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            sb.Append("usemap ").Append(mats[material].name).Append("\n");

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[i] + 1 + StartIndex, triangles[i + 1] + 1 + StartIndex, triangles[i + 2] + 1 + StartIndex));
            }
        }

        StartIndex += numVertices;
        return sb.ToString();
    }
}
