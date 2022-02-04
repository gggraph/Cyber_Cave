using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimation : MonoBehaviour
{
    public string _info = "";
    // when this item is highlight in menu selection
    public virtual void SetHighLight(){}
    // when this item is not highlight in menu selection
    public virtual void DisableHighLight() { }
}

public class MenuAnimation_Default : MenuAnimation
{
    public GameObject quote;

    public void CreateQuoteObject()
    {
        quote = new GameObject();
        quote.transform.position = new Vector3(this.transform.position.x, this.transform.position.y+1.5f, this.transform.position.z);
        quote.transform.parent = this.transform;
        TextMesh t = quote.AddComponent<TextMesh>();
        t.text = _info;
        t.characterSize = 0.2f;
        t.anchor = TextAnchor.MiddleCenter;
        quote.AddComponent<LookAtCamera>();
        quote.transform.localScale = new Vector3(-1, 1, 1);
    }
    public override void SetHighLight()
    {
        if (!quote)
            CreateQuoteObject();


        ObjectUtilities.FadeInObject(quote, 1f);
        AutoRotate r = gameObject.AddComponent<AutoRotate>();
        r.SetSpeed(0.2f);
        base.SetHighLight();
    }
    public override void DisableHighLight()
    {
        if (!quote)
            CreateQuoteObject();

        ObjectUtilities.FadeOutObject(quote, 1f);
        Destroy(gameObject.GetComponent<AutoRotate>());
        base.DisableHighLight();
    }
}
public class MenuAnimation_SoundObject : MenuAnimation
{

    
    public override void SetHighLight()
    {
       
        gameObject.GetComponent<AudioSource>().Play();
        AutoRotate r = gameObject.AddComponent<AutoRotate>();
        r.SetSpeed(0.2f);
        base.SetHighLight();
    }
    public override void DisableHighLight()
    {
        gameObject.GetComponent<AudioSource>().Stop();
        Destroy(gameObject.GetComponent<AutoRotate>());
        base.DisableHighLight();
    }
}

public class AnimationForMenu : MonoBehaviour
{
}
