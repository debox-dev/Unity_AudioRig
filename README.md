# DeBox AudioRig

Audio control utility

## Installation instructions
### Quick Installation
Put this in your `Packages/manigest.json` file
```
"com.rsg.promises": "https://github.com/debox-dev/RSG_Promises.git",
"com.debox.audiorig": "https://github.com/debox-dev/Unity_AudioRig.git",
```

## Requirements
- Unity 2019 or higher.
- RSG Promises

## Documentation
[Documentation link](https://debox-dev.github.io/Unity_AudioRig/Docs/html/index.html)

## Usage

### Simple setup
1. Create an empty game object
2. Add the component AudioPlayer
3. Make sure 'isMain' attribute of the AudioPlayer is turned on
4. Done!

### Basic audio clip playing
```
using DeBox.AudioRig;
```
```
[SerializeField] private AudioClip _myClip;

private void Start()
{
    AudioPlayer.Main.Play(_myClip);
}
```

### Looping
Use the `PlayLoop` method
```
AudioPlayer.Main.PlayLoop(_myClip);
```

### Using the audio control when playing clips

#### Fade the clip out
```
// Plays a clip in a loop, waits 3 seconds, then fades out the clip
private IEnumerator PlayWaitAndFadeOutCoroutine()
{
    var audioControl = AudioPlayer.Main.PlayLoop(_myClip);
    yield return new WaitForSeconds(3);
    audioControl.FadeOut(3);
}
```

#### Controlling clip volume at runtime
```
private void StartHumming()
{
    this._hummAudioControl = AudioPlayer.Main.PlayLoop(_hummLoopClip);
}

private void Update()
{
    // Play at 0.3 volume if the tutorial voice actor is speaking
    this._hummAudioControl?.Volume = _isTutorialVoiceSpeaking ? 0.3f : 1f;
}
```


#### Controlling pitch
```
var control = AudioPlayer.Main.Play(_myClip);
control.Pitch = this._pressedKeyPitch;
```

#### Playing at a specific position
```
var control = AudioPlayer.Main.Play(_myClip);
control.PlayAt(transform.position);
```

#### Following a transform while playing
```
// I am a buzzing be!
var control = AudioPlayer.Main.PlayLoop(_buzzLoop);
control.Follow(transform);
```

You can stop following at any time with
```
control.StopFollow();
```
