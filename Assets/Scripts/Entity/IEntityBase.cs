using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityBase
{
    public float healthMax { get; set; }
    public void TakeDamage(float damage, Vector3 direction);
}
