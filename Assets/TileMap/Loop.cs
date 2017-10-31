using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loop : MonoBehaviour 
{
    public GameObject other;

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            coll.gameObject.SetActive(false);
            coll.gameObject.transform.position = new Vector3(other.transform.position.x, coll.gameObject.transform.position.y, coll.gameObject.transform.position.z);
            coll.gameObject.SetActive(true);
        }

        if(coll.gameObject.tag == "Projectile") 
        {
            var temp = Instantiate(coll.gameObject, new Vector3(other.transform.position.x, coll.gameObject.transform.position.y, coll.gameObject.transform.position.z), coll.gameObject.transform.rotation);
            temp.GetComponent<Rigidbody2D>().velocity = coll.gameObject.GetComponent<Rigidbody2D>().velocity;
            Destroy(coll.gameObject, 2.0f);
        }
        
        

    }
}
