using UnityEngine;
using UnityEngine.UI;

public class EmojiClueButton : MonoBehaviour
{
    public void OnEmojiPressed()
    {
        Image image = GetComponent<Image>();

        if (image == null)
        {
            Debug.Log("No Image component on " + gameObject.name);
            return;
        }

        if (image.sprite == null)
        {
            Debug.Log("No sprite on " + gameObject.name);
            return;
        }

        Debug.Log("Emoji clicked: " + gameObject.name);
        GameManager.Instance.AddEmojiClue(image.sprite);
    }
}