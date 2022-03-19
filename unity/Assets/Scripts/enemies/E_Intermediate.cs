using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Enemy type : Easy
public class E_Intermediate : MonoBehaviour, IEnemyBehavior
{
    private Player player; // Player 컴포넌트 체크
    private float distance; // Player ~ enemy 사이 거리
    
    private string intermediateType;
    private IEnemyBehavior.enemyState current; // enemy 상태
    private IEnemyBehavior.playerDistanceState isDetected; // enemy의 player 탐지  
    private bool shouldFire = false; // intermediate enemy는 fire 가능
    private NavMeshAgent _agent; // enemy 인공지능 인스턴스
    [SerializeField] Transform[] AgentRoutes; // enemy 인공지능 인스턴스 path
    private int routeIndex = 0;
    public GameObject projectile; // Fire 메소드 발사체
    public GameObject projectileCreator; // Fire 메소드 발사체 creator
    public GameObject clone;
    [Range (0,15)] public float detectLimit = 5f; // enemy 감지 거리 한계, 에디터에서 설정 가능하도록 세팅
    [Range (0,15)] public float fireLimit = 2.5f; // enemy 사격 거리 한계, detectLimit보다 작게 설정할 것.

    
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
        _agent = this.GetComponent<NavMeshAgent>(); 

        // Enemy 초기화 : awake시 상태는 idle, not detectable
        if (player != null && _agent != null) // 오브젝트 null check
        {
            current = IEnemyBehavior.enemyState.Idle; 
            isDetected = IEnemyBehavior.playerDistanceState.TooFar;
            intermediateType = IEnemyBehavior.enemyType.Intermediate.ToString();
            _agent.autoBraking = false; // NavMeshAgent 연속적인 움직임 설정
        } 
    }
    // ============== Object initialization and update ============== // 

    // ============== Enemy state and behavior ============== // 
    public void updateState()
    {
        calculateDistance(); // isDetected 변수 상태 change
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
    public void calculateDistance() { 
        // 플레이어 ~ 적 거리 계산
        distance = Vector3.Distance(player.transform.position, this.transform.position);

        // enemy 초기 상태 : 사격하지 않음, 추적 멈추지 않음.
        shouldFire = false;
        _agent.isStopped = false;

        // 플레이어가 탐지 거리 이하 좁혀지면 추적 시작
        if (distance < detectLimit) {
            isDetected = IEnemyBehavior.playerDistanceState.Within;
        }

        // 플레이어가 사격 거리 이하 좁혀지면 사격 시작
        if (distance < fireLimit) shouldFire = true;
    }
    // ============== Enemy state and behavior ============== // 


    // ============== IEnemyBehavior implementation ============== // 
    public void Idle() 
    {
        // 플레이어가 탐지 거리 바깥이면 주변 패트롤
        print("Enemy being idle");
        Patrol();
    }

    public void Track() 
    {
        print("Enemy detected a player, starting tracking");
        _agent.SetDestination(player.transform.position);
    }

    public void Fire() 
    {
        print($"{intermediateType} enemy fires an object");
        _agent.isStopped = true; // 제자리에서 오브젝트 슈팅 시작
        _agent.transform.LookAt(player.transform); // should look at player after stopped

        // 슈팅 논리 전개
        RaycastHit hit;
        bool isHit = Physics.Raycast(
            _agent.transform.position, 
            _agent.transform.forward, 
            out hit, 
            fireLimit);

        if (isHit) {
            // StartCoroutine(name, param)
            StartCoroutine(enemyShootCoroutine(0.1f));
        }
    }

    // Player die coroutine
    IEnumerator enemyShootCoroutine(float time) { 
        print("ray casted and enemyShootCoroutine started");

        // 데미지 logic
        // 1. 발사체 오브젝트 생성
        if (Input.GetKeyDown(KeyCode.Space)) {
            GameObject _clone = Instantiate(projectile, projectileCreator.transform.position, Quaternion.identity);
            clone = _clone;
        }
        
        yield return new WaitForSeconds(time); 
        // 2. 발사체 플레이어에게 투척
        clone.transform.position = Vector3.MoveTowards(clone.transform.position, player.transform.position, Time.deltaTime *4f);

        yield return new WaitForSeconds(time);
        print("player got hit!!");
    }

    public void Die() 
    {
        // TO DO : 플레이어가 밟고 지나가면 죽음
        print("Enemy died by a player");
        Destroy(this.gameObject);
    }

    public void Patrol()
    {
        // AgentRoutes: empty 게임 오브젝트로 맵 내 이동 포인트 지정
        _agent.destination = AgentRoutes[routeIndex].transform.position;
        _agent.isStopped = false;

        // 이동 포인트 남은 거리 0.5f 미만일시 다음 이동 포인트로 이동
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            routeIndex = (routeIndex+1)%AgentRoutes.Length;
        }
    }
    // ============== IEnemyBehavior implementation ============== // 
}
