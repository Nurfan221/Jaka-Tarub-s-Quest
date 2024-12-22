using UnityEngine;

public class PredatoryAnimal : AnimalBehavior
{
    public float attackRange = 5f;
    public int attackDamage = 20;

    private void Update()
    {
        Collider2D target = Physics2D.OverlapCircle(transform.position, attackRange);
        if (target != null && jenisHewan == Jenis.HewanBuas)
        {
            Attack(target.gameObject);
        }
    }

    private void Attack(GameObject target)
    {
        AnimalBehavior otherAnimal = target.GetComponent<AnimalBehavior>();
        if (otherAnimal != null)
        {
            Debug.Log($"{namaHewan} menyerang {otherAnimal.namaHewan}");
            otherAnimal.TakeDamage(attackDamage);
        }
    }
}
