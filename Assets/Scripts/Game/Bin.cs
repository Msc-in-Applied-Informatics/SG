using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bin : MonoBehaviour
{
    public TrashItem.TrashType acceptedType;

    public Sprite paperSprite;
    public Sprite plasticSprite;
    public Sprite glassSprite;
    public Sprite aluminumSprite;
    public Sprite organicSprite;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // Παίρνουμε τον SpriteRenderer του κάδου
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Bin!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TrashItem item = collision.GetComponent<TrashItem>();
        if (item != null)
        {
            Debug.Log(item.trashType + " - " + acceptedType);

            if (item.trashType == acceptedType)
            {
                GameManager.Instance.OnCorrectTrashCollected();
            }
            else
            {
                GameManager.Instance.LoseLife();
            }

            Destroy(item.gameObject);
        }
    }


    public void SetBinType(TrashItem.TrashType type)
    {
        acceptedType = type;

        switch (type)
        {
            case TrashItem.TrashType.Paper:
                spriteRenderer.sprite = paperSprite;
                break;
            case TrashItem.TrashType.Plastic:
                spriteRenderer.sprite = plasticSprite;
                break;
            case TrashItem.TrashType.Glass:
                spriteRenderer.sprite = glassSprite;
                break;
            case TrashItem.TrashType.Aluminum:
                spriteRenderer.sprite = aluminumSprite;
                break;
            case TrashItem.TrashType.Organic:
                spriteRenderer.sprite = organicSprite;
                break;
            default:
                spriteRenderer.sprite = paperSprite;
                break;
        }
    }

    public void SetBinColor(int level)
    {
        // (προαιρετικά) αλλαγή χρώματος στο sprite αν θες να δώσεις έμφαση
        Color color = Color.white;

        switch (level)
        {
            case 1: color = Color.green; break;
            case 2: color = Color.blue; break;
            case 3: color = Color.yellow; break;
            case 4: color = Color.red; break;
            default: color = Color.white; break;
        }

        spriteRenderer.color = color;
    }

    public void SetRandomBinColor()
    {
        Color[] colors = { Color.green, Color.blue, Color.yellow, Color.red };
        spriteRenderer.color = colors[Random.Range(0, colors.Length)];
    }
}
