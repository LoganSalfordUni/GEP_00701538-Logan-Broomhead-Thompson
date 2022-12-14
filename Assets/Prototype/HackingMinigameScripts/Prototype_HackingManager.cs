using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Prototype_HackingManager : MonoBehaviour
{
    public static Prototype_HackingManager instance;
    private void Awake()
    {
        instance = this;
    }


    Prototype_HackableObject _currentHackingObject;

    [Header("Keys")]
    [SerializeField] Prototype_KeyboardKey[] keyboardKeys;

    [Header("Pop Ups")]
    [Tooltip("What the pop ups spawn into")]
    [SerializeField] GameObject popUpParent;
    [SerializeField] Vector2 popUpBoundsX;
    [SerializeField] Vector2 popUpBoundsY;
    [Tooltip("Possible pop ups that might spawn")]
    [SerializeField] GameObject[] popUpPrefabs;

    [Header("UI")]
    [SerializeField] GameObject HackingParent;
    [Tooltip("slider that visually represents the progress of the hack")]
    [SerializeField] Image progressSlider;
    [Tooltip("Text that shows what percentage the player has completed of the current hack")]
    [SerializeField] Text progressText;
    [Tooltip("Text that shows how long remains for the player to hack the device")]
    [SerializeField] Text timeLimitText;

    /*[Header("Balancing Values")]
    [Tooltip("How much progress does each key give the player on click? Amount is randomly determined between the two numbers")]
    [SerializeField] Vector2Int easyDifficultProgressRate = new Vector2Int(6, 12);
    [Tooltip("How long does the player have to solve an easy hack. Note: On average 5 keys appear per second")]
    [SerializeField] float easyDifficultyTimeLimit = 15f;
    [Space(5)]
    [SerializeField] Vector2Int mediumDifficultProgressRate = new Vector2Int(5, 10);
    [SerializeField] float mediumDifficultyTimeLimit = 10f;
    [Space(5)]
    [SerializeField] Vector2Int hardDifficultProgressRate = new Vector2Int(4, 8);
    [SerializeField] float hardDifficultyTimeLimit = 10f;

    [Space(15)]
    [SerializeField] Vector2 timeBetweenPopUpsEasy = new Vector2(4f, 7f);
    [SerializeField] Vector2 timeBetweenPopUpsMedium = new Vector2(3f, 5f);
    [SerializeField] Vector2 timeBetweenPopUpsHard = new Vector2(1.5f, 3f);
    //[SerializeField] int maximumPopUps = 8;*/

    [Header("Balancing Values")]
    [Tooltip("How much progress is lost per second")]
    [SerializeField] int baseDecayRate = 1;
    /*[Tooltip("How much additional progress is lost per second per pop up on the screen")]
    [SerializeField] int popUpDecay = 4;*/
    
    Vector2 timeBetweenKeys = new Vector2(0.1f, 0.3f);
    Vector2 timeKeysStayActive = new Vector2(0.5f, 0.8f);

    private Vector2Int _progressRate;
    private float _timeLimit;
    private Vector2 _timeBetweenPopUps;
    private int _popUpDecayRate;


    /*public enum difficulty
    {
        easy,
        medium,
        hard,
    }
    private difficulty currentDifficulty = difficulty.easy;*/

    [SerializeField] Difficulty testDifficulty;
    private void Start()
    {
        //testing only
        BeginHack(testDifficulty);
    }


    private int progress;
    private float currentTimeBetweenKeySpawns;
    private float keySpawnTimer;
    private float popUpSpawnTimer;
    int numberOfPopUpsActive;
    private bool hacking = false;

    float progressDecayTimer;

    /*void BeginHack(difficulty hackDifficulty)
    {
        currentDifficulty = hackDifficulty;
        HackingParent.SetActive(true);


        progress = 0;
        progressDecayTimer = 1f;
        UpdateProgress(0);

        SetNextPopUpSpawnTimer();
        SetNextKeySpawnTimer();

        hacking = true;
    }*/

    public void BeginHack(Difficulty difficulty)
    {
        _progressRate = difficulty.myProgressRate;
        _timeLimit = difficulty.myTimeLimit;
        _timeBetweenPopUps = difficulty.myTimeBetweenPopUps;
        _popUpDecayRate = difficulty.myPopUpDecay;


        HackingParent.SetActive(true);


        progress = 0;
        progressDecayTimer = 1f;
        UpdateProgress(0);

        SetNextPopUpSpawnTimer();
        SetNextKeySpawnTimer();

        hacking = true;
    }

    void UpdateProgress(int progressIncrease)
    {
        //call this function every time the progres changes. It will change the progress for you, and then update the UI
        progress += progressIncrease;
        if (progress < 0)
            progress = 0;
        if (progress >= 100)
        {
            progress = 100;
            HackComplete();
        }

        progressSlider.fillAmount = progress / 100f;
        progressText.text = (progress.ToString() + "%");
    }

    void HackComplete()
    {
        return;
        //when the hack is complete, do stuff. 
    }

    private void Update()
    {
        if (hacking)
        {
            //enabling the keys on a timer
            keySpawnTimer += Time.deltaTime;
            if (keySpawnTimer > currentTimeBetweenKeySpawns)
            {
                SpawnKey();
                SetNextKeySpawnTimer();
            }

            //decrease the players progress every second based on the number of pop ups (but before a pop up is spawned this frame)
            progressDecayTimer -= Time.deltaTime;
            if (progressDecayTimer < 0f)
            {
                int amount = baseDecayRate + (_popUpDecayRate * numberOfPopUpsActive);
                progressDecayTimer += 1f;
                UpdateProgress(-amount);
            }

            //spawning pop ups
            popUpSpawnTimer -= Time.deltaTime;
            if (popUpSpawnTimer < 0f)
            {
                SpawnPopUp();
                SetNextPopUpSpawnTimer();
            }

        }
    }

    void SpawnKey()
    {
        int i = Random.Range(0, keyboardKeys.Length);
        float time = Random.Range(timeKeysStayActive.x, timeKeysStayActive.y);
        keyboardKeys[i].ActivateKey(time);
    }
    void SpawnPopUp()
    {
        float xPos = Random.Range(popUpBoundsX.x, popUpBoundsX.y);
        float yPos = Random.Range(popUpBoundsY.x, popUpBoundsY.y);

        Vector3 spawnPos = new Vector3(xPos, yPos, 0f);

        int popUpNumber = Random.Range(0, popUpPrefabs.Length);

        Instantiate(popUpPrefabs[popUpNumber], spawnPos, Quaternion.identity, popUpParent.transform);

        numberOfPopUpsActive += 1;
    }
    public void PopUpDestroyed()
    {
        numberOfPopUpsActive -= 1;
    }


    void SetNextKeySpawnTimer()
    {
        //decide how long till the next key spawns, and set the keyspawn timer to 0. 
        currentTimeBetweenKeySpawns = Random.Range(timeBetweenKeys.x, timeBetweenKeys.y);
        keySpawnTimer = 0f;
    }
    void SetNextPopUpSpawnTimer()
    {
        popUpSpawnTimer = Random.Range(_timeBetweenPopUps.x, _timeBetweenPopUps.y);

        /*if (currentDifficulty == difficulty.easy)
            popUpSpawnTimer = Random.Range(timeBetweenPopUpsEasy.x, timeBetweenPopUpsEasy.y);
        else if (currentDifficulty == difficulty.medium)
            popUpSpawnTimer = Random.Range(timeBetweenPopUpsMedium.x, timeBetweenPopUpsMedium.y);
        else if (currentDifficulty == difficulty.hard)
            popUpSpawnTimer = Random.Range(timeBetweenPopUpsHard.x, timeBetweenPopUpsHard.y);*/
    }

    public void KeyPressed()
    {
        //The Keyboard Key script calls this when its button has been pressed and its active. When this happens, the keyboard key also disables itself.

        UpdateProgress(Random.Range(_progressRate.x, _progressRate.y));

        /*if (currentDifficulty == difficulty.easy)
        {
            UpdateProgress(Random.Range(easyDifficultProgressRate.x, easyDifficultProgressRate.y + 1));
        }
        if (currentDifficulty == difficulty.medium)
        {
            UpdateProgress(Random.Range(mediumDifficultProgressRate.x, mediumDifficultProgressRate.y + 1));
        }
        if (currentDifficulty == difficulty.hard)
        {
            UpdateProgress(Random.Range(hardDifficultProgressRate.x, hardDifficultProgressRate.y + 1));
        }*/
    }
}
