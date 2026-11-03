using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class BackgroundSwitcher : MonoBehaviour
{

 public bool isSwitched = false;
    public Image Image1;
    public Image Image2;
    private Animator animator;
    public Sprite CurrentBackground { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SwitchImage(Sprite sprite)
    {
        CurrentBackground = sprite;
        if(!isSwitched)
        {
            Image1.sprite = sprite;
            animator.SetTrigger("SwitchZero");
        }
        else
        {
            Image2.sprite = sprite; 
            animator.SetTrigger("SwitchOne");
        }
        isSwitched = !isSwitched;
    }
    public void SetImage(Sprite sprite)
    {
        CurrentBackground = sprite;
        if (!isSwitched)
        {
            Image1.sprite = sprite;
        }
        else
        {
            Image2.sprite = sprite;
        }
    }
    public Sprite GetImage()
    {
        if(!isSwitched)
        {
            return Image1.sprite;
        }
        else
        {
            return Image2.sprite;
        }
    }

}