using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAbility : MonoBehaviour
{
    public CardController cardControl;

    public GameObject Shield, Provocation;



    public int currentAttack;

    public void OnCast()
    {
        foreach (var ability in cardControl.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.SHIELD:
                    Shield.SetActive(true);
                    break;

                case Card.AbilityType.PROVOCATION:
                    Provocation.SetActive(true);
                    break;
                case Card.AbilityType.SNAKE_SKIN:
                    cardControl.Card.Health += (int)((double)cardControl.Card.Health * 0.5 + 0.9);
                    cardControl.Info.RefreshData();
                    break;
                case Card.AbilityType.BERSERK:
                    cardControl.Card.Health += (int)((double)cardControl.Card.Health * 0.2 + 0.9);
                    cardControl.Card.Attack += (int)((double)cardControl.Card.Attack * 3);
                    cardControl.Info.RefreshData();
                    break;
            }
        }
    }

    public void BeforeAttack()
    {
        foreach (var ability in cardControl.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.CRIT:
                    currentAttack = cardControl.Card.Attack;
                    double randomValue = UnityEngine.Random.value;
                    if (randomValue > 0.4)
                    {
                        cardControl.Card.Attack = (int)(cardControl.Card.Attack * 1.5);
                    }
                    else if (randomValue > 0.7)
                    {
                        cardControl.Card.Attack = (int)(cardControl.Card.Attack * 2);
                    }
                    break;
                case Card.AbilityType.AOE_CASTER:
                    currentAttack = cardControl.Card.Attack;
                    cardControl.Card.Attack = (int)(cardControl.Card.Attack / GameManagerScr.Instance.GetFieldList(cardControl.isPlayerCard).Count);
                    break;
            }
        }
    }

    public void OnDamageDeal(CardController defender = null)
    {
        foreach (var ability in cardControl.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.DOUBLE_ATTACK:
                    if (cardControl.Card.TimesDealDamage == 1)
                    {
                        cardControl.Card.canAttack = true;
                        cardControl.Card.isntAttacked = true;

                        if (cardControl.isPlayerCard)
                            cardControl.Info.HighlightCard(true);
                    }
                    break;
                case Card.AbilityType.VAMPIRISM:
                    cardControl.Card.Health += (int)((double)cardControl.Card.Attack * 0.1 + 0.9);
                    cardControl.Info.RefreshData();
                    break;
                case Card.AbilityType.CRIT:
                    cardControl.Card.Attack = currentAttack;
                    cardControl.Info.RefreshData();
                    break;
                case Card.AbilityType.SPLASH:
                    CardController randomCard = GameManagerScr.Instance.GetRandomCard(cardControl.isPlayerCard);
                    if (randomCard != null)
                    {
                        randomCard.Card.GetDamage(cardControl.Card.Attack);
                    }
                    break;
                case Card.AbilityType.KAMIKADZE:
                    cardControl.Card.Health = 0;
                    defender.Card.Health = 0;
                    cardControl.Info.RefreshData();
                    defender.Info.RefreshData();
                    break;
                case Card.AbilityType.AOE_CASTER:
                    List<CardController> enemyCards = GameManagerScr.Instance.GetFieldList(cardControl.isPlayerCard);
                    foreach(CardController enemyCard in enemyCards)
                    {
                        if (enemyCard != defender)
                        {
                            enemyCard.Card.GetDamage(cardControl.Card.Attack);
                            enemyCard.CheckForAlive();
                        }
                    }
                    cardControl.Card.Attack = currentAttack;
                    cardControl.Info.RefreshData();
                    break;

            }
        }
    }

    public void OnDamageTake(CardController attacker = null)
    {
        Shield.SetActive(false);

        foreach (var ability in cardControl.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.SHIELD:
                    Shield.SetActive(true);
                    break;
                case Card.AbilityType.RETURN_DAMAGE:
                    attacker.Card.Health -= (int)((double)attacker.Card.Attack * 0.25 + 0.9);
                    attacker.Info.RefreshData();
                    
                    break;
            }
        }
    }

    public void OnNewTurn()
    {
        cardControl.Card.TimesDealDamage = 0;

        foreach (var ability in cardControl.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.REGENERATION_EACH_TURN:
                    cardControl.Card.Health += (int)((double)cardControl.Card.Health * 0.1 + 0.9);
                    cardControl.Info.RefreshData();
                    break;

                case Card.AbilityType.TRANSFORMATION:
                    cardControl.Card.TransformationCounter += 1;
                    if (cardControl.Card.TransformationCounter == 4)
                    {
                        cardControl.Info.Picture.sprite = Resources.Load<Sprite>("Sprites/Cards/card-werewolf");
                        cardControl.Card.Attack += cardControl.Card.Attack;
                        cardControl.Card.Health += cardControl.Card.Health;
                        cardControl.Info.RefreshData();
                    }
                    break;
                case Card.AbilityType.ATTACK_BUFF:
                    cardControl.Card.CounterForBuff += 1;
                    if (cardControl.Card.CounterForBuff == 2)
                    {
                        List<CardController> fieldCards = GameManagerScr.Instance.GetFieldList(cardControl.isPlayerCard);
                        foreach (CardController card in fieldCards)
                        {
                            card.Card.Attack += (int)(card.Card.Attack * 0.2);
                            card.Info.RefreshData();
                        }
                        cardControl.Card.CounterForBuff = 0;
                    }
                    break;
                case Card.AbilityType.AOE_HEALING:
                    cardControl.Card.isntAttacked = false;
                    List<CardController> playerCards = GameManagerScr.Instance.GetFieldList(cardControl.isPlayerCard);
                    int manaCost = cardControl.isPlayerCard ? GameManagerScr.Instance.PlayerMana : GameManagerScr.Instance.EnemyMana;

                    if (manaCost > cardControl.Card.Manacost)
                    {
                        int healthBonus = (int)(cardControl.Card.Attack / playerCards.Count);
                        foreach (CardController card in playerCards)
                        {
                            card.Card.Health += healthBonus;
                            card.Info.RefreshData();
                        }

                        if (cardControl.isPlayerCard)
                            GameManagerScr.Instance.PlayerMana -= cardControl.Card.Manacost;
                        else
                            GameManagerScr.Instance.EnemyMana -= cardControl.Card.Manacost;
                    }
                    break;
                case Card.AbilityType.MANA_BUFF:
                    int manaBuff = GameManagerScr.Instance.GetFieldList(cardControl.isPlayerCard).Count;
                    if (cardControl.isPlayerCard)
                        GameManagerScr.Instance.PlayerMana += manaBuff;
                    else
                        GameManagerScr.Instance.EnemyMana += manaBuff;
                    break;
                case Card.AbilityType.SUMMON:
                    cardControl.Card.CounterForBuff += 1;
                    if (cardControl.Card.CounterForBuff == 3)
                    {
                        Card cardToCreate = new Card(
                            "Skeleton",
                            "Sprites/Cards/card-skeleton",
                            (int)(cardControl.Card.Attack * 0.2),
                            (int)(cardControl.Card.Health * 0.2),
                            1,
                            0,
                            0,
                            Card.AbilityType.NO_ABILITY,
                            Card.ElementType.Air
                        );
                        if (cardControl.isPlayerCard)
                            GameManagerScr.Instance.CreateCardPref(cardToCreate, GameManagerScr.Instance.PlayerHand);
                        else
                            GameManagerScr.Instance.CreateCardPref(cardToCreate, GameManagerScr.Instance.EnemyHand);
                        cardControl.Card.CounterForBuff = 0;
                    }
                    break;
            }
        }
    }
}
