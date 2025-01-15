using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    public Transform player; // Referência ao jogador
    public Vector3 offset;   // Offset da câmera em relação ao jogador
    public float followSpeed = 10f; // Velocidade de seguimento
    public float rotationSpeed = 5f; // Velocidade de rotação da câmera

    private bool isRunning; // Indica se o jogador está correndo

    void Update()
    {
        // Verifica se o jogador está correndo
        isRunning = Input.GetKey(KeyCode.LeftShift) && player.GetComponent<Rigidbody>().velocity.magnitude > 0.1f;
    }

    void LateUpdate()
    {
        if (isRunning)
        {
            // Define a posição desejada da câmera atrás do jogador
            Vector3 desiredPosition = player.position + player.rotation * offset;

            // Move a câmera suavemente para a posição desejada
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

            // Faz a câmera olhar para o jogador
            Quaternion desiredRotation = Quaternion.LookRotation(player.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
