using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{

    public bool isSwitched = false;
    public Image Background1;
    public Image Background2;
    public Animator animator;

    public void SwitchImage(Sprite sprite)
    {
        if(!isSwitched)
        {
            Background1.sprite = sprite;
            animator.SetTrigger("SwitchZero");
        }
        else
        {
            Background2.sprite = sprite;
            animator.SetTrigger("SwitchOne");
        }
        isSwitched = !isSwitched;
    }

    public void SetImage(Sprite sprite)
    {
        if(!isSwitched)
        {
            Background1.sprite = sprite;
        }
        else
        {
            Background2.sprite = sprite;
        }
    }

}
