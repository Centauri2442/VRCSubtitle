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
using System.Collections.Generic;
using TMPro;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

namespace Centauri.SubtitleSystem
{
    [CustomEditor(typeof(SubtitleScript))]

    public class SubtitleScriptEditor : Editor
    {
        private SubtitleScript script;
        private List<SubtitleLine> lines = new List<SubtitleLine>();
        
        private GUIContent delayTooltip;
        private GUIContent pauseTooltip;
        private GUIContent typewriterLinear;

        private SerializedProperty propAudioSource;
        private SerializedProperty propSubtitleAnimator;
        private SerializedProperty propText;
        private SerializedProperty propUdonBehaviour;
        private SerializedProperty propPauseEvent;
        private SerializedProperty propEndEvent;
        private SerializedProperty propFadeType;
        private SerializedProperty propMainPropertiesOpen;
        private SerializedProperty propTypewriterLinear;
        private SerializedProperty propExtraPropertiesOpen;
        private SerializedProperty propAudioDisable;

        private Texture2D Logo;

        private void OnEnable()
        {
            script = target as SubtitleScript;
            if (script == null) return;

            propAudioSource = serializedObject.FindProperty(nameof(script.Audio));
            propSubtitleAnimator = serializedObject.FindProperty(nameof(script.SubtitleAnimator));
            propText = serializedObject.FindProperty(nameof(script.Text));
            propUdonBehaviour = serializedObject.FindProperty(nameof(script.OptionalTargetBehaviour));
            propPauseEvent = serializedObject.FindProperty(nameof(script.PauseEventName));
            propEndEvent = serializedObject.FindProperty(nameof(script.EndEventName));
            propFadeType = serializedObject.FindProperty(nameof(script.FadeType));
            propMainPropertiesOpen = serializedObject.FindProperty(nameof(script.mainSettingsOpen));
            propTypewriterLinear = serializedObject.FindProperty(nameof(script.TypewriterLinear));
            propExtraPropertiesOpen = serializedObject.FindProperty(nameof(script.extraSettingsOpen));
            propAudioDisable = serializedObject.FindProperty(nameof(script.audioDisable));

            Logo = Resources.Load<Texture2D>("VRCSubtitleLogo");
            
            
            Undo.undoRedoPerformed += PullFromScript;
            PullFromScript();

            delayTooltip = new GUIContent("Delay Time (s)", "This is the amount of delay in seconds AFTER a subtitle has been played with NO TEXT shown!");
            pauseTooltip = new GUIContent("Pause", "This controls whether the system will stop after this line plays, of which it will resume upon calling the start StartPlayingLines event again");
            typewriterLinear = new GUIContent("Consistent Type Speed", "If enabled, this will force every set of subtitles to be typed at a constant speed, at the expense of the possibility of the line lasting longer than it would without it.");
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= PullFromScript;
        }

        public override void OnInspectorGUI()
        {
            HeaderGUI();
            GUILayout.Space(5f);
            UdonSharpGUI.DrawUILine();
            GUILayout.Space(5f);
            ListGUI();
        }

        private void HeaderGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Box(Logo, GUIStyle.none);
                GUILayout.FlexibleSpace();
            }
            
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    propMainPropertiesOpen.boolValue = EditorGUILayout.Foldout(propMainPropertiesOpen.boolValue, "Main Settings", true , EditorStyles.boldLabel);
                }

                if (propMainPropertiesOpen.boolValue)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.PropertyField(propAudioSource, new GUIContent("Audio Source", "This is the audio source that audio clips will play from!"));
                        EditorGUILayout.PropertyField(propSubtitleAnimator, new GUIContent("Subtitle Animator", "Animator that holds and controls subtitle animation visuals!"));
                        EditorGUILayout.PropertyField(propText, new GUIContent("Subtitle Text Field", "TextMeshPro UI element that displays the text!"));
                    }
            
                    GUILayout.Space(10);
            
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.PropertyField(propUdonBehaviour, new GUIContent($"(Optional) Target Udon Behaviour"));
                        
                        if (propUdonBehaviour.objectReferenceValue != null)
                        {
                            EditorGUILayout.Space(5);
                            propPauseEvent.stringValue = EditorGUILayout.TextField("Pause Event Name", propPauseEvent.stringValue);
                            propEndEvent.stringValue = EditorGUILayout.TextField("End Event Name", propEndEvent.stringValue);
                        }
                    }
            
                    GUILayout.Space(10);
            
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.PropertyField(propFadeType);

                        if (propFadeType.enumValueIndex == 1)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.Space(370);
                                GUILayout.Label(typewriterLinear);
                                propTypewriterLinear.boolValue = EditorGUILayout.Toggle(propTypewriterLinear.boolValue, GUILayout.Width(30));
                            }
                        }
                        else
                        {
                            propTypewriterLinear.boolValue = false;
                        }
                    }
                }
            }
            
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    propExtraPropertiesOpen.boolValue = EditorGUILayout.Foldout(propExtraPropertiesOpen.boolValue, "Extra Settings", true , EditorStyles.boldLabel);
                }

                if (propExtraPropertiesOpen.boolValue)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(285);
                            GUILayout.Label(new GUIContent("Disable Audio Source When Not In Use", "Disable audio source when it is not actively being used by the system!"));
                            propAudioDisable.boolValue = EditorGUILayout.Toggle(propAudioDisable.boolValue, GUILayout.Width(30));
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ListGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            for (int i = 0; i < lines.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    lines[i].isOpen = EditorGUILayout.Foldout(lines[i].isOpen, $"Subtitle {i+1}", true , EditorStyles.boldLabel);

                    if (GUILayout.Button("▲", GUILayout.Width(20)))
                    {
                        ReorderList(i, -1);
                    }

                    if (GUILayout.Button("▼", GUILayout.Width(20)))
                    {
                        ReorderList(i, 1);
                    }

                    if (GUILayout.Button("✖", GUILayout.Width(20)))
                    {
                        lines.RemoveAt(i);

                        break;
                    }
                }

                if (lines[i].isOpen)
                {
                    lines[i].SubtitleLines = EditorGUILayout.TextArea(lines[i].SubtitleLines);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (lines[i].UsesClip)
                        {
                            GUILayout.Label("Audio Clip");
                            lines[i].SubtitleAudio = (AudioClip)EditorGUILayout.ObjectField(lines[i].SubtitleAudio, typeof(AudioClip), false, GUILayout.Width(250));

                            if (lines[i].SubtitleAudio != null)
                            {
                                lines[i].SubtitleLength = lines[i].SubtitleAudio.length;
                            }
                            
                            if (GUILayout.Button("✖"))
                            {
                                script.SubtitleAudio[i] = null;
                                lines[i].SubtitleAudio = null;
                                lines[i].UsesClip = false;
                                lines[i].SubtitleLength = 0f;
                            }
                        }
                        else
                        {
                            GUILayout.Label("Subtitle Length (s)");
                            lines[i].SubtitleLength = EditorGUILayout.FloatField(lines[i].SubtitleLength, GUILayout.Width(50));

                            if (GUILayout.Button("Add Audio Clip"))
                            {
                                lines[i].UsesClip = true;
                            }
                        }

                        GUILayout.Label(delayTooltip);
                        lines[i].DelayTime = EditorGUILayout.FloatField(lines[i].DelayTime, GUILayout.Width(50));
                    }
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(pauseTooltip);
                    lines[i].pauseAfterLine = EditorGUILayout.Toggle(lines[i].pauseAfterLine, GUILayout.Width(30));
                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add"))
            {
                lines.Add(new SubtitleLine());
            }

            if (EditorGUI.EndChangeCheck())
            {
                PushToScript();
            }
        }

        private void PullFromScript()
        {
            int lineCount = 0;
            if (script.SubtitleLines != null &&
                script.SubtitleAudio != null &&
                script.DelayTime != null &&
                script.SubtitleLength != null
                && script.isOpen != null && script.pauseAfterLine != null)
            {
                lineCount = Mathf.Min(script.SubtitleLines.Length, script.SubtitleAudio.Length, script.DelayTime.Length, script.SubtitleLength.Length, script.SubtitleLines.Length, script.isOpen.Length, script.pauseAfterLine.Length);
            }
        
            lines = new List<SubtitleLine>();
        
            for (int i = 0; i < lineCount; i++)
            {
                SubtitleLine line = new SubtitleLine
                {
                    SubtitleLines = script.SubtitleLines[i],
                    SubtitleAudio = script.SubtitleAudio[i],
                    DelayTime = script.DelayTime[i],
                    SubtitleLength = script.SubtitleLength[i],
                    UsesClip = script.SubtitleAudio[i],
                    isOpen = script.isOpen[i],
                    pauseAfterLine = script.pauseAfterLine[i]
                };
        
                lines.Add(line);
            }
        }

        private void PushToScript()
        {
            List<string> tempSubtitles = new List<string>();
            List<AudioClip> tempClips = new List<AudioClip>();
            List<float> tempSubtitleLength = new List<float>();
            List<float> tempDelayTimes = new List<float>();
            List<string> tempLabels = new List<string>();
            List<bool> tempIsOpen = new List<bool>();
            List<bool> tempPause = new List<bool>();

            foreach (SubtitleLine line in lines)
            {
                tempSubtitles.Add(line.SubtitleLines);
                tempClips.Add(line.SubtitleAudio);
                tempSubtitleLength.Add(line.SubtitleLength);
                tempDelayTimes.Add(line.DelayTime);
                tempLabels.Add(line.Label);
                tempIsOpen.Add(line.isOpen);
                tempPause.Add(line.pauseAfterLine);
            }

            Undo.RecordObject(script, "Pushed Values to Subtitles");

            script.SubtitleLines = tempSubtitles.ToArray();
            script.SubtitleAudio = tempClips.ToArray();
            script.SubtitleLength = tempSubtitleLength.ToArray();
            script.DelayTime = tempDelayTimes.ToArray();
            script.isOpen = tempIsOpen.ToArray();
            script.pauseAfterLine = tempPause.ToArray();
        }

        private void ReorderList(int index, int change)
        {
            int changeDelta = index + change;
            if (changeDelta < 0 || changeDelta >= lines.Count) return;
            Undo.RecordObject(script, "Reordered List");

            SubtitleLine item = lines[index];
            lines.RemoveAt(index);
            lines.Insert(changeDelta, item);
        }
    }
    
    [Serializable]
    public class SubtitleLine
    {
        public string Label;
        public string SubtitleLines;
        public AudioClip SubtitleAudio;
        public float DelayTime;
        public float SubtitleLength;
        public bool UsesClip;
        public bool isOpen = true;
        public bool pauseAfterLine;
    }
}
