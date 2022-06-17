using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    private List<Renderer> rends = new List<Renderer>();
    float maxalpha = 1f;
    public int fadeMode = 0; // 0: is static, 1: is fade In, 2 : is Fade Out, 3 : is Fade Out & Destroy
    public float elapsedTime = 0f;
    public float fadeDuration = 0f;
    bool _rendset = false;


    bool _resetMaterial = true;

    public List<float> defaultAlpha = new List<float>();
    void GetRenderers()
    {

        List<GameObject> allgo = new List<GameObject>();
        ObjectUtilities.GetChildsFromParent(this.gameObject, allgo); // recursive loop
        allgo.Add(this.gameObject);
        foreach (GameObject go in allgo)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r != null)
            {
                rends.Add(r);
                //defaultAlpha.Add(r.material.color.a);
            }
        }


        _rendset = true;
    }


    public void FadeIn(float duration = 2f, float maxAlpha = 1f)
    {
        if (!_rendset)
            GetRenderers();
        if (fadeMode == 3)
            return;
        maxalpha = 0.9f;
        fadeDuration = duration;
        fadeMode = 1;
        elapsedTime = 0f;



    }
    public void FadeOut(float duration = 2f, float minALpha = 0f)
    {
        if (!_rendset)
            GetRenderers();
        if (fadeMode == 3)
            return;
        fadeDuration = duration;
        elapsedTime = 0f;
        fadeMode = 2;

        if (_resetMaterial)
        {
            for (int i = 0; i < rends.Count; i++)
            {
                //rend.materials = new Material[1] { m };
                if (rends[i].materials.Length > 0)
                {
                    foreach (Material m in rends[i].materials)
                    {
                        MaterialUtilities.SetMaterialToFadeMode(m);
                    }
                }
                else
                {
                    MaterialUtilities.SetMaterialToFadeMode(rends[i].material);
                }

            }
        }



    }
    public void FadeOutAndDestroy(float duration = 2f)
    {
        if (!_rendset)
            GetRenderers();
        fadeDuration = duration;
        elapsedTime = 0f;
        fadeMode = 3;

    }

    private void Update()
    {
        if (fadeMode == 1)
        {
            if (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                for (int i = 0; i < rends.Count; i++)
                {

                    rends[i].material.color = new Color(rends[i].material.color.r, rends[i].material.color.g, rends[i].material.color.b, (elapsedTime / fadeDuration) * (maxalpha));

                }
            }
            else
            {
                if (_resetMaterial)
                {
                    for (int i = 0; i < rends.Count; i++)
                    {

                        if (rends[i].materials.Length > 0)
                        {
                            foreach (Material m in rends[i].materials)
                            {
                                MaterialUtilities.ChangeRenderMode(m, MaterialUtilities.BlendMode.Opaque);
                            }
                        }
                        else
                        {
                            MaterialUtilities.ChangeRenderMode(rends[i].material, MaterialUtilities.BlendMode.Opaque);
                        }

                    }
                }
                fadeMode = 0;
                elapsedTime = 0;
                return;
            }
        }
        if (fadeMode == 2)
        {
            if (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                for (int i = 0; i < rends.Count; i++)
                {
                    rends[i].material.color = new Color(rends[i].material.color.r, rends[i].material.color.g, rends[i].material.color.b, 1f - (elapsedTime / fadeDuration));
                    // Debug.Log("Apply fade Out to object " + rends[i].gameObject.name);
                }

            }
            else
            {
                fadeMode = 0;
                elapsedTime = 0;
                return;
            }
        }
        if (fadeMode == 3)
        {
            if (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                for (int i = 0; i < rends.Count; i++)
                {
                    rends[i].material.color = new Color(rends[i].material.color.r, rends[i].material.color.g, rends[i].material.color.b, 1f - (elapsedTime / fadeDuration));
                }

            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }


}
