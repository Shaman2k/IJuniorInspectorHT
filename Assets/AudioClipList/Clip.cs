using UnityEngine;
using System;

[Serializable]
public class Clip
{
    public ClipName Name;
    public AudioClip CurrentAudioClip;
    public float Volume;
    public float Pitch;
}