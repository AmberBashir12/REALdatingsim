using System.Collections;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    private SpriteSwitcher switcher;
    private Animator animator;
    private Transform rect;

    private void Awake()
    {
        switcher = GetComponent<SpriteSwitcher>();
        animator = GetComponent<Animator>();
        rect = GetComponent<Transform>();
    }

    public void Setup(Sprite sprite)
    {
        switcher.SetImage(sprite);
    }

    public void SetTint(Color color)
    {
        switcher.SetTint(color);
    }

    public void ResetTint()
    {
        switcher.SetTint(Color.white);
    }

    public void Show(Vector2 coords)
    {
        if (animator != null)
        {
            animator.SetTrigger("Show");
        }
        if (rect != null)
        {
            rect.localPosition = coords;
        }
    }

    public void Bounce()
    {
        if (animator != null)
        {
            animator.SetTrigger("Bounce");
        }
    }

    public void Hide()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hide");
        }
    }

    public void Move(Vector2 coords, float speed)
    {
        StartCoroutine(MoveCoroutine(coords, speed));
    }

    private IEnumerator MoveCoroutine(Vector2 coords, float speed)
    {
        while (rect.localPosition.x != coords.x || rect.localPosition.y != coords.y)
        {
            rect.localPosition = Vector2.MoveTowards(rect.localPosition, coords,
                Time.deltaTime * 1000 * speed);
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void SwitchSprite(Sprite sprite)
    {
        if(switcher.GetImage() != sprite)
        {
            switcher.SwitchImage(sprite);
        }
    }
}


