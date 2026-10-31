using UnityEngine;
using System.Collections;

public class GroundImpactEffect : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private float yOffset = -0.9f; // תתאימי את הגובה דרך ה-Inspector

    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownTime = 0.5f; // זמן ההפסקה בין מכה למכה

    [Header("Camera Shake")]
    [SerializeField] private float shakeMagnitude = 0.2f;
    [SerializeField] private float shakeDuration = 0.15f;

    private bool canTriggerImpact = true;

    // הפונקציה שנקראת מה-Animation Event
    public void TriggerGroundImpact()
    {
        if (!canTriggerImpact) return;
        StartCoroutine(CooldownRoutine());

        // מיקום הפגיעה לפי ה-yOffset שנקבע
        Vector3 spawnPos = transform.position + new Vector3(0f, yOffset, 0f);

        // 1. יצירת אפקט חלקיקי האדמה
        CreateDustParticles(spawnPos);

        // 2. יצירת גל הדף (Shockwave)
        CreateShockwaveRing(spawnPos);

        // 3. רעד מצלמה
        if (Camera.main != null)
        {
            StartCoroutine(ShakeCamera());
        }
    }

    private void CreateDustParticles(Vector3 pos)
    {
        GameObject dustObj = new GameObject("VFX_GroundDust");
        dustObj.transform.position = pos;
        dustObj.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem ps = dustObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
        main.startColor = new Color(0.35f, 0.22f, 0.1f);
        main.gravityModifier = 1.8f;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 45) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 65f;
        shape.radius = 0.1f;

        ParticleSystemRenderer psRenderer = dustObj.GetComponent<ParticleSystemRenderer>();
        psRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

        ps.Play();
    }

    private void CreateShockwaveRing(Vector3 pos)
    {
        GameObject ringObj = new GameObject("VFX_Shockwave");
        ringObj.transform.position = pos + new Vector3(0f, 0.05f, 0f);
        ringObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        ParticleSystem ps = ringObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = 0.3f;
        main.startSpeed = 8f;
        main.startSize = 0.15f;
        main.startColor = new Color(0.7f, 0.6f, 0.4f, 0.5f);
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Donut;
        shape.radius = 0.2f;

        ParticleSystemRenderer psRenderer = ringObj.GetComponent<ParticleSystemRenderer>();
        psRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

        ps.Play();
    }

    private IEnumerator CooldownRoutine()
    {
        canTriggerImpact = false;
        yield return new WaitForSeconds(cooldownTime);
        canTriggerImpact = true;
    }

    private IEnumerator ShakeCamera()
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            Camera.main.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }
}