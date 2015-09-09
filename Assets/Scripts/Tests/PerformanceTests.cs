
using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Common;

public class PerformanceTests : MonoBehaviour
{
    List<int> Tests = new List<int>();

    void Awake()
    {
        Tests.Add(100);
        Tests.Add(1000);
    }
    void Update()
    {
        SysTests();
        CustomTests();
    }

    void SysTests()
    {
        var N = new System.Collections.ObjectModel.ReadOnlyCollection<int>(Tests);

        var Iter = N.GetEnumerator();

        while( Iter.MoveNext() )
        {

        }
    }

    void CustomTests()
    {
        var N = new ReadonlyContext<int>(Tests);

        var Iter = N.GetEnumerator();

        while (Iter.MoveNext())
        {

        }
    }    
}