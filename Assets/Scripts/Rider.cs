using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rider : MonoBehaviour
{
    private enum GameState {GamePlaying, GameWin, GameOver}
    private GameState gameState;
    private Vector3 speed;
    private const float speedChange = 0.2f;
    private int coins;
    public GameObject stickmanPrefab;
    private GameObject character;
    private List<GameObject> balls;
    private GameObject droppedBalls;
    private Animator animator;
    private Touch touch; //For Android Development
    private const float maxX = 1.5f;

    private void Awake() {
        this.gameState = GameState.GamePlaying;
        this.coins = 0;
        this.speed = new Vector3(0, 0, 1f);
        this.character = Instantiate(stickmanPrefab, Vector3.zero, Quaternion.identity);
        this.character.transform.SetParent(this.transform);
        this.balls = new List<GameObject>();
        this.droppedBalls = new GameObject("Dropped Balls");
        this.animator = this.character.GetComponent<Animator>();
        animator.SetBool("IsPlaying", true);
    }

    private void Start() {
        //animator.SetBool("IsPlaying", true);
    }

    private void OnTriggerEnter(Collider collider) {
        if(collider.gameObject.tag == "Coin") {
            IncreaseCoin(1);
            Destroy(collider.gameObject);
        }
        else if(collider.gameObject.tag == "Ball") {
            IncreaseCoin(1);
            BallCollide(collider.gameObject);
        }
        else if(collider.gameObject.tag == "Wall") {
            IncreaseCoin(1);
            StartCoroutine(WallCollide(collider.gameObject));
        }
        else if(collider.gameObject.tag == "Road") {
            RidingRoad(collider.gameObject);
        }
        else if(collider.gameObject.tag == "Finish") {
            StartCoroutine(CompleteGame(collider.gameObject.transform.position));
            this.gameState = GameState.GameWin;
        }
    }

    private void OnTriggerExit(Collider collider) {
        if(collider.gameObject.tag == "Road" && collider.gameObject.transform.rotation.x < 0) {
            this.speed = new Vector3(0, -1f, 1f) * speedChange;
        }
    }
    
    private void BallCollide(GameObject ballObj) {
        ballObj.transform.SetParent(this.transform);
        ballObj.GetComponent<SphereCollider>().enabled = false;
        this.balls.Add(ballObj);
        Restack();
    }

    private IEnumerator WallCollide(GameObject wallsObj) {
        wallsObj.GetComponent<BoxCollider>().enabled = false;
        Walls walls = wallsObj.GetComponent<Walls>();
        if(balls.Count - 1 < walls.wallHeight) {
            this.gameState = GameState.GameOver;
            GameOverWindow.StaticShowLoose();
        }
        else {
            for(int i = 0; i < walls.wallHeight; i++) {
                this.balls[balls.Count - 1].transform.SetParent(droppedBalls.transform);
                this.balls.RemoveAt(balls.Count - 1);
            }
            const float fallingTime = 0.5f;
            yield return new WaitForSeconds(fallingTime);
            Restack();
        }
    }

    private void RidingRoad(GameObject roadObj) {
        float roadAngle = roadObj.transform.localRotation.x;
        if(roadAngle == 0) {
            animator.SetBool("IsFastRunning", false);
            this.transform.position = new Vector3(this.transform.position.x, 0, this.transform.position.z);
            this.speed = new Vector3(0, 0, 1f) * speedChange;
        }
        else if(roadAngle < 0) {
            animator.SetBool("IsFastRunning", true);
            const float upSpeed = 0.5f;
            this.speed = new Vector3(0, 1f, 1f) * speedChange * upSpeed;
        }
        else if(roadAngle > 0) {
            animator.SetBool("IsFastRunning", false);
            const float downSpeed = 2f;
            this.speed = new Vector3(0, -1f, 1f) * speedChange * downSpeed;
        }
    }

    private void IncreaseCoin(int coin) {
        this.coins += coin;
        ScoreWindow.UpdateCoin(this.coins);
    }

    private IEnumerator CompleteGame(Vector3 finishPos) {
        // Move player to the finish position
        Vector3 currPos = this.transform.position;
        const int framesNum = 20;
        float percent = 0;
        for(int i = 0; i < framesNum; i++) {
            percent += (float) 1 / framesNum;
            this.transform.position = Vector3.Lerp(currPos, finishPos, percent);
            yield return new WaitForSeconds(2.0f / framesNum);
        }
        animator.SetBool("IsWinning", true);

        // Explode the ball
        const float explodingTime = 0.5f;
        GameObject explodedBalls = new GameObject("Exploded Balls");
        explodedBalls.transform.position = finishPos;
        int coin = 0;
        while(this.balls.Count > 0) {
            GameObject ball = balls[balls.Count - 1];
            balls.RemoveAt(balls.Count - 1);

            // Set up and render Particle system
            GameObject explosion = new GameObject("Explosion");
            explosion.transform.SetParent(explodedBalls.transform);
            ParticleSystem particleSystem = explosion.AddComponent<ParticleSystem>();
            var partSysmain = particleSystem.main;
            partSysmain.playOnAwake = false;
            particleSystem.Stop();
            partSysmain.duration = 2f;
            partSysmain.loop = false;
            ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.SetMeshes(new Mesh[] {
                Resources.GetBuiltinResource<Mesh>("Cube.fbx")
            });
            renderer.material = ball.GetComponentInChildren<MeshRenderer>().material;
            explosion.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            explosion.transform.Translate(finishPos);
            particleSystem.Play();
            
            coin += 5;
            IncreaseCoin(coin);
            Destroy(ball);
            Restack();
            yield return new WaitForSeconds(explodingTime);
        }

        // Set up winning screen
        GameOverWindow.StaticShowWin();
    }

    private void Restack() {
        Vector3 playerPos = this.transform.position;
        for(int i = 0; i < balls.Count; i++) {
            balls[i].transform.position = new Vector3(playerPos.x, balls.Count - 1 - i, playerPos.z);
        }
        this.character.transform.SetParent(this.transform);
        this.character.transform.position = new Vector3(playerPos.x, balls.Count, playerPos.z);
    }

    private void RotateBalls() {
        for(int i = 0; i < this.balls.Count; i++) {
            GameObject sphere = this.balls[i].transform.Find("Sphere").gameObject;
            sphere.transform.Rotate(1f, 0, 0);
        }
    }
    
    private void Update() {
        switch(gameState) {
            case GameState.GamePlaying:
                RotateBalls();
                this.transform.Translate(speed);
                if(Input.touchCount > 0) {
                    this.touch = Input.GetTouch(0);        
                }
                if(Input.GetKeyDown(KeyCode.A) || this.touch.deltaPosition.x < 0) {
                    this.transform.Translate(new Vector3(-1, 0, 0) * 0.5f);
                    if(this.transform.position.x < -maxX) {
                        this.transform.position = new Vector3(-maxX, this.transform.position.y, this.transform.position.z);
                    }
                }
                if(Input.GetKeyDown(KeyCode.D) || this.touch.deltaPosition.x > 0) {
                    this.transform.Translate(new Vector3(1, 0, 0) * 0.5f);
                    if(this.transform.position.x > maxX) {
                        this.transform.position = new Vector3(maxX, this.transform.position.y, this.transform.position.z);
                    }
                }
                break;
            case GameState.GameWin:
                return;
            case GameState.GameOver:
                return;
            default:
                return;
        }
    }
}
