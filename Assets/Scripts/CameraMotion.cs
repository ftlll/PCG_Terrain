using UnityEngine;

public class CameraMotion : MonoBehaviour
{
    private CharacterController controller;
    private bool is_first_person = false;

    public float moveSpeed = 20.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        is_first_person = false;
        SetCameraView();
    }

    void Update()
    {
        // Toggle first person and third peron view with 'C'
        if (Input.GetKeyDown(KeyCode.C))
        {
            is_first_person = !is_first_person;
            SetCameraView();
        }

        if (is_first_person)
        {
            FirstPersonUpdate();
        }
        else
        {
            ThirdPersonUpdate();
        }
    }

    void SetCameraView()
    {
        if (is_first_person)
        {
            Camera.main.transform.position = new Vector3(5f, 1.8f, 5f);
            Camera.main.transform.localRotation = Quaternion.identity;
        }
        else
        {
			controller.enabled = false;
			transform.position = new Vector3(5f, 10f, 5f);
			controller.enabled = true;
            Camera.main.transform.position = new Vector3(5f, 10f, 5f);
            Camera.main.transform.localRotation = Quaternion.identity;
        }
    }

    void FirstPersonUpdate()
    {
        float dx = Input.GetAxis("Horizontal");
        float dz = Input.GetAxis("Vertical");

		Camera.main.transform.Rotate (0, dx * 0.1f, 0);
        Vector3 move = transform.forward * dz;
        controller.Move(move * moveSpeed * Time.deltaTime);

        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // added: jump logic, use space to jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void ThirdPersonUpdate()
    {
        float dx = Input.GetAxis("Horizontal");
        float dz = Input.GetAxis("Vertical");

		Camera.main.transform.Rotate(0, dx * 0.1f, 0);
		Vector3 move = transform.forward * dz;
		velocity.y = 0f;
		move.y = 0f;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}