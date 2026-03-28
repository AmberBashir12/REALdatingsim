using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class SpriteSwitcher : MonoBehaviour
{
    public bool isSwitched = false;
    public Image Image1;
    public Image Image2;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SwitchImage(Sprite sprite)
    {
        if(!isSwitched)
        {
            Image2.sprite = sprite;
            animator.SetTrigger("Switch1");
        }
        else
        {
            Image1.sprite = sprite; 
            animator.SetTrigger("Switch2");
        }
        isSwitched = !isSwitched;
    }

    public void SetImage(Sprite sprite)
    {
        if (Image1 != null)
        {
            Image1.sprite = sprite;
        }

        if (Image2 != null)
        {
            Image2.sprite = sprite;
        }
    }

    public void SetTint(Color color)
    {
        if (Image1 != null)
        {
            Image1.color = color;
        }

        if (Image2 != null)
        {
            Image2.color = color;
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
