using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Singleton behaviour class, used for components that should only have one instance
/// </summary>
/// <typeparam name="T"></typeparam>
[DefaultExecutionOrder(-50)]
public class Singleton<T> : MonoBehaviour where T : Singleton<T> {

    private static T _instance;

    // get instance when called, so can be used in Awake
    public static T Instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<T>();
               
            }
            return _instance;
        }
    }
    /// <summary>
    ///     Returns whether the instance has been initialized or not.
    /// </summary>
    public static bool IsInitialized => Instance != null;
    /// <summary>
    /// Should this Singleton be created if it is not found?
    /// </summary>
    // protected static bool autoCreate => false;

    /// <summary>
    ///     should this gameobject be destroyed if another is found?
    /// </summary>
    protected virtual bool destroyIfMultiple => true;

    /// <summary>
    ///     Base awake method that checks the singleton's unique instance.
    /// </summary>
    protected virtual void Awake() {
        if (GameObject.FindObjectsOfType<T>().Length > 1) {
            Debug.LogErrorFormat("Trying to instantiate a second instance of singleton class {0}", GetType().Name);
        }
        if (_instance != null && _instance != this) {
            if (destroyIfMultiple) {
                if (Application.isPlaying) {
                    Destroy(_instance.gameObject);
                } else {
                    DestroyImmediate(_instance.gameObject);
                }
            }
        }
    }

    protected virtual void OnDestroy() {
        if (_instance == this)
            _instance = null;
    }

}
