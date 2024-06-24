using System;
using Unity.XR.CoreUtils.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// Manager singleton for <see cref="IDisposable"/> objects to help ensure they are disposed at the end of the application's life.
    /// </summary>
    [AddComponentMenu("")] // Hide in menu
    [HelpURL(XRHelpURLConstants.k_DisposableManagerSingleton)]
    sealed class DisposableManagerSingleton : MonoBehaviour
    {
        static DisposableManagerSingleton instance => Initialize();

        static DisposableManagerSingleton s_DisposableManagerSingleton;

        readonly HashSetList<IDisposable> m_Disposables = new HashSetList<IDisposable>();

        static DisposableManagerSingleton Initialize()
        {
            if (s_DisposableManagerSingleton == null)
            {
                var singleton = new GameObject("[DisposableManagerSingleton]");
                DontDestroyOnLoad(singleton);

                s_DisposableManagerSingleton = singleton.AddComponent<DisposableManagerSingleton>();
            }

            return s_DisposableManagerSingleton;
        }

        void Awake()
        {
            if (s_DisposableManagerSingleton != null && s_DisposableManagerSingleton != this)
            {
                Destroy(this);
                return;
            }

            if (s_DisposableManagerSingleton == null)
            {
                s_DisposableManagerSingleton = this;
            }
        }

        void OnDestroy()
        {
            DisposeAll();
        }

        void OnApplicationQuit()
        {
            DisposeAll();
        }

        void DisposeAll()
        {
            var disposableList = m_Disposables.AsList();
            for (var i = 0; i < disposableList.Count; ++i)
            {
                disposableList[i].Dispose();
            }
            m_Disposables.Clear();
        }

        /// <summary>
        /// Register disposable to auto dispose on Destroy or application quit.
        /// </summary>
        /// <param name="disposableToRegister">Disposable to auto-dispose when application quits.</param>
        public static void RegisterDisposable(IDisposable disposableToRegister)
        {
            instance.m_Disposables.Add(disposableToRegister);
        }
    }
}
