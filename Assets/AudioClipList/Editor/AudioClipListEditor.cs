using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System;
using System.Linq;

[CustomEditor(typeof(AudioClipList))]

public class AudioClipListEditor : Editor
{
    private const string _enumFile = "ClipName";

    public List<ClipName> HideClipNames = new List<ClipName>();

    [SerializeField] private Texture _playButtonTexture;

    private AudioClipList _audioClipList;
    private string _pathToEnumFile;
    private string _clipName = "New audio clip";
    private AudioSource _audioSource;

    private void OnEnable()
    {

        _audioClipList = (AudioClipList)target;

        _audioSource = _audioClipList.gameObject.GetComponent<AudioSource>();

        _pathToEnumFile = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(_enumFile)[0]);
    }

    public override void OnInspectorGUI()
    {

      
        _audioClipList.Clips = RefreshClips(_audioClipList.Clips);

        for (int clipNumber = 0; clipNumber < _audioClipList.Clips.Count; clipNumber++)
        {
            string clipName = _audioClipList.Clips[clipNumber].Name.ToString();
            bool isHide = HideClipNames.Contains(_audioClipList.Clips[clipNumber].Name);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(clipName);

            DrawPlayButton(_audioClipList.Clips[clipNumber]);

            DrawDeleteClipButton(_audioClipList.Clips[clipNumber]);
            if (isHide)
                DrawShowButton(_audioClipList.Clips[clipNumber].Name);
            else
                DrawHideButton(_audioClipList.Clips[clipNumber].Name);

            EditorGUILayout.EndHorizontal();

            if (isHide == false)
            {
                EditorGUILayout.BeginVertical(GUI.skin.window);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.EndHorizontal();

                _audioClipList.Clips[clipNumber].CurrentAudioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", (_audioClipList.Clips[clipNumber].CurrentAudioClip), typeof(AudioClip), false);
                _audioClipList.Clips[clipNumber].Volume = EditorGUILayout.Slider("Volume", _audioClipList.Clips[clipNumber].Volume, 0f, 1f);
                _audioClipList.Clips[clipNumber].Pitch = EditorGUILayout.FloatField("Pitch", _audioClipList.Clips[clipNumber].Pitch);

                EditorGUILayout.EndVertical();
            }



            EditorGUILayout.EndVertical();
        }

        DrawNewClipSection();
    }

    private void DrawNewClipSection()
    {
        _clipName = EditorGUILayout.TextField("Name", _clipName);
        DrawAddButton();

    }
    private void DrawAddButton()
    {
        if (GUILayout.Button("Add"))
        {
            AddClip();
        }
    }

    private void AddClip()
    {
        if (_clipName == string.Empty)
            return;

        if (!Regex.IsMatch(_clipName, @"^[a-zA-Z][a-zA-z0-9_]*$"))
            return;

        Array array = Enum.GetValues(typeof(ClipName));

        if (array.Length != 0)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (_clipName == array.GetValue(i).ToString())
                {
                    Debug.LogError("A clip whith the same name has already exist");
                    return;
                }
            }
        }

        EnumEditor.WriteToFile(_clipName, _pathToEnumFile);
        Refresh();
    }

    private void DrawPlayButton(Clip clip)
    {
        if (GUILayout.Button(_playButtonTexture, GUILayout.Width(25), GUILayout.Height(25)))
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }

            _audioSource.pitch = clip.Pitch;
            _audioSource.volume = clip.Volume;
            _audioSource.clip = clip.CurrentAudioClip;
            _audioSource.Play();
        }
    }

    private void RemoveClip(Clip clip)
    {
        if (!EnumEditor.TryRemoveFromFile(clip.Name.ToString(), _pathToEnumFile))
            return;

        Refresh();
    }

    private void Refresh()
    {
        Debug.Log("WAIT");

        var realivePath = _pathToEnumFile.Substring(_pathToEnumFile.IndexOf("Assets"));
        AssetDatabase.ImportAsset(realivePath);
    }

    private List<Clip> RefreshClips(List<Clip> oldClips)
    {
        int countClip = Enum.GetNames(typeof(ClipName)).Length;
        List<Clip> clips = new List<Clip>(countClip);

        for (int i = 0; i < countClip; i++)
        {
            ClipName clipName = (ClipName)i;
            Clip clip = TryRestoreClip(oldClips, clipName.ToString());

            if (clip == null)
            {
                clip = CreateNewClip(clipName);
            }

            clips.Add(clip);
        }

        return clips;
    }

    private Clip TryRestoreClip(List<Clip> oldClips, string name)
    {
        return oldClips.FirstOrDefault(o => o.Name.ToString() == name);
    }

    private Clip CreateNewClip(ClipName clipName)
    {
        Clip clip = new Clip
        {
            Name = clipName
        };

        return clip;
    }

    private void DrawHideButton(ClipName clipName)
    {
        if (GUILayout.Button("⇓", GUILayout.Width(20), GUILayout.Height(20)))
        {
            HideClipNames.Add(clipName);
        }
    }

    private void DrawShowButton(ClipName clipName)
    {
        if (GUILayout.Button("⇒", GUILayout.Width(20), GUILayout.Height(20)))
        {
            HideClipNames.Remove(clipName);
        }
    }

    private void DrawDeleteClipButton(Clip clip)
    {
        if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
        {
            RemoveClip(clip);
        }
    }
}
