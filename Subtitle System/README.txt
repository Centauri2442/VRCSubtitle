v1 VRCSubtitle - CentauriCore

VRCSubtitle is a system that allows users to quickly and easily add subtitles to their projects! Designed to be performant and modular, it supports a variety of different subtitle types!


IMPORTANT NOTES

- Subtitles cannot currently be cancelled or skipped once they start!
- Spamming the StartPlayingLines event can and will cause the system to freeze up! If it does so, call the ResetSubtitles event to fix it!
- The system only supports U# 1.0 and above! If you are using an older version of Udon, expect compile errors!

INSTRUCTIONS

1) Put prefab in scene, and position either as part of a hud UI or in the world itself as a world canvas.

2) (Optional) Re-assign audio source to whatever source you wish for the (optional) audio clips to play from. Audio is NOT required though!

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
    - Pause: Pauses the subtitles after the end of the subtitle that it is checked on. To continue, simply call the StartPlayingLines event again.

7) Implement call into the subtitle system in your own scripts, calling the StartPlayingLines event when you wish to start the subtitles!


EXTRAS

- ChangeLine Event: Allows you to change a subtitle during runtime, using the index of the subtitle (Number shown on the script - 1) and the string that you wish to replace it with.
    - EX: Display a dynamically changing amount of gold that something costs as part of the subtitle.


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