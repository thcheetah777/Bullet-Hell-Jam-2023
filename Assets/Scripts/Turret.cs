using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{

    [SerializeField] private float delay = 1;
    [SerializeField] private float force = 3;
    [SerializeField] private float angleMin = -90;
    [SerializeField] private float angleMax = 90;
    [SerializeField] private float gunRotateSpeed = 0.5f;
    [SerializeField] private Rigidbody2D bulletPrefab;
    [SerializeField] private Transform gunAnchor;

    float angle = 0;

    void Start() {
        StartCoroutine(ShootRoutine());
    }

    void Update() {
        gunAnchor.rotation = Quaternion.RotateTowards(gunAnchor.rotation, Quaternion.Euler(0, 0, angle), gunRotateSpeed * Time.deltaTime);
    }

    private IEnumerator ShootRoutine() {
        while (true)
        {
            angle = Random.Range(angleMin, angleMax);
            yield return new WaitForSeconds(delay);
            Shoot();
        }
    }

    private void Shoot() {
        Quaternion randomAngle = Quaternion.Euler(0, 0, angle);
        Rigidbody2D bulletBody = Instantiate(bulletPrefab, transform.position, randomAngle);
        bulletBody.AddRelativeForce(Vector2.up * force, ForceMode2D.Impulse);
    }

}
