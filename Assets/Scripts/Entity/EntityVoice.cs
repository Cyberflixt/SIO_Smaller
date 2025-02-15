using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityVoice_", menuName = "Entities/Voice", order = 1)]
public class EntityVoice : ScriptableObject
{
    public SoundVariant attackSound = new SoundVariant("Humph");
    public SoundVariant hurtSound = new SoundVariant("Humph");
    public SoundVariant deathSound = new SoundVariant("Humph");
    public SoundVariant jumpSound = new SoundVariant("Humph");
}
