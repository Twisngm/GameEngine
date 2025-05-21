using Unity.VisualScripting;
using UnityEngine;

public class Dagger : MonoBehaviour
{
    bool pen;
    public void setPen(bool xor)
    {
        pen = xor;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            if (pen)
            {
                // 몬스터 히트
            }
            else
            {
                // 몬스터 히트
                Destroy(gameObject);
            }
        }
        if (collision.gameObject.tag == "Map")
            Destroy(gameObject);
    }
}
