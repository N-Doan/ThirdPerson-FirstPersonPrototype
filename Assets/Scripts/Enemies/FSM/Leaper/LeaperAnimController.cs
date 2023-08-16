using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class LeaperAnimController : MonoBehaviour
{
    [SerializeField]
    private List<Animator> animators;
    [SerializeField]
    private EnemyAnimationController animController;
    // Start is called before the first frame update

    void Start()
    {

    }

    public void activation()
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("Activated", true);
        }
    }

    public void jumping()
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("IsJumping", true);
        }
        if (animController != null)
        {
            animController.disableIK();
        }
    }

    public void landing()
    {
        foreach (Animator anim in animators)
        {
            anim.SetBool("IsJumping", false);
        }
        if(animController != null)
        {
            animController.enableIK();
            animController.calculateArmPositions();
        }

    }
}
