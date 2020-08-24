#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

namespace DeBox.AudioRig.Tests
{
    public class AudioTest : MonoBehaviour
    {
        [SerializeField] private AudioClip _testClipInstant = null;
        [SerializeField] private AudioClip _loopSound = null;

        private IEnumerator Start()
        {
            IAudioPlayControlPromise control;
            yield return new WaitForSeconds(1);
            control = AudioPlayer.Main.Play(_testClipInstant);
            yield return new WaitForSeconds(1);
            control = AudioPlayer.Main.Play(_testClipInstant);
            control.FadeOut(0.1f);
            yield return new WaitForSeconds(1);
            control = AudioPlayer.Main.Play(_testClipInstant);
            control.FadeIn(0.1f);
            yield return new WaitForSeconds(1);
            control = AudioPlayer.Main.Play(_testClipInstant);
            control.Volume = 0.1f;
            yield return new WaitForSeconds(1);
            AudioPlayer.Main.MasterVolume = 0.1f;
            control = AudioPlayer.Main.Play(_testClipInstant);
            yield return new WaitForSeconds(1);
            AudioPlayer.Main.MasterVolume = 1f;
            control = AudioPlayer.Main.Play(_testClipInstant);
            AudioPlayer.Main.MasterVolume = 0.1f;
            yield return new WaitForSeconds(1);
            AudioPlayer.Main.MasterVolume = 1f;
            control = AudioPlayer.Main.Play(_loopSound, 1, true, 0);
            yield return new WaitForSeconds(1);
            AudioPlayer.Main.MasterVolume = 0.2f;
            yield return new WaitForSeconds(1);
            AudioPlayer.Main.MasterVolume = 1f;
            yield return new WaitForSeconds(1);
            control.FadeOut(10);
            yield return new WaitForSeconds(10);
            AudioPlayer.Main.MasterVolume = 0f;
            control = AudioPlayer.Main.Play(_testClipInstant);
            yield return new WaitForSeconds(1);
            AudioPlayer.Main.MasterVolume = 1f;
            control = AudioPlayer.Main.Play(_loopSound, 0, true, 0);
            control.FadeIn(4);
            yield return new WaitForSeconds(4);
            control.FadeOut(4);
        }
    }
}
#endif