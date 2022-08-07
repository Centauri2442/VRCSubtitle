![VRCSubtitle](https://user-images.githubusercontent.com/28989460/183273832-bc709af2-5183-4966-8746-da98625ed422.png)
#### VRCSubtitle is a system that allows users to quickly and easily add subtitles to their projects! Designed to be performant and modular, it supports a variety of different subtitle types and usecases!
---
![SubtitleMain](https://user-images.githubusercontent.com/28989460/183273641-d4365703-9bc9-469e-bea6-1326d2be6e78.PNG)
---
## Requirements
- VRCSDK3 - Latest
- Udonsharp 1.0 or higher (Get through VRChat Creator Companion)
- TextMeshPro (Should automatically import)
---
## Setup Instructions

1) Put prefab in scene, and position either as part of a hud UI or in the world itself as a world canvas.
2) (Optional) Re-assign audio source to whatever source you wish for the (optional) audio clips to play from. Audio is **NOT** required though!
3) (Optional) Assign a target udon behaviour to recieve the events!
4) Set fade type
    - None: Text fades linearly
    - Typewriter: Text is written out as though it is typed by a typewriter
        - [Consistent Type Speed]: If enabled, will adjust timing to ensure the typing is always shown consistently. If disabled, type speed will be adjusted to the duration set by either the audio clip or manual timing.

5) (Optional) Enable the box that disables the audio source when not actively being used by the system. This will cause the audio source to be disabled when not in use, improving scene voice count.

6) Create your subtitles!
    - Text Box: Put your text there!
    - Subtitle Length: Manual value that controls the length of the subtitle. This can also be set to use an audio clip, and will be modified by the typewriter fade type if set to consistent type speed.
    - Delay Time: Amount of time after the subtitle is played before it moves onto the next subtitle
    - Pause: Pauses the subtitles after the end of the subtitle that it is checked on. To continue, simply call the **StartPlayingLines()** event again.

7) Implement event call into the subtitle system in your own scripts, calling the **StartPlayingLines()** event when you wish to start the subtitles!
---

## Limitations

- Subtitles cannot currently be cancelled or skipped once they start!
- Spamming the **StartPlayingLines()** event can and will cause the system to freeze up! If it does so, call the **ResetSubtitles()** event to fix it!
- The system only supports U# 1.0 and above! If you are using an older version of Udon, expect compile errors!

---
## Extras

- **ChangeLine(int, string)** Event: Allows you to change a subtitle during runtime, using the index of the subtitle (Number shown on the inspector-1), and the string that you wish to replace it with.
    - EX: Display a dynamically changing amount of gold that something costs as part of the subtitle.

---
## Modifying Animations

If you wish to modify the animations for having the subtitle system open/close, make sure to stay within the existing animation timing/length! The specifics of each animation can easily be changed through the animator, but the animations themselves are controlled by the script!

---
Copyright 2022 CentauriCore

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

---
## Thanks

Thank you Vowgan for helping me setup the editor inspector!
