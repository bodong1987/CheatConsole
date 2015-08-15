// Debug Helper
using UnityEngine;

namespace Assets.Scripts.Common
{
    public abstract class DebugHelper
    {
        public static void Assert( bool bInCondition, string InErrorMessage = "" )
        {
            if( !bInCondition )
            {
                Debug.LogError(InErrorMessage);
            }
        }
    }
}