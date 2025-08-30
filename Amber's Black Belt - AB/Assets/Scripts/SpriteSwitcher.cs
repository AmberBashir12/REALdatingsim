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
        if (Image1 == null || Image2 == null)
        {
            Debug.LogError("Image1 or Image2 not assigned in SpriteSwitcher! Please assign them in the Inspector.");
            return;
        }
        
        if (animator == null)
        {
            Debug.LogError("Animator component missing on SpriteSwitcher!");
            return;
        }
        
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
        if (Image1 == null || Image2 == null)
        {
            Debug.LogError("Image1 or Image2 not assigned in SpriteSwitcher! Please assign them in the Inspector.");
            return;
        }
        
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
