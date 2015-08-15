
namespace Assets.Scripts.Common
{
    public class Singleton<T> where T : class, new()
    {
        private static T InnerInstance;

        protected Singleton()
        {
        }

        public static T GetInstance()
        {
            if (null == InnerInstance)
            {
                InnerInstance = new T();

                (InnerInstance as Singleton<T>).Init();
            }

            return InnerInstance;
        }

        public static T instance
        {
            get { return GetInstance(); }
        }
    
        public static void DestroyInstance()
        {
            (InnerInstance as Singleton<T>).UnInit();
            InnerInstance = null;
        }

        public virtual void Init()
        {
        }

        public virtual void UnInit()
        {
        }

        public static bool HasInstance()
        {
            return InnerInstance != null;
        }
    }
}