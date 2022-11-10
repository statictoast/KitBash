
using UnityEngine;
using UnityEngine.UI;

public class TestGrounding : MonoBehaviour
{
    public PlayerEntity player;
    private Text m_GuiText;

    // Start is called before the first frame update
    void Start()
    {
        m_GuiText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2 vel = player.GetCurrentVelocity();
        //m_GuiText.text = player.testVal ? "test val" : "NOT test val";
    }
}
