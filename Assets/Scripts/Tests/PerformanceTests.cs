
using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Common;

public class PerformanceTests : MonoBehaviour
{
    Plane[] SysPlanes = null;
    Plane[] CustomPlanes = new Plane[6];
    
    void Awake()
    {        
    }

    void Update()
    {
        for (int i = 0; i < 1000; ++i)
        {
            SysTests();
            CustomTests();
        }
    }

    void SysTests()
    {
        SysPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
    }

    void CustomTests()
    {
        GeometryUtilityUser.CalculateFrustumPlanes(Camera.main, ref CustomPlanes);
    }    
}

public static class GeometryUtilityUser
{
    /**
     * @warning OutPlanes must be new Plane[6]
     *    Plane Position :
     *       Left
     *       Right
     *       Bottom
     *       Top
     *       Near
     *       Far
    */
    enum EPlaneSide
    {
        Left,
        Right,
        Bottom,
        Top,
        Near,
        Far
    }

    static float[] tmpVec = new float[4];
    static float[] otherVec = new float[4];

    public static void CalculateFrustumPlanes(Camera InCamera, ref Plane[] OutPlanes)
    {
        Matrix4x4 projectionMatrix = InCamera.projectionMatrix;
        Matrix4x4 worldToCameraMatrix = InCamera.worldToCameraMatrix;
        Matrix4x4 worldToProjectionMatrix = projectionMatrix * worldToCameraMatrix;
        
        tmpVec[0] = worldToProjectionMatrix[3, 0];
        tmpVec[1] = worldToProjectionMatrix[3, 1];
        tmpVec[2] = worldToProjectionMatrix[3, 2];
        tmpVec[3] = worldToProjectionMatrix[3, 3];

        otherVec[0] = worldToProjectionMatrix[0, 0];
        otherVec[1] = worldToProjectionMatrix[0, 1];
        otherVec[2] = worldToProjectionMatrix[0, 2];
        otherVec[3] = worldToProjectionMatrix[0, 3];

        CalcPlane(ref OutPlanes[(int)EPlaneSide.Left], otherVec[0] + tmpVec[0], otherVec[1] + tmpVec[1], otherVec[2] + tmpVec[2], otherVec[3] + tmpVec[3]);        
        CalcPlane(ref OutPlanes[(int)EPlaneSide.Right], - otherVec[0] + tmpVec[0], -otherVec[1] + tmpVec[1], -otherVec[2] + tmpVec[2], -otherVec[3] + tmpVec[3]);
        
        otherVec[0] = worldToProjectionMatrix[1, 0];
        otherVec[1] = worldToProjectionMatrix[1, 1];
        otherVec[2] = worldToProjectionMatrix[1, 2];
        otherVec[3] = worldToProjectionMatrix[1, 3];

        CalcPlane(ref OutPlanes[(int)EPlaneSide.Bottom], otherVec[0] + tmpVec[0], otherVec[1] + tmpVec[1], otherVec[2] + tmpVec[2], otherVec[3] + tmpVec[3]);
        CalcPlane(ref OutPlanes[(int)EPlaneSide.Top], - otherVec[0] + tmpVec[0], -otherVec[1] + tmpVec[1], -otherVec[2] + tmpVec[2], -otherVec[3] + tmpVec[3]);
        
        otherVec[0] = worldToProjectionMatrix[2, 0];
        otherVec[1] = worldToProjectionMatrix[2, 1];
        otherVec[2] = worldToProjectionMatrix[2, 2];
        otherVec[3] = worldToProjectionMatrix[2, 3];

        CalcPlane(ref OutPlanes[(int)EPlaneSide.Near], otherVec[0] + tmpVec[0], otherVec[1] + tmpVec[1], otherVec[2] + tmpVec[2], otherVec[3] + tmpVec[3]);
        CalcPlane(ref OutPlanes[(int)EPlaneSide.Far], - otherVec[0] + tmpVec[0], -otherVec[1] + tmpVec[1], -otherVec[2] + tmpVec[2], -otherVec[3] + tmpVec[3]);
        
    }

    static void CalcPlane(ref Plane InPlane, float InA, float InB, float InC, float InDistance )
    {
        Vector3 Normal = new Vector3(InA, InB, InC);
        
        float InverseMagnitude = 1.0f / (float)Math.Sqrt(Normal.x * Normal.x + Normal.y * Normal.y + Normal.z * Normal.z);

        InPlane.normal = new Vector3(Normal.x * InverseMagnitude, Normal.y * InverseMagnitude, Normal.z * InverseMagnitude);

        InPlane.distance = InDistance * InverseMagnitude;
    }
}