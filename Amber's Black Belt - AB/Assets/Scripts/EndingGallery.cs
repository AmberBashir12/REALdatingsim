using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingGallery : MonoBehaviour
{
    [Serializable]
    public class Card
    {
        public EndingScene ending;
        public Image image;
        public TMP_Text status;
    }

    [SerializeField] private Card[] cards;

    private void OnEnable()
    {
        foreach (Card card in cards)
        {
            if (card.ending == null || card.image == null || card.status == null)
            {
                continue;
            }

            if (!card.ending.IsObtained)
            {
                continue;
            }

            if (card.ending.background == null)
            {
                continue;
            }

            card.image.sprite = card.ending.background;
            card.image.type = Image.Type.Simple;
            card.status.text = "Obtained";
        }
    }
}
