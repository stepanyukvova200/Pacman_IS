using UnityEngine;

public class PowerFruit : Fruit
{
    public float duration = 8f;

    protected override void Eat()
    {
        FindObjectOfType<GameManager>().PowerFruitEaten(this);
    }

}
