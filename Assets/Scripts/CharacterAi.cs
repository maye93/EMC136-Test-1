using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterAi : MonoBehaviour
{
    public float speed = 5f;  // Constant speed
    public float rotationSpeed = 5f;

    private List<Transform> waypoints = new List<Transform>();
    private int waypointCounter = 0;
    private Rigidbody rb;
    public LineRenderer lineRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Check if LineRenderer is already attached, if not, add it.
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        GameObject[] coinObjects = GameObject.FindGameObjectsWithTag("Coin");
        foreach (var coinObject in coinObjects)
        {
            waypoints.Add(coinObject.transform);
        }

        if (waypoints.Count > 0)
        {
            UpdateLineRenderer();
        }
        else
        {
            Debug.LogError("No waypoints found. Please ensure waypoints are properly tagged.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            RemoveWaypoint(other.gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (waypoints.Count == 0)
        {
            LoadNextScene();
            return;  // Stop FixedUpdate if there are no waypoints left
        }

        RotateTowardsWaypoint();
        MoveForward();
    }

    private void RotateTowardsWaypoint()
    {
        if (waypoints.Count == 0) // Check if there are waypoints left
        {
            return;
        }

        if (waypointCounter >= waypoints.Count)
        {
            return;
        }

        Vector3 targetDirection = waypoints[waypointCounter].position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

        // Check if the rotation is almost complete
        if (Quaternion.Angle(rb.rotation, targetRotation) < 1.0f)
        {
            waypointCounter = Mathf.Clamp(waypointCounter + 1, 0, waypoints.Count - 1);
            UpdateLineRenderer();
        }
    }

    private void MoveForward()
    {
        rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);
    }

    private void RemoveWaypoint(GameObject coin)
    {
        waypoints.Remove(coin.transform);
        Destroy(coin);
        waypointCounter = Mathf.Clamp(waypointCounter, 0, waypoints.Count - 1);
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = waypoints.Count + 1;
        lineRenderer.SetPosition(0, transform.position);

        for (int i = 0; i < waypoints.Count; i++)
        {
            lineRenderer.SetPosition(i + 1, waypoints[i].position);
        }
    }

    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next scene available. Check your build settings.");
        }
    }
}