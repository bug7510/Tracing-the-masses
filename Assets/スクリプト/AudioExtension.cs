using UnityEngine;

public static class AudioExtension
{
    /// <summary>
    /// AudioSourceをAddComponentし、ピッチをランダムに変更してAudioClipを再生する
    /// </summary>
    /// <param name="audioClip">再生するAudioClip</param>
    /// <param name="gameObject">gameObjectの想定</param>
    /// <param name="minPitch">ピッチの最小値</param>
    /// <param name="maxPitch">ピッチの最大値</param>
    public static void PlayRandomPitchSound(this AudioClip audioClip, GameObject gameObject, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        // AudioSourceを生成し、GameObjectにアタッチ
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();

        // ピッチをランダムに設定
        audioSource.pitch = Random.Range(minPitch, maxPitch);

        // AudioClipを再生
        audioSource.PlayOneShot(audioClip);

        // 再生後、一定時間後にAudioSourceを破棄
        Object.Destroy(audioSource, audioClip.length);
    }
}