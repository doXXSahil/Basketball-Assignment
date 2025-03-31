using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasketballController : MonoBehaviour {

    // Gameplay variables
    public float MoveSpeed = 10;
    public Transform Ball;
    public Transform PosDribble;
    public Transform PosOverHead;
    public Transform Arms;
    public Transform Target;

    [Header("Shot Settings")]
    public float MaxDriftForce = 0.5f;
    public float MinDriftForce = 0.1f;
    public float PerfectShotRadius = 1.0f;
    public float ScoringRadius = 2.0f;

    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    public Button exitButton;

    private bool IsBallInHands = true;
    private bool IsBallFlying = false;
    private float T = 0;
    private Vector3 driftOffset;
    private float distanceToTarget;
    private int score = 0;
    private int shotsTaken = 0;
    private bool gamePaused = false;

    void Start()
    {
        // Initialize UI
        gameOverPanel.SetActive(false);
        UpdateScoreUI();

        // Setup button listeners
        restartButton.onClick.AddListener(RestartGame);
        exitButton.onClick.AddListener(ExitGame);
    }

    void Update()
    {
        if (gamePaused) return;

        // Movement and shooting code...
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        transform.position += direction * MoveSpeed * Time.deltaTime;
        transform.LookAt(transform.position + direction);

        if (IsBallInHands)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                Ball.position = PosOverHead.position;
                Arms.localEulerAngles = Vector3.right * 180;
                transform.LookAt(Target.parent.position);
            }
            else
            {
                Ball.position = PosDribble.position + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * 5));
                Arms.localEulerAngles = Vector3.right * 0;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                PrepareShot();
            }
        }

        if (IsBallFlying)
        {
            UpdateBallFlight();
        }
    }

    void PrepareShot()
    {
        IsBallInHands = false;
        IsBallFlying = true;
        T = 0;
        distanceToTarget = Vector3.Distance(PosOverHead.position, Target.position);

        float normalizedDistance = Mathf.Clamp01(distanceToTarget / 15f);
        float currentDriftForce = Mathf.Lerp(MinDriftForce, MaxDriftForce, normalizedDistance);

        driftOffset = new Vector3(
            Random.Range(-currentDriftForce, currentDriftForce),
            0,
            Random.Range(-currentDriftForce, currentDriftForce)
        );

        shotsTaken++;
    }

    void UpdateBallFlight()
    {
        T += Time.deltaTime;
        float duration = 0.5f + (distanceToTarget * 0.05f);
        float t01 = T / duration;

        Vector3 A = PosOverHead.position;
        Vector3 B = Target.position + driftOffset;
        Vector3 pos = Vector3.Lerp(A, B, t01);

        float arcHeight = 2.0f + (distanceToTarget * 0.2f);
        float arc = arcHeight * Mathf.Sin(t01 * Mathf.PI);

        Ball.position = pos + Vector3.up * arc;

        if (t01 >= 1)
        {
            CompleteShot();
        }
    }

    void CompleteShot()
    {
        IsBallFlying = false;
        Ball.GetComponent<Rigidbody>().isKinematic = false;

        float distanceFromTarget = Vector3.Distance(Ball.position, Target.position);

        if (distanceFromTarget < PerfectShotRadius)
        {
            score += 2;
            Debug.Log("Perfect Shot!");
        }
        else if (distanceFromTarget < ScoringRadius)
        {
            score += 1;
            Debug.Log("Good Shot!");
        }
        else
        {
            GameOver();
            return;
            
        }

        UpdateScoreUI();
    }

    void GameOver()
    {
        gamePaused = true;
        Time.timeScale = 0; // Pause the game

        // Show game over panel
        gameOverPanel.SetActive(true);
        scoreText.gameObject.SetActive(false);
        finalScoreText.text = $"Final Score: {score}";

        // Enable cursor for button interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestartGame()
    {
        // Reset game state
        score = 0;
        shotsTaken = 0;
        gamePaused = false;
        Time.timeScale = 1;

        // Reset ball position
        Ball.position = PosDribble.position;
        Ball.GetComponent<Rigidbody>().isKinematic = true;
        IsBallInHands = true;
        IsBallFlying = false;

        // Hide UI
        gameOverPanel.SetActive(false);
        UpdateScoreUI();

        // Lock cursor back to game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}\nShots: {shotsTaken}";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsBallInHands && !IsBallFlying && !gamePaused)
        {
            IsBallInHands = true;
            Ball.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

}
