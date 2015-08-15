using System;
using UnityEngine;

namespace Assets.Scripts.Common
{
    public class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        private static T InnerInstance;
        private static bool bIsDestroyed;
        private static readonly string RootObjectName = "RootObj";

        public static T GetInstance()
        {
            if (InnerInstance == null && !bIsDestroyed)
            {
                Type theType = typeof(T);

                InnerInstance = (T)FindObjectOfType(theType);

                if (InnerInstance == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    go.AddComponent<T>();

                    GameObject bootObj = GameObject.Find(RootObjectName);
                    if (bootObj != null)
                    {
                        go.transform.parent = bootObj.transform;
                    }
                }
            }


            return InnerInstance;
        }

        public static T instance
        {
            get { return GetInstance(); }
        }

        public static void DestroyInstance()
        {
            if (InnerInstance != null)
            {
                Destroy(InnerInstance.gameObject);
            }

            bIsDestroyed = true;
            InnerInstance = null;
        }

        protected virtual void Awake()
        {
            if (InnerInstance != null && InnerInstance.gameObject != gameObject)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }

            }
            else if (InnerInstance == null)
            {
                InnerInstance = GetComponent<T>();
            }

            DontDestroyOnLoad(gameObject);

            Init();
        }

        protected virtual void OnDestroy()
        {
            if (InnerInstance != null && InnerInstance.gameObject == gameObject)
            {
                InnerInstance = null;
            }
        }

        public virtual void DestroySelf()
        {
            InnerInstance = null;
            Destroy(gameObject);
        }

        public static bool HasInstance()
        {
            return InnerInstance != null;
        }

        protected virtual void Init()
        {

        }
    }
}