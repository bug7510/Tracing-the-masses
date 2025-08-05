using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DOTweenExtension
{
    class DOTweenExtensionBehaviour : MonoBehaviour
    {
        public event TweenCallback OnUpdate;
        void Start()
        {
            SceneManager.sceneUnloaded += (scene) => { OnUpdate = null; };
        }
        void Update()
        {
            OnUpdate?.Invoke();
        }
    }
    static DOTweenExtension()
    {
        if (behaviour == null)
        {
            GameObject behaviourObject = new("DTE Behaviour", new Type[] { typeof(DOTweenExtensionBehaviour) });
            UnityEngine.Object.DontDestroyOnLoad(behaviourObject);
            behaviour = behaviourObject.GetComponent<DOTweenExtensionBehaviour>();
        }
    }
    static DOTweenExtensionBehaviour behaviour;
    public static T OnPauseInUpdate<T>(this T targetTween, TweenCallback action) where T : Tween
    {
        targetTween.OnPause(() => behaviour.OnUpdate += action)
                    .OnPlay(() => behaviour.OnUpdate -= action);
        return targetTween;
    }
    /// <summary>
    ///　デリゲートによって、動くか否かをbool値に依存させる
    /// </summary>
    /// <param name="dependency">デリゲート。Tweenが動くか否かはこれの返り値に依存する</param>
    public static T SetDependency<T>(this T targetTween, Func<bool> dependency) where T : Tween
    {
        targetTween.OnUpdate(() =>
                                {
                                    if (!dependency())
                                    {
                                        targetTween.Pause();
                                    }
                                })
                    .OnPauseInUpdate(() =>
                                {
                                    if (dependency())
                                    {
                                        targetTween.Play();
                                    }
                                });
        return targetTween;
    }
}