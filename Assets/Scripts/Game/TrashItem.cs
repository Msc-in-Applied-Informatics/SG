using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrashItem : MonoBehaviour
{
    public enum TrashType { Paper, Plastic, Glass, Aluminum, Organic, Electronics, Battery, NonRecyclable };
    public TrashType trashType;

    public float fallSpeed = 2f;

    // Update is called once per frame
    void Update()
    {
        // Η κίνηση του σκουπιδιού προς τα κάτω
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        // Έλεγχος αν το σκουπίδι πέφτει εκτός οθόνης
        if (transform.position.y < -6f)
        {
            // Κλήση της συνάρτησης που αφαιρεί ζωή από τον GameManager
            //GameManager.Instance.LoseLife();

            // Καταστρέφει το αντικείμενο όταν φτάνει κάτω
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Collision with: " + other.name);

        if (other.CompareTag("Bin"))
        {
            Debug.Log("It's a bin!");

            Bin bin = other.GetComponent<Bin>();
            if (bin != null)
            {
                //Debug.Log("Bin script found, comparing trash types");

                if (bin.acceptedType == trashType)
                {
                    //Debug.Log("Correct bin! Destroying trash.");
                    Destroy(gameObject);
                }
                else
                {
                    //Debug.Log("Wrong bin! Destroying trash.");
                    //GameManager.Instance.LoseLife();
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning("Bin script NOT found on object: " + other.name);
            }
        }
    }
}