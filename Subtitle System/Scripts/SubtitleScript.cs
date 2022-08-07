/*
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
 */

using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Centauri.SubtitleSystem
{
    public enum TextFadeType
    {
        None,
        Typewriter
    }
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SubtitleScript : UdonSharpBehaviour
    {
        #region Editor Script Variables

        [HideInInspector] public bool[] isOpen;
        [HideInInspector] public bool mainSettingsOpen;
        [HideInInspector] public bool extraSettingsOpen;

        #endregion

        #region Public Variables

        [HideInInspector] public string[] SubtitleLines;
        [HideInInspector] public AudioClip[] SubtitleAudio;
        [HideInInspector] public float[] DelayTime;
        [HideInInspector] public float[] SubtitleLength;
        [HideInInspector] public bool[] pauseAfterLine;
        [HideInInspector] public TextFadeType FadeType;
        [HideInInspector] public bool TypewriterLinear;

        [HideInInspector] public AudioSource Audio;
        [HideInInspector] public bool audioDisable = true;
        [HideInInspector] public Animator SubtitleAnimator;
        [HideInInspector] public TextMeshProUGUI Text;
        [HideInInspector] public UdonBehaviour OptionalTargetBehaviour;
        [HideInInspector] public string PauseEventName = "PausedSubtitles";
        [HideInInspector] public string EndEventName = "FinishedSubtitles";

        #endregion

        #region Private Variables

        private bool isPlayingLines;
        private int currentLine;
        private int numOfLines;
        private bool isDelayed;
        private bool isPaused;
        private int charactersShown = 0;

        #endregion

        #region External Event Calls

        public void ChangeLine(int index, string line)
        {
            SubtitleLines[index] = line;
        }

        public void StartPlayingLines()
        {
            if (audioDisable && Audio != null)
            {
                Audio.enabled = true;
            }

            if (isPlayingLines)
            {
                return;
            }
            else if(!isPaused)
            {
                currentLine = 0;
            }
            
            isPlayingLines = true;

            SubtitleAnimator.CrossFadeInFixedTime("TextFade.Empty", 0);
            
            switch(FadeType)
            {
                case TextFadeType.None:
                    Text.maxVisibleCharacters = Int32.MaxValue;
                    break;
                case TextFadeType.Typewriter:
                    charactersShown = 0;
                    Text.maxVisibleCharacters = charactersShown;
                    break;
            }

            if (isPaused)
            {
                currentLine++;
            }

            if (currentLine <= 0)
            {
                currentLine = 0;

                Text.text = SubtitleLines[currentLine]; // Allows first line to be seen without it flickering in

                SubtitleAnimator.CrossFadeInFixedTime("Base Layer.OpenSubtitle", 0f);

                SendCustomEventDelayedSeconds(nameof(PlayLineLoop), 0.5f);
            }
            else
            {
                if (currentLine >= SubtitleLines.Length)
                {
                    Debug.Log("Subtitles have been broken! Call the ResetSubtitles function to fix!");
                    return;
                }
                Text.text = SubtitleLines[currentLine]; // Allows first line to be seen without it flickering in

                SubtitleAnimator.CrossFadeInFixedTime("Base Layer.OpenSubtitle", 0f);

                SendCustomEventDelayedSeconds(nameof(PlayLineLoop), 0.5f);
            }
            
            var line = SubtitleLines[currentLine];

            line = line.Replace("{PlayerName}", Networking.LocalPlayer.displayName);
            
            Text.text = line;
        }

        public void ResetSubtitles()
        {
            if (audioDisable && Audio != null)
            {
                Audio.enabled = false;
            }
            isPlayingLines = false;
            isPaused = false;
            currentLine = 0;
            Text.text = "";

            SubtitleAnimator.SetFloat("SpeedMultiplier", 1f);
            SubtitleAnimator.CrossFadeInFixedTime("TextFade.Empty", 0);
            
            SubtitleAnimator.CrossFadeInFixedTime("Base Layer.CloseSubtitle", 0f);
        }
        
        #endregion

        #region Internal Functions
        
        private void Start()
        {
            numOfLines = SubtitleLines.Length;
            Text.text = "";

            SendCustomEventDelayedFrames(nameof(DelayedStart), UnityEngine.Random.Range(1, 10));
        }

        public void DelayedStart()
        {
            if (TypewriterLinear)
            {
                for (var i = 0; i < SubtitleLength.Length; i++)
                {
                    var startTime = SubtitleLength[i];
                    float lineLength = SubtitleLines[i].Length;

                    if (startTime / lineLength < 0.05f)
                    {
                        SubtitleLength[i] = lineLength * 0.05f;
                    }

                    SubtitleLength[i] += 1f;
                }
            }
        }

        public void ShowNextCharacter()
        {
            if (currentLine >= SubtitleLines.Length)
            {
                Debug.Log("Subtitles have been broken! Call the ResetSubtitles function to fix!");
                return;
            }

            if (charactersShown > SubtitleLines[currentLine].Length) return;

            charactersShown++;
            Text.maxVisibleCharacters = charactersShown;

            if (!TypewriterLinear)
            {
                var delay = 0.0f;
                var length = SubtitleLength[currentLine];
            
                delay = SubtitleLength[currentLine] / SubtitleLines[currentLine].Length;

                if (delay > 0.1f)
                {
                    delay = 0.1f;
                }

                if (delay > 0.05f)
                {
                    SendCustomEventDelayedSeconds(nameof(ShowNextCharacter), delay);
                }
                else
                {
                    SendCustomEventDelayedFrames(nameof(ShowNextCharacter), 1);
                }
            }
            else
            {
                SendCustomEventDelayedSeconds(nameof(ShowNextCharacter), 0.05f);
            }
        }

        public void PlayLineLoop()
        {
            if (currentLine >= SubtitleLines.Length)
            {
                Debug.Log("Subtitles have been broken! Call the ResetSubtitles function to fix!");
                return;
            }

            if (SubtitleAudio[currentLine] != null)
            {
                PlayLine(currentLine);
            }

            if (isDelayed)
            {
                SubtitleAnimator.CrossFadeInFixedTime("TextFade.FadeTextIn", 0);
            }

            isDelayed = false;
            isPaused = false;
            
            
            switch(FadeType)
            {
                case TextFadeType.None:
                    break;
                case TextFadeType.Typewriter:
                    charactersShown = 0;
                    Text.maxVisibleCharacters = charactersShown;
                    ShowNextCharacter();
                    break;
            }

            if (numOfLines == 1)
            {
                SendCustomEventDelayedSeconds(nameof(FinishPlayingLines), SubtitleLength[currentLine]);
                return;
            }

            if (numOfLines > 1 && currentLine <= SubtitleLength.Length - 2)
            {
                if (!pauseAfterLine[currentLine])
                {
                    SendCustomEventDelayedSeconds(nameof(LineDelay), SubtitleLength[currentLine]);
                }
                else
                {
                    SendCustomEventDelayedSeconds(nameof(PauseLines), SubtitleLength[currentLine]);
                }

                return;
            }

            if (numOfLines > 1 && currentLine > SubtitleLength.Length - 2)
            {
                SubtitleAnimator.CrossFadeInFixedTime("TextFade.Empty", 0);
                
                SendCustomEventDelayedSeconds(nameof(FinishPlayingLines), SubtitleLength[currentLine]);
            }
        }

        public void LineDelay()
        {
            if (!isPlayingLines || isPaused) return;

            if (currentLine >= SubtitleLines.Length)
            {
                Debug.Log("Subtitles have been broken! Call the ResetSubtitles function to fix!");
                return;
            }
            
            var delay = DelayTime[currentLine];

            if (currentLine <= SubtitleLength.Length - 1)
            {
                isDelayed = true;

                if (delay >= 1f)
                {
                    SubtitleAnimator.SetFloat("SpeedMultiplier", 1f);
                    SubtitleAnimator.CrossFadeInFixedTime("TextFade.FadeTextOut", 0);
                }
                else if(delay > 0f)
                {
                    SubtitleAnimator.SetFloat("SpeedMultiplier", Mathf.Lerp(10f, 1f, delay));
                    SubtitleAnimator.CrossFadeInFixedTime("TextFade.FadeTextOut", 0);
                }
                else
                {
                    Text.text = "";
                }
            }
            else
            {
                SubtitleAnimator.CrossFadeInFixedTime("TextFade.Empty", 0);
            }

            SendCustomEventDelayedSeconds(nameof(PlayNextLine), delay);
        }

        public void PlayNextLine()
        {
            currentLine++;
            PlayLineLoop();
        }

        public void PauseLines()
        {
            if (audioDisable && Audio != null)
            {
                Audio.enabled = false;
            }
            isPlayingLines = false;
            isPaused = true;

            if (OptionalTargetBehaviour != null)
            {
                OptionalTargetBehaviour.SendCustomEvent(PauseEventName);
            }
            
            SubtitleAnimator.SetFloat("SpeedMultiplier", 1f);
            SubtitleAnimator.CrossFadeInFixedTime("TextFade.Empty", 0);
            
            SubtitleAnimator.CrossFadeInFixedTime("Base Layer.CloseSubtitle", 0f);
        }

        public void PlayLine(int line)
        {
            if (Audio == null)
            {
                Debug.Log("Audio source not assigned!");
                return;
            }
            
            Audio.PlayOneShot(SubtitleAudio[line]);
        }

        public void FinishPlayingLines()
        {
            if (audioDisable && Audio != null)
            {
                Audio.enabled = false;
            }
            isPlayingLines = false;

            if (OptionalTargetBehaviour != null)
            {
                OptionalTargetBehaviour.SendCustomEvent(EndEventName);
            }
            
            SubtitleAnimator.CrossFadeInFixedTime("Base Layer.CloseSubtitle", 0f);
        }

        #endregion
    }
}
