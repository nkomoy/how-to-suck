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
        var go = new GameObject("AndroidDiagnostics");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<Runner>();
#endif
    }

    private sealed class Runner : MonoBehaviour
    {
        private IEnumerator Start()
        {
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
            yield return new WaitForSeconds(4f);
            ShowToast("API: " + SystemInfo.graphicsDeviceType);
            yield return new WaitForSeconds(4f);
            ShowToast("Shader: " + SystemInfo.graphicsShaderLevel);
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
