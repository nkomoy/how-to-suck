using System;
using System.Collections;
using System.IO;
using UnityEngine;

public static class AndroidDiagnostics
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void Initialize()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Array.Exists(Environment.GetCommandLineArgs(), x => x == "-diagnostic")) return;
        var go = new GameObject("AndroidDiagnostics");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<Runner>();
#endif
    }

    private sealed class Runner : MonoBehaviour
    {
        private IEnumerator Start()
        {
            // Replace the game presentation with the simplest possible camera output.
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                if (root != gameObject)
                    Destroy(root);
            }

            var camGo = new GameObject("DiagnosticCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.8f, 0.2f, 1f);
            cam.depth = 1000;
            camGo.transform.position = new Vector3(0, 0, -10);
            Camera.main?.gameObject.SetActive(false);

            yield return new WaitForEndOfFrame();
            Debug.Log("=== BASIC CAMERA DIAGNOSTIC ===");
            Debug.Log("Camera rendered test color.");

            yield return new WaitForSeconds(2f);

            string info =
                "Unity: " + Application.unityVersion + "\n" +
                "Device: " + SystemInfo.deviceModel + "\n" +
                "GPU: " + SystemInfo.graphicsDeviceName + "\n" +
                "GPU API: " + SystemInfo.graphicsDeviceType + "\n" +
                "GPU Version: " + SystemInfo.graphicsDeviceVersion + "\n" +
                "Shader Level: " + SystemInfo.graphicsShaderLevel + "\n" +
                "RenderTexture: " + SystemInfo.supportsRenderTextures + "\n" +
                "HDR: " + SystemInfo.hdrDisplaySupportFlags + "\n" +
                "System RAM MB: " + SystemInfo.systemMemorySize + "\n" +
                "Screen: " + Screen.width + "x" + Screen.height + "\n" +
                "FPS: " + Application.targetFrameRate;

            Debug.Log("=== ANDROID GRAPHICS DIAGNOSTICS ===\n" + info);

            try
            {
                File.WriteAllText(
                    Path.Combine(Application.persistentDataPath, "AndroidDiagnostics.txt"),
                    info);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            ShowToast("GPU: " + SystemInfo.graphicsDeviceName);
            yield return new WaitForSeconds(3f);
            ShowToast("API: " + SystemInfo.graphicsDeviceType);
            yield return new WaitForSeconds(3f);
            ShowToast("Shader: " + SystemInfo.graphicsShaderLevel);
            yield return new WaitForSeconds(3f);
            ShowDialog(info);
        }

        private void ShowDialog(string message)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var builderClass = new AndroidJavaClass("android.app.AlertDialog$Builder"))
                using (var builder = new AndroidJavaObject("android.app.AlertDialog$Builder", activity))
                {
                    builder.Call<AndroidJavaObject>("setTitle", "Android Graphics Diagnostics");
                    builder.Call<AndroidJavaObject>("setMessage", message);
                    builder.Call<AndroidJavaObject>("setPositiveButton", "OK", null);
                    using (var dialog = builder.Call<AndroidJavaObject>("create"))
                        dialog.Call("show");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#endif
        }

        private void ShowToast(string message)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var toastClass = new AndroidJavaClass("android.widget.Toast"))
                using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (var toast = toastClass.CallStatic<AndroidJavaObject>(
                    "makeText", context, message, 1))
                {
                    toast.Call("show");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#endif
        }
    }
}
