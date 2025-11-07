
using UnityEngine;

public class Barrel : MonoBehaviour
{
    [SerializeField] GameObject player;
    float barrelWait;
    GameEvent unHide = new GameEvent();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventManager.AddInvoker(GameplayEvent.UnHide, unHide);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {

            player.SetActive(true);
            unHide.Invoke(unHide.Data);
            Destroy(gameObject);

        }
    }

    public void barrelInitialize(GameObject thisPlayer)
    {
        player = thisPlayer;
    }
}
