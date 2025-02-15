using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_StealthAttack : MonoBehaviour
{
    [SerializeField] private RectTransform image;

    public static UI_StealthAttack instance;
    private int animToken = 0;
    private bool visible = false;

    public void Hide(){
        if (visible){
            visible = false;
            animToken++;
            StartCoroutine(HideAnim());
        }
    }
    private void Show(List<Transform> transforms){
        RefreshBounds(transforms);
        if (!visible){
            visible = true;
            animToken++;
            StartCoroutine(ShowAnim());
        }
    }
    
    private void IncludeBounds(Vector3 point, Camera cam, ref float minx, ref float miny, ref float maxx, ref float maxy){
        Vector3 flat = cam.WorldToScreenPoint(point);
        if (flat.z > 0){
            if (flat.x < minx)
                minx = flat.x;
            if (flat.y < miny)
                miny = flat.y;
            if (flat.x > maxx)
                maxx = flat.x;
            if (flat.y > maxy)
                maxy = flat.y;
        }
    }

    private void RefreshBounds(List<Transform> transforms){
        Camera cam = Camera.main;
        Vector3 flat = cam.WorldToScreenPoint(transforms[0].position);

        float minx = flat.x;
        float miny = flat.y;
        float maxx = minx;
        float maxy = miny;
        
        foreach (Transform transform in transforms){
            CapsuleCollider capsule = transform.GetComponent<CapsuleCollider>();
            Vector3 cen = transform.position + capsule.center;
            IncludeBounds(cen - capsule.height / 2 * Vector3.up , cam, ref minx, ref miny, ref maxx, ref maxy);
            IncludeBounds(cen + capsule.height / 2 * Vector3.up , cam, ref minx, ref miny, ref maxx, ref maxy);
            IncludeBounds(cen - capsule.radius * Vector3.right  , cam, ref minx, ref miny, ref maxx, ref maxy);
            IncludeBounds(cen + capsule.radius * Vector3.right  , cam, ref minx, ref miny, ref maxx, ref maxy);
            IncludeBounds(cen - capsule.radius * Vector3.forward, cam, ref minx, ref miny, ref maxx, ref maxy);
            IncludeBounds(cen + capsule.radius * Vector3.forward, cam, ref minx, ref miny, ref maxx, ref maxy);
        }

        image.localPosition = new Vector3((maxx+minx)/2, (maxy+miny)/2);
        image.sizeDelta = new Vector2(maxx - minx, maxy - miny);
    }

    public static void SetTransforms(List<Transform> transforms){
        if (instance == null) return;

        if (transforms.Count == 0){
            instance.Hide();
        } else {
            instance.Show(transforms);
        }
    }

    private IEnumerator HideAnim(){
        float duration = .2f;
        float start = Time.time;
        int token = animToken;

        while (Time.time < start + duration && animToken == token){
            float alpha = (Time.time - start) / duration;
            alpha = alpha * alpha;

            image.localScale = Vector3.one * (1f + alpha * .3f);

            if (alpha * 4 % 2 > 1){
                image.gameObject.SetActive(false);
            } else {
                image.gameObject.SetActive(true);
            }
            yield return null;
        }
        image.gameObject.SetActive(false);
    }
    private IEnumerator ShowAnim(){
        float duration = .2f;
        float start = Time.time;
        int token = animToken;

        while (Time.time < start + duration && animToken == token){
            float alpha = (Time.time - start) / duration;
            alpha = 1 - (1-alpha) * (1-alpha);

            image.localScale = Vector3.one * (1.3f - alpha * .3f);

            if (alpha * 4 % 2 > 1){
                image.gameObject.SetActive(true);
            } else {
                image.gameObject.SetActive(false);
            }
            yield return null;
        }
        image.gameObject.SetActive(true);
    }


    // Event
    void Awake()
    {
        instance = this;

        visible = false;
        image.gameObject.SetActive(false);
        Hide();
    }
}
