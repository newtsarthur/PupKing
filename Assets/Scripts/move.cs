using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float jumpForce;
    public LayerMask groundLayer; // A camada que o chão deve ter
    public Transform peDoPersonagem; // Transform do pé do personagem (para verificação de colisão com o chão)
    public float groundCheckRadius = 0.3f; // Raio para verificar se o personagem está no chão
    public float fallMultiplier = 2.5f; // Multiplicador de aceleração para a queda
    public float lowJumpMultiplier = 2f; // Multiplicador para pulo baixo (ajusta o tempo de subida)
    public Camera myCamera;
    public CharacterController controller;
    public float tempoPressionado;
    public float tempoMaxPulo;

    private int pulos = 1;
    private Rigidbody rb;
    private bool isGrounded;
    private Animator anim;
    private float forcaY;
    private bool podePular = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        MovePlayer();
        RotationPlayer();
        Jump();
        Run();
        ApplyFallAcceleration(); // Aplica aceleração de queda no ar
    }

    void MovePlayer()
    {
        // Obtém o valor de entrada vertical (W/S)
        float vertical = Input.GetAxis("Vertical");

        if (vertical > 0) // Move para frente com W
        {
            anim.SetBool("Walk", true); // Ativa a animação de andar

            // Cria o vetor de movimento baseado na direção da câmera
            Vector3 movement = new Vector3(0, 0, vertical);
            movement = myCamera.transform.TransformDirection(movement); // Aplica a direção da câmera
            movement.y = 0; // Impede que o personagem suba ou desça

            // Move o personagem para frente na direção que ele está olhando
            controller.Move(movement * Time.deltaTime * speed);

            // Faz o personagem olhar para a direção do movimento
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * rotationSpeed * 5f);
        }
        else
        {
            anim.SetBool("Walk", false); // Desativa a animação de andar quando não há movimento
        }

        // Aplica a gravidade
        controller.Move(new Vector3(0, -9.81f, 0) * Time.deltaTime);
    }

    void Run()
    {
        // Obtém os valores de entrada para movimentação
        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");

        // Cria um vetor de movimento baseado na entrada
        Vector3 mov = new Vector3(moveH, 0f, moveV);

        // Verifica se há movimento e se a tecla Shift está pressionada
        if (mov.magnitude >= 0.1f && Input.GetKey(KeyCode.LeftShift))
        {
            anim.SetBool("Run", true); // Ativa a animação de correr
            speed = 30f; // Define a velocidade de corrida
            mov = mov.normalized; // Normaliza o vetor de movimento para manter direção
        }
        else if (mov.magnitude >= 0.1f) // Caso esteja se movendo, mas Shift não está pressionado
        {
            anim.SetBool("Run", false); // Desativa a animação de correr
            speed = 10f; // Define uma velocidade normal
        }
        else
        {
            anim.SetBool("Run", false); // Desativa a animação de movimento
            speed = 10f; // Personagem parado
        }

        // Atualiza a posição do personagem no mundo (opcional dependendo do motor)
        transform.Translate(mov * speed * Time.deltaTime);
    }

    void RotationPlayer()
    {
        // Obtém os inputs de movimento rotacional
        float horizontal = Input.GetAxis("Horizontal");

        // Se estiver pressionando A ou D, gira o personagem em torno do eixo Y
        if (horizontal != 0)
        {
            transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);
        }
    }

    void Jump()
    {
        // Verifica se o personagem está tocando o chão usando Physics.CheckSphere
        isGrounded = Physics.CheckSphere(peDoPersonagem.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            pulos = 1; // Permite pular ao tocar no chão
            forcaY = 0f; // Reseta a força vertical
            tempoPressionado = 0f; // Permite reiniciar o tempo do pulo
            podePular = true; // Habilita a possibilidade de pular
            anim.SetBool("Jump", false); // Reseta a animação de pulo

            // Verifica se o botão de pulo foi pressionado
            if (Input.GetButtonDown("Jump") && podePular && pulos == 1)
            {
                pulos = 0; // Bloqueia pulo enquanto no ar
                forcaY = jumpForce; // Aplica a força inicial do pulo
                podePular = false; // Bloqueia pulo adicional enquanto no ar
                anim.SetBool("Jump", true); // Ativa a animação de pulo
            }
        }
        else
        {
            // Aplica gravidade continuamente enquanto no ar
            forcaY += Physics.gravity.y * Time.deltaTime;

            // Se o botão de pulo estiver pressionado e dentro do limite de tempo
            if (Input.GetButton("Jump") && tempoPressionado < tempoMaxPulo)
            {
                pulos = 0;
                tempoPressionado += Time.deltaTime; // Incrementa o tempo pressionado
                // Aplica força de pulo suavemente enquanto o botão está pressionado
                forcaY = Mathf.Lerp(forcaY, jumpForce, tempoPressionado / tempoMaxPulo);
            }

            // Se o botão for solto ou o tempo máximo de pulo for atingido
            if (Input.GetButtonUp("Jump") || tempoPressionado >= tempoMaxPulo)
            {
                // Interrompe o impulso de pulo e aplica a gravidade imediatamente
                forcaY = 0f; // Zera o impulso vertical
                ApplyFallAcceleration(); // Aplica a aceleração de queda
            }
        }

        // Aplica o movimento vertical
        Vector3 movimento = new Vector3(0, forcaY, 0);
        controller.Move(movimento * Time.deltaTime); // Para CharacterController
    }

    // Aplica aceleração de queda quando o personagem está no ar
    void ApplyFallAcceleration()
    {
        if (!isGrounded && (tempoPressionado >= 2f || !Input.GetButton("Jump")))
        {
            // Acelera a queda do personagem no ar
            if (forcaY < 0)
            {
                forcaY += Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            // Se o personagem estiver subindo, mas a tecla Jump não estiver sendo pressionada
            else if (forcaY > 0 && !Input.GetButton("Jump"))
            {
                forcaY += Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }
        else if (forcaY < 0)
        {
            forcaY = 0f; // Impede que a força Y continue caindo após o pulo
        }

        // Move o personagem com a força Y (pulo)
        controller.Move(new Vector3(0, forcaY, 0) * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        if (peDoPersonagem != null)
        {
            // Desenha uma esfera verde para visualizar o raio de verificação do chão
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(peDoPersonagem.position, groundCheckRadius);
        }
    }
}
