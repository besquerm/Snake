using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Snake : MonoBehaviour
{
    float step;
    bool growingPending;
    bool isPaused;
    Controls controls;
    public GameObject tailPrefab;
    public GameObject foodPrefab;
	public GameObject trapPrefab;
    public GameObject leftSide;
    public GameObject rightSide;
    public GameObject topSide;
    public GameObject bottomSide;
    public BoxCollider2D foodSpawn;

    [SerializeField] private AudioClip eatFood;
    [SerializeField] private AudioClip starTrap;
    [SerializeField] private AudioClip loose;
    [SerializeField] public AudioSource BGSound;

    public float score;
	public float trapScore;
    public TextMeshProUGUI scoreText;

    public List<Transform> tail = new List<Transform>();
    Vector3 lastPos;
    enum Controls{
        up,
        down,
        left,
        right
    }

    private void Start(){

        StartCoroutine(MoveCoroutine());
        step = GetComponent<SpriteRenderer>().bounds.size.x;
        CreateFood();
        BGSound = GetComponent<AudioSource>();
        BGSound.Play();
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.RightArrow) && controls != Controls.left){
            controls = Controls.right;
        }else if(Input.GetKeyDown(KeyCode.LeftArrow) && controls != Controls.right){
             controls = Controls.left;
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow) && controls != Controls.down){
             controls = Controls.up;
        }else if(Input.GetKeyDown(KeyCode.DownArrow) && controls != Controls.up){
             controls = Controls.down;
        }else if(Input.GetKeyDown(KeyCode.Escape)){
             UpdateGameState();
        }else if(Input.GetKeyDown(KeyCode.Space)){
             GameReset();
        }

        scoreText.text = "" + score;
    }

    private void Move(){
        lastPos = transform.position;
        Vector3 nextPos = Vector3.zero;

        if(controls == Controls.left){
            nextPos = Vector3.left;
        }else if(controls == Controls.right){
            nextPos = Vector3.right;
        }else if(controls == Controls.up){
            nextPos = Vector3.up;
        }else if(controls == Controls.down){
            nextPos = Vector3.down;
        }

        transform.position += nextPos * step;
		transform.eulerAngles = new Vector3(0,0,GetAngleFromVector(nextPos));
        MoveTail();
    }

    void MoveTail(){
        for (int i = 0; i < tail.Count; i++){
            Vector3 temp = tail[i].position;
            tail[i].position = lastPos;
            lastPos = temp;
        }
        if(growingPending){
            CreateTail();
        }
    }

    IEnumerator MoveCoroutine(){
        while (true){
            yield return new WaitForSeconds(0.08f);
            Move();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision){ 
        if (collision.gameObject.tag == "Food"){
            SoundController.Instance.StartSound(eatFood);
            growingPending = true;
            Destroy(collision.gameObject);
            score += 1;
			trapScore += 1;
			if(trapScore == 5){
				CreateTrap();
				trapScore = 0;
			}
            CreateFood();
            
        }
        else if(collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Tail" || collision.gameObject.tag == "Trap"){
            Time.timeScale = 0f;
            SoundController.Instance.StartSound(loose);
            GameLost();
            
        }
    }

    private void GameLost(){
        //Time.timeScale = 0f;
        BGSound.Pause();
        UpdateGameState();
    }

    private void GameReset(){
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CreateTail(){
        GameObject newTail = Instantiate(tailPrefab, lastPos, Quaternion.identity);
        newTail.name = "Tail_" + tail.Count;
        tail.Add(newTail.transform);
        growingPending = false;
    }

    void CreateFood(){
        Bounds bounds = this.foodSpawn.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        
        Vector2 pos = new Vector2(Mathf.Round(x), Mathf.Round(y)); 
		
        Instantiate(foodPrefab, pos, Quaternion.identity);
    }
	
	void CreateTrap(){
        SoundController.Instance.StartSound(starTrap);
        Bounds bounds = this.foodSpawn.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        
        Vector2 pos = new Vector2(Mathf.Round(x), Mathf.Round(y)); 
		
        Instantiate(trapPrefab, pos, Quaternion.identity);
    }
	
	private float GetAngleFromVector(Vector3 dir){
		float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		if(n<0) n += 360;
		return n;
	}

    private void UpdateGameState(){
        isPaused = !isPaused;

        if(isPaused){
            Time.timeScale = 0f;
        }
        else{
            Time.timeScale = 1f;
        }
    }

}
