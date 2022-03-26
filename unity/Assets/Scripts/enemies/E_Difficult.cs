using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// difficult type enemy does not use NavMesh
// should track
// should fire

// Enemy type : Difficult
public class E_Difficult : MonoBehaviour, IEnemyBehavior
{
    private Player player; 
    private float distance; // Player ~ enemy 사이 거리
    private string difficultType; // enemy 타입
    private bool shouldFire = false; // intermediate, difficult enemy는 fire 가능
    
    private IEnemyBehavior.enemyState current; // enemy 상태
    private IEnemyBehavior.playerDistanceState isDetected; // enemy의 player 탐지  
    // public NavMeshAgent _agent; // enemy 인공지능 인스턴스
    // [SerializeField] Transform[] AgentRoutes;
    [SerializeField] Transform[] inAirRoutes;
    private int routeIndex = 0;
    [SerializeField] float patrolSpeed = 2f;
    
    [Range (0,15)] [SerializeField] float detectLimit = 5f; // enemy 감지 거리 한계, 에디터에서 설정 가능하도록 세팅
    [Range (0,15)] [SerializeField] float fireLimit = 2.5f; // enemy 사격 거리 한계, detectLimit보다 작게 설정할 것.

    [SerializeField] GameObject beeSting;
    [SerializeField] GameObject stingCreator;
    // ============== Object initialization and update ============== // 
    void Awake()
    {
        InitSetup();
    }

    void Update()
    {
        updateState();
        updateBehavior();
    }

        void InitSetup() 
    {
        // 플레이어 & NavMesh 초기화
        player = FindObjectOfType<Player>(); 
        // _agent = this.GetComponent<NavMeshAgent>(); 

        // Enemy 초기화 : awake시 상태는 idle, not detectable
        if (player != null ) // 오브젝트 null check && _agent != null
        {
            current = IEnemyBehavior.enemyState.Idle; 
            isDetected = IEnemyBehavior.playerDistanceState.TooFar;
            difficultType = IEnemyBehavior.enemyType.Difficult.ToString();
            this.GetComponent<Rigidbody>().useGravity = false; // flying enemy 중력사용 x
        } 
    }
    // ============== Object initialization and update ============== // 

    // ============== Enemy state and behavior ============== // 
    public void calculateDistance() { 
        // 플레이어 ~ 적 거리 계산
        distance = Vector3.Distance(player.transform.position, this.transform.position);

        // enemy 초기 상태 : 사격하지 않음, 추적 멈추지 않음.
        shouldFire = false;

        //  플레이어가 탐지 거리 바깥이면 패트롤 시작
        if (distance > detectLimit) isDetected = IEnemyBehavior.playerDistanceState.TooFar;

        // 플레이어가 탐지 거리 이하 좁혀지면 추적 시작
        if (distance < detectLimit) {
            isDetected = IEnemyBehavior.playerDistanceState.Within;
        }

        // 플레이어가 사격 거리 이하 좁혀지면 사격 시작
        if (distance < fireLimit) shouldFire = true;
    }
    public void updateBehavior()
    {
        switch(current) 
        {
            case IEnemyBehavior.enemyState.Idle : 
                Idle();
                break;
            case IEnemyBehavior.enemyState.Track : 
                Track();
                break;
            case IEnemyBehavior.enemyState.Fire :
                Fire();
                break;
            case IEnemyBehavior.enemyState.Die : 
                Die();
                break;
            default : 
                Idle();
                break;
        }
    }

    public void updateState()
    {
        calculateDistance(); // isDetected, shouldFire 변수 상태 change
        switch (isDetected) {
            case IEnemyBehavior.playerDistanceState.TooFar :
                current = IEnemyBehavior.enemyState.Idle;
                break;
            case IEnemyBehavior.playerDistanceState.Within : 
                if (shouldFire) {
                    current = IEnemyBehavior.enemyState.Fire;
                } else {
                    current = IEnemyBehavior.enemyState.Track;
                }
                break;
            default : 
                current = IEnemyBehavior.enemyState.Idle;
                break;
        }
    }
    // ============== Enemy state and behavior ============== // 


    // ============== IEnemyBehavior implementation ============== // 
    public void Idle() 
    {
        // TO DO : 플레이어가 탐지 거리 바깥이면 주변 패트롤
        print("Enemy being idle");
        Patrol();
    }

    public void Track() 
    {
        print("Enemy detected a player, starting tracking");
        StartCoroutine(EnemyTrackCoroutine(0.01f));
    }

    IEnumerator EnemyTrackCoroutine(float time)
    {
        // 1. set rotation to player
        ShouldLookAtPlayer();

        // 2. Track player
        yield return new WaitForSeconds(time);
        transform.position = Vector3.MoveTowards(
                transform.position, 
                player.transform.position, 
                Time.deltaTime * patrolSpeed
        );

        // 3. end coroutine
        yield return null;
    }

    public void Fire() 
    {
        print("Difficult Enemy firing projectiles faster");
        ShouldLookAtPlayer();
        StartCoroutine(EnemyFireCoroutine());
    }

    // FIX : infinite instantiation
    IEnumerator EnemyFireCoroutine()
    {
        bool shouldFireAgain = false;
        // 1. fire 사정 거리 체크
        // 2. 사정 거리 이내면 sting 발사
        yield return new WaitForSeconds(0.1f);
        // Invoke("spawnSting", 2f);
        if (shouldFire)
        {
            beeSting.transform.position = Vector3.MoveTowards(beeSting.transform.position, player.transform.position, Time.deltaTime*4f);
            shouldFireAgain = true;
        }

        if (shouldFireAgain)
        {
            GameObject clone = Instantiate(beeSting, stingCreator.transform.position, Quaternion.identity);
            shouldFireAgain = false;
        }

        yield return null;
    }

    public void Die() 
    {
        // TO DO : 플레이어가 밟고 지나가면 죽음
        print("Enemy died by a player");
        Destroy(this.gameObject);
    }

    public void Patrol()
    {
        // 첫 번째 inAirRoutes로 enemy 이동
        transform.position = Vector3.MoveTowards(
            transform.position,
            inAirRoutes[routeIndex].transform.position, 
            Time.deltaTime * patrolSpeed
        );

        // inAirRoutes에 맞춰 경로 상에서 반복 패트롤
        if (Vector3.Distance(transform.position, inAirRoutes[routeIndex].transform.position) < 1f) 
        {
            routeIndex = (routeIndex+1)%inAirRoutes.Length;
        }
    }
    // ============== IEnemyBehavior implementation ============== // 

    // ============== Enemy rotation implementation ============== // 
    void ShouldLookAtPlayer()
    {
        Vector3 playerPosWithLockedYAxis = new Vector3(
            player.transform.position.x, 
            transform.position.y, 
            player.transform.position.z
        ); 

        transform.LookAt(playerPosWithLockedYAxis); // fix y axis
        transform.Rotate( 0, 90, 0 ); // rotate enemy 90 degree in Y axis to correct direction
    }

    // ============== Enemy rotation implementation ============== // 
}
