using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform cam;
    [SerializeField] GameObject freeCam;
    [SerializeField] AudioClip[] noms;
    [SerializeField] AudioClip win;

    public float size = 1;
    public float velocity = 2.5f;
    private float horizInput;
    private float vertInput;
    private float rotationSpeed = 360;
    private Rigidbody _rb;
    private Animator _anim;
    private Vector3 startPos;
    private float rayDistance = 0.1f;
    private RaycastHit hit;
    private bool isGrounded;

    private bool isWin;
    private CinemachineFreeLook _camera;

    private void Start()
    {
        startPos = transform.position;
        transform.localScale = new Vector3(size, size, size);
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _camera = freeCam.GetComponent<CinemachineFreeLook>();
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            _rb.angularVelocity = new Vector3(0, 0, 0);
            _anim.SetTrigger("Idle");
        }
        else _anim.SetTrigger("Go");
    }

    private void Update()
    {
        //Debug.Log(isGrounded);
        horizInput = Input.GetAxis("Horizontal");
        vertInput = Input.GetAxis("Vertical");       

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0;
        camRight.y = 0;
        Vector3 forwardRelative = vertInput * camForward;
        Vector3 rightRelative = horizInput * camRight;

        Vector3 moveDir = forwardRelative + rightRelative;
        _rb.velocity = new Vector3(moveDir.x * velocity, _rb.velocity.y, moveDir.z * velocity);

        Vector3 movementDirection = new Vector3(horizInput, 0, vertInput);
        movementDirection.Normalize();

        if (movementDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(new Vector3(moveDir.x, moveDir.y, _rb.velocity.z), Vector3.up);
            transform.localRotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        

        if (collision.gameObject.CompareTag("food"))
        {
            if (collision.gameObject.transform.localScale.x <= this.transform.localScale.x/2)
            {
                SoundManager.instance.PlayRandomSoundFXClip(noms, transform, Random.Range(0.6f, 0.8f));
                EatSomething(collision.gameObject.GetComponent<EdibleScript>().gain);
                if (collision.gameObject.GetComponent<EdibleScript>().isWinFood) WinFoodEated();
                Destroy(collision.gameObject);
            }
        }
        if (collision.gameObject.CompareTag("void"))
        {
            transform.position = startPos;
        }

        Vector3 velocity = _rb.velocity;
        // vertical limitation
        velocity.y = Mathf.Clamp(velocity.y, -1f, 1f);
        _rb.velocity = velocity;
    }
    public void EatSomething(float gain)
    {
        size += gain;
        velocity = size*2.5f;
        transform.localScale = new Vector3(size, size, size);
        _camera.m_Orbits[0].m_Radius += gain * 1.5f;
        _camera.m_Orbits[1].m_Radius += gain * 1.5f;
        _camera.m_Orbits[2].m_Radius += gain * 1.5f;
        _camera.m_Orbits[0].m_Height += gain;
        _camera.m_Orbits[1].m_Height += gain;
        _camera.m_Orbits[2].m_Height += gain;
        _rb.mass = size * 1.5f;

    }

    public void WinFoodEated()
    {
        if (!isWin)
        {
            isWin = true;
            Debug.Log("Level Completed");
            StartCoroutine(WinTimer());
        }
        else return;
        
    }

    private IEnumerator WinTimer()
    {
        yield return new WaitForSeconds(1.5f);
        SoundManager.instance.PlaySoundFXClip(win, transform, 0.8f);
        StopCoroutine(WinTimer());
    }
}
