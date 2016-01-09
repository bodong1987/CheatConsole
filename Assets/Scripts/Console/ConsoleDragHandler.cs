/**
 * @brief help drag on screen
 * @email bodong@tencent.com
*/
#if !WITH_OUT_CHEAT_CONSOLE
using Assets.Scripts.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Console
{
    class ConsoleDragHandler
    {
        bool bDragging = false;
        Vector2 DeltaPosition;

        public void OnUpdate()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.fingerId == 0)
                {
                    bDragging = touch.phase == TouchPhase.Moved;

                    if(bDragging)
                    {
                        DeltaPosition = new Vector2(
                        touch.deltaPosition.x / Screen.width,
                        touch.deltaPosition.y / Screen.height
                        );
                    }
                }
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            bDragging = Input.GetMouseButton(0);

            if(bDragging)
            {
                DeltaPosition = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
                );
            }
#endif
        }

        public bool isDragging
        {
            get
            {
                return bDragging;
            }
        }

        public Vector2 deltaPosition
        {
            get
            {
                return DeltaPosition;
            }
        }

        public float dragDelta
        {
            get
            {
                return DeltaPosition.y;
            }
        }
    }

}
#endif