using UnityEngine;
using UnityEngine.Rendering; // 如果是旧版管线，请改为 using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class MemoryEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("放入你配置好的 Volume 物体")]
    public Volume postProcessingVolume; // 如果是旧管线报错，把 Volume 改成 PostProcessVolume

    [Tooltip("放入场景中展示图片的 Plane 物体")]
    public MeshRenderer scenePlane;

    [Header("Settings")]
    public float displayDuration = 3f;      // 持续显示的时间
    public float fadeDuration = 0.5f;       // 淡入淡出的时间

    public void ActivateEffect()
    {
        Debug.Log("【特效管理器】收到了小猫的指令！开始播放特效！"); // 加上这句
        StartCoroutine(EffectRoutine());
    }

    IEnumerator EffectRoutine()
    {
        // 1. 确保物体被激活，并将初始透明度和权重设为 0
        if (postProcessingVolume != null)
        {
            postProcessingVolume.gameObject.SetActive(true);
            postProcessingVolume.weight = 0f;
        }

        Color matColor = scenePlane != null ? scenePlane.material.color : Color.white;
        if (scenePlane != null)
        {
            scenePlane.gameObject.SetActive(true);
            matColor.a = 0f;
            scenePlane.material.color = matColor;
        }

        // 2. 同步平滑淡入
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float progress = t / fadeDuration;

            if (postProcessingVolume != null) postProcessingVolume.weight = progress;
            if (scenePlane != null)
            {
                matColor.a = progress;
                scenePlane.material.color = matColor;
            }
            yield return null;
        }

        // 确保数值完全到位
        if (postProcessingVolume != null) postProcessingVolume.weight = 1f;
        if (scenePlane != null) { matColor.a = 1f; scenePlane.material.color = matColor; }

        // 3. 持续显示设定的时间
        yield return new WaitForSeconds(displayDuration);

        // 4. 同步平滑淡出
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float progress = 1f - (t / fadeDuration); // 从 1 减到 0

            if (postProcessingVolume != null) postProcessingVolume.weight = progress;
            if (scenePlane != null)
            {
                matColor.a = progress;
                scenePlane.material.color = matColor;
            }
            yield return null;
        }

        // 5. 收尾：确保数值为 0 并隐藏物体
        if (postProcessingVolume != null)
        {
            postProcessingVolume.weight = 0f;
            postProcessingVolume.gameObject.SetActive(false);
        }
        if (scenePlane != null)
        {
            matColor.a = 0f;
            scenePlane.material.color = matColor;
            scenePlane.gameObject.SetActive(false);
        }
    }
}