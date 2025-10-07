using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Animator))]
public class JournalButtonController : MonoBehaviour
{
    private Animator anim;
    private bool isOpen;

    void Awake()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("Open", false);   // start closed
        // DO NOT add the listener here when wiring via OnClick()
        // GetComponent<Button>().onClick.AddListener(Toggle);
    }

    // Must be public for Inspector wiring
    public void Toggle()
    {
        isOpen = !isOpen;
        anim.SetBool("Open", isOpen);
        Debug.Log("Journal Toggle clicked. isOpen=" + isOpen);
    }
}
