using UnityEngine;
using UnityEngine.UI; // ← Text는 여기 필요
using TMPro;

/* V1
public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float fadeSpeed = 2f;
    public float lifetime = 1f;

    private Text text;
    private Color color;
    private float timer;

    void Awake()
    {
        text = GetComponentInChildren<Text>(); // ← Legacy Text 가져오기
        color = text.color;
    }

    public void Setup(int damage)
    {
        text.text = damage.ToString();
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        timer += Time.deltaTime;

        if (timer > lifetime)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            text.color = color;

            if (color.a <= 0f)
                Destroy(gameObject);
        }
    }
    void LateUpdate()
    {
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
}*/
public class UI_DamagePopup : UI_Base
{
    public float moveSpeed = 1f;
    public float fadeSpeed = 2f;
    public float lifetime = 1f;
    private float timer;
    private Vector3 moveDirection;

    //private Text text;
    private TextMeshProUGUI text;
    private Color color;
    private Vector3 originalScale;

	void Awake()
    {
        //text = GetComponentInChildren<Text>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        color = text.color;
        originalScale = transform.localScale;

        // 이동 방향 + 속도 랜덤화
        moveDirection = (Vector3.up + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f))).normalized;
        moveSpeed *= Random.Range(0.8f, 1.2f);
	}

	public override void Init()
	{

	}

	// 데미지와 색상 설정
	public void Setup(int damage, Color damageColor, bool isNegative = false)
    {
        if (damage <= 0) 
        {
            Destroy(gameObject);  // 0일 경우 삭제
            return;
        }

        text.text = (isNegative ? "-" : "") + damage.ToString();
        color = damageColor;
        text.color = color;

        // 팡! 효과 시작
        transform.localScale = originalScale * 1.5f;
    }

    void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        timer += Time.deltaTime;

        // 스케일 서서히 축소
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 5f);

        // 페이드 아웃
        if (timer > lifetime)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            text.color = color;

            if (color.a <= 0f)
                Managers.Resource.Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
}