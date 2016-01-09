
using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Common;

public class PerformanceTests : MonoBehaviour
{
    //Plane[] SysPlanes = null;
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
     //   SysPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
    }

    void CustomTests()
    {
        GeometryUtilityUser.CalculateFrustumPlanes(Camera.main, ref CustomPlanes);
    }

    private Plane[] CalcFrustum(Camera InCamera)
    {
        GeometryUtilityUser.CalculateFrustumPlanes(InCamera, ref CachedPlanes);

#if UNITY_EDITOR && true
        Plane[] SysPlanes = GeometryUtility.CalculateFrustumPlanes(InCamera);

        for (int i = 0; i < SysPlanes.Length; ++i)
        {
            if (!IsEqual(SysPlanes[i], CachedPlanes[i]))
            {
                DebugHelper.Assert(false, "Internal error in CalcFrustum");
            }
        }
#endif

        return CachedPlanes;
    }


    private static bool IsEqual(Plane InFirst, Plane InSecond)
    {
        return IsEqual(InFirst.normal, InSecond.normal) &&
            IsEqual(InFirst.distance, InSecond.distance);
    }

    private static bool IsEqual(Vector3 InFirst, Vector3 InSecond)
    {
        return IsEqual(InFirst.x, InSecond.x) &&
            IsEqual(InFirst.y, InSecond.y) &&
            IsEqual(InFirst.y, InSecond.y);
    }

    private static bool IsEqual(float InFirst, float InSecond)
    {
        return System.Math.Abs(InFirst - InSecond) < 0.001f;
    }

    private Plane[] CachedPlanes = new Plane[6];
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

    static float[] RootVector = new float[4];
    static float[] ComVector = new float[4];

    public static void CalculateFrustumPlanes(Camera InCamera, ref Plane[] OutPlanes)
    {
        Matrix4x4 projectionMatrix = InCamera.projectionMatrix;
        Matrix4x4 worldToCameraMatrix = InCamera.worldToCameraMatrix;
        Matrix4x4 worldToProjectionMatrix = projectionMatrix * worldToCameraMatrix;

        RootVector[0] = worldToProjectionMatrix[3, 0];
        RootVector[1] = worldToProjectionMatrix[3, 1];
        RootVector[2] = worldToProjectionMatrix[3, 2];
        RootVector[3] = worldToProjectionMatrix[3, 3];

        ComVector[0] = worldToProjectionMatrix[0, 0];
        ComVector[1] = worldToProjectionMatrix[0, 1];
        ComVector[2] = worldToProjectionMatrix[0, 2];
        ComVector[3] = worldToProjectionMatrix[0, 3];

        CalcPlane(ref OutPlanes[(int)EPlaneSide.Left], ComVector[0] + RootVector[0], ComVector[1] + RootVector[1], ComVector[2] + RootVector[2], ComVector[3] + RootVector[3]);
        CalcPlane(ref OutPlanes[(int)EPlaneSide.Right], -ComVector[0] + RootVector[0], -ComVector[1] + RootVector[1], -ComVector[2] + RootVector[2], -ComVector[3] + RootVector[3]);

        ComVector[0] = worldToProjectionMatrix[1, 0];
        ComVector[1] = worldToProjectionMatrix[1, 1];
        ComVector[2] = worldToProjectionMatrix[1, 2];
        ComVector[3] = worldToProjectionMatrix[1, 3];

        CalcPlane(ref OutPlanes[(int)EPlaneSide.Bottom], ComVector[0] + RootVector[0], ComVector[1] + RootVector[1], ComVector[2] + RootVector[2], ComVector[3] + RootVector[3]);
        CalcPlane(ref OutPlanes[(int)EPlaneSide.Top], -ComVector[0] + RootVector[0], -ComVector[1] + RootVector[1], -ComVector[2] + RootVector[2], -ComVector[3] + RootVector[3]);

        ComVector[0] = worldToProjectionMatrix[2, 0];
        ComVector[1] = worldToProjectionMatrix[2, 1];
        ComVector[2] = worldToProjectionMatrix[2, 2];
        ComVector[3] = worldToProjectionMatrix[2, 3];

        CalcPlane(ref OutPlanes[(int)EPlaneSide.Near], ComVector[0] + RootVector[0], ComVector[1] + RootVector[1], ComVector[2] + RootVector[2], ComVector[3] + RootVector[3]);
        CalcPlane(ref OutPlanes[(int)EPlaneSide.Far], -ComVector[0] + RootVector[0], -ComVector[1] + RootVector[1], -ComVector[2] + RootVector[2], -ComVector[3] + RootVector[3]);

    }

    static void CalcPlane(ref Plane InPlane, float InA, float InB, float InC, float InDistance)
    {
        Vector3 Normal = new Vector3(InA, InB, InC);

        float InverseMagnitude = 1.0f / (float)System.Math.Sqrt(Normal.x * Normal.x + Normal.y * Normal.y + Normal.z * Normal.z);

        InPlane.normal = new Vector3(Normal.x * InverseMagnitude, Normal.y * InverseMagnitude, Normal.z * InverseMagnitude);

        InPlane.distance = InDistance * InverseMagnitude;
    }
}
