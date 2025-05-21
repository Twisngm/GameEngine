using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerSkill : MonoBehaviour
{
    [SerializeField] GameObject DaggerPrefeb;
    [SerializeField] float SkillColldown1;
    [SerializeField] float SkillColldown2;
    [SerializeField] float BulletSpeed;

    [SerializeField] bool Penetration;

    [SerializeField] Transform Attackpos;
    Transform DaggerTrans;

    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Bandit_short(InputAction.CallbackContext obj)
    {
         
    }
    public void Throw(InputAction.CallbackContext obj)
    {
        GameObject TDagger = Instantiate(DaggerPrefeb);
        TDagger.transform.position = new Vector2(Attackpos.position.x, Attackpos.position.y - 0.25f);

        Vector2 shotVec;
        if (GameObject.FindWithTag("Player").GetComponent<Player>().Fliper())
        {
            shotVec = Vector2.right * BulletSpeed;
            TDagger.GetComponent<Transform>().Rotate(new Vector3(0, 0, 0));
        }
        else
        {
            shotVec = Vector2.left * BulletSpeed;
            TDagger.GetComponent<Transform>().Rotate(new Vector3(0, 0, 180));
        }
        TDagger.GetComponent<Rigidbody2D>().linearVelocityX = shotVec.x * BulletSpeed;
        TDagger.GetComponent<Dagger>().setPen(Penetration);

        StopCoroutine("AttackCancel");
        StartCoroutine("AttackCancel");
    }
    IEnumerator AttackCancel()
    {
        animator.SetBool("Skill2", true);
        yield return new WaitForSeconds(0.15f);
        animator.SetBool("Skill2", false);
        yield break;
    }
}