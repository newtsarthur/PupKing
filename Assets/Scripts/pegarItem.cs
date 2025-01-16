using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pegarItem : MonoBehaviour
{
    public Transform hand;

    public void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "itemColetavel")
        {
            // Tornar o objeto kinemático para evitar interações físicas
            col.gameObject.GetComponent<Rigidbody>().isKinematic = true;

            // Desabilitar o collider do item coletável se ele existir
            BoxCollider itemCollider = col.gameObject.GetComponent<BoxCollider>();
            CapsuleCollider itemColliderCapsule = col.gameObject.GetComponent<CapsuleCollider>();

            // Verificar se o collider do tipo BoxCollider está presente
            if (itemCollider != null)
            {
                itemCollider.enabled = false;
            }

            // Verificar se o collider do tipo CapsuleCollider está presente
            if (itemColliderCapsule != null)
            {
                itemColliderCapsule.isTrigger = true;  // Define a capsule como trigger
                itemColliderCapsule.enabled = false;  // Define a capsule como trigger

            }

            // Posicionar o objeto na mão e setar a rotação
            col.transform.position = hand.position;
            col.transform.rotation = hand.rotation;

            // Definir o objeto como filho da mão
            col.transform.SetParent(hand);
        }    
    }
}
