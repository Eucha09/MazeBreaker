using UnityEngine;

public class TestAgent : MonoBehaviour
{
    NavHybridAgent _agent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = GetComponent<NavHybridAgent>();
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Vector3 randomPos = new Vector3(Random.Range(-100f, 100f), 0f, Random.Range(-100f, 100f));
            _agent.SetDestination(randomPos);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            PlayerController player = Managers.Object.GetPlayer();
            _agent.SetDestination(player.transform.position);
        }
    }
}
