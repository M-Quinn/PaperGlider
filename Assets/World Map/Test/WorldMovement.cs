using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldMovement : MonoBehaviour
{
    private Vector2 userMovement;
    public GameObject World;
    public float speed = 3;
    Camera mainCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Input.mousePosition);
        
        RotatePlayerTowardsMouse();
        
        Vector2 moveOffset = -transform.right * speed * Time.deltaTime;
        
        World. transform.Translate(moveOffset, Space.World);
    }
    
    private void RotatePlayerTowardsMouse()
    {
        if (mainCamera == null) return;

        Vector3 mousePos = Input.mousePosition;
        // Convert mouse position to world coordinates, considering the camera's Z position
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.transform.position.z));

        // Get the direction from the player to the mouse position
        Vector2 direction = mouseWorldPosition - transform.position;
        direction.Normalize(); // Ensure the direction vector has a magnitude of 1

        // Calculate the rotation angle in degrees (0 degrees is right, increasing counter-clockwise)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply the rotation to the player.
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
