using UnityEngine;

public class S_MaterialChanger : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Material targetMaterial;
    [SerializeField] private string propertyName = "_Parameter";
    [SerializeField] private float valueA = 0f;
    [SerializeField] private float valueB = 1f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool toggled = false;
    private Coroutine currentCoroutine;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) 

        {
            toggled = !toggled;

            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(Transition(toggled ? valueB : valueA));
        }
    }

    private System.Collections.IEnumerator Transition(float targetValue)
    {
        float startValue = targetMaterial.GetFloat(propertyName);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = curve.Evaluate(elapsed / duration);
            targetMaterial.SetFloat(propertyName, Mathf.Lerp(startValue, targetValue, t));
            yield return null;
        }

        targetMaterial.SetFloat(propertyName, targetValue);
    }
}