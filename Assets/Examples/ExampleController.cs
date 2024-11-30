using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleController : MonoBehaviour
{
    public Transform camera;
    public CCEnhanced player;
    public float speed;
    public float drag;
    public float mouseSensitivity;
    public float gravity;
    public float jumpForce;
    public float adhesion;
    Vector3 velocity;
    Vector3 wishDir;
    Vector3 cameraRotation;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<CCEnhanced>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        wishDir = Vector3.Scale((camera.transform.right * Input.GetAxisRaw("Horizontal") + camera.transform.forward * Input.GetAxisRaw("Vertical")), new Vector3(1, 0, 1)).normalized;
        player.Move(velocity * Time.deltaTime);
        cameraRotation += new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * mouseSensitivity;
        camera.transform.eulerAngles = cameraRotation;
        if(player.isGrounded && Input.GetKeyDown(KeyCode.Space)){
            //player.BreakStep();
            velocity.y += jumpForce;
        }
    }
    void FixedUpdate(){
        velocity += wishDir * speed * Time.deltaTime;
        velocity *= 1/(drag+1);
        if(!player.isGrounded)velocity.y -= gravity;
    }
    void OnControllerColliderHit(ControllerColliderHit hit){
        float momentum = Vector3.Dot(velocity, hit.normal);
        velocity -= hit.normal * momentum;
        velocity -= hit.normal * adhesion;
    }
}