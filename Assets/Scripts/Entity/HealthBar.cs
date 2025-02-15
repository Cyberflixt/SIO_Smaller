using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public EntityBase entity;
    public RectTransform bar;

    private void Refresh(){
        // Refresh bar progress
        float progress = entity.health / entity.healthMax;
        bar.anchorMax = new Vector2(progress, 1);
    }
    void Start(){
        // Refresh bar when health changes
        entity.onHealthChanged += Refresh;
        Refresh();
    }
}
