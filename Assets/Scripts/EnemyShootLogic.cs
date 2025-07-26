using UnityEngine;
using System.Collections;

public class EnemyShootLogic : MonoBehaviour
{
    public enum ShotType { Single, Shotgun, Double }

    public GameObject BulletPrefab;
    public ShotType Type = ShotType.Single;
    public int Power = 1;
    public float ShootCooldown = 1f;
    public float BulletSpeed = 8f;

    public int ShotgunBullets = 3;
    public float ShotgunAngle = 0.5f;

    private float Timer = 0f;
    private EnemyController controller;

    public EnemyController enemy;


    public void TryShoot()
    {

        Debug.Log("TryShoot called from state: " + enemy.currentState);



        Timer += Time.deltaTime;
        Debug.Log($"TryShoot running. Timer: {Timer:F2}, Cooldown: {ShootCooldown}");

        if (Timer >= ShootCooldown)
        {
            Debug.Log("TryShoot cooldown met, calling Fire()");
            Fire();
            Timer = 0f;
        }
    }

    void Start()
    {
        controller = GetComponent<EnemyController>();
    }

    void Update()
    {
        if (controller.currentState != EnemyState.RangeAttack)
            return;

        Timer += Time.deltaTime;

        if (Timer >= ShootCooldown)
        {
            Fire();
            Timer = 0f;
        }
    }

    void Fire()
    {
        Vector2 origin = enemy.transform.position;
        Vector2 targetPos = enemy.target.position;
        Vector2 direction = (targetPos - origin).normalized;

        switch (Type)
        {
            case ShotType.Single:
                SpawnBullet(direction);
                break;
        }
    }

    void SpawnBullet(Vector2 direction)
    {
        //Debug.Log("Spawning bullet at " + transform.position);

        var bullet = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        bullet.transform.up = direction;
        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction * BulletSpeed;

        var logic = bullet.GetComponent<BulletLogic>();
        if (logic != null)
        {
            logic.Power = Power;
            logic.Team = Teams.Enemy;
        }
    }

    Vector2 RotateVector(Vector2 vec, float angle)
    {
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        return new Vector2(cos * vec.x - sin * vec.y, sin * vec.x + cos * vec.y);
    }
}
