using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public Card Card;

    public bool isPlayerCard;

    public CardInfo Info;
    
    public GameCardChoice Movement;
    public CardAbility Ability;

    GameManagerScr gameManager;

    public void Init(Card card, bool IsPlayerCard)
    {
        Card = card;
        gameManager = GameManagerScr.Instance;
        isPlayerCard = IsPlayerCard;

        if (isPlayerCard)
        {
            Info.ShowCardInfo();
            //GetComponent<AttackedCard>().enabled = false;
        }
        else
            Info.HideCardInfo();
    }

    public void OnCast()
    {
        if (isPlayerCard)
        {
            gameManager.PlayerHandCards.Remove(this);
            gameManager.PlayerFieldCards.Add(this);
            gameManager.ReduceMana(true, Card.Manacost);
            gameManager.CheckCardsForAvaliability();
        }
        else
        {
            gameManager.EnemyHandCards.Remove(this);
            gameManager.EnemyFieldCards.Add(this);
            gameManager.ReduceMana(false, Card.Manacost);
            Info.ShowCardInfo();
        }

        Card.isPlaced = true;

        if (Card.HasAbility)
        {
            Ability.OnCast();
        }
    }

    public void BeforeAttack()
    {
        if (Card.HasAbility)
        {
            Ability.BeforeAttack();
        }
    }

    public void OnTakeDamage(CardController attacker = null)
    {
        CheckForAlive();
        Ability.OnDamageTake(attacker);

    }

    public void OnDamageDeal(CardController defender = null)
    {
        Card.TimesDealDamage++;
        Card.canAttack = false;
        Info.HighlightCard(false);

        if (Card.HasAbility)
        {
            Ability.OnDamageDeal(defender);
        }
    }


    public void CheckForAlive()
    {
        if (Card.isAlive)
            Info.RefreshData();
        else
            DestroyCard();
    }


    void DestroyCard()
    {
        //Movement.OnEndMove();

        RemoveCardFromList(gameManager.EnemyFieldCards);
        RemoveCardFromList(gameManager.PlayerFieldCards);
        RemoveCardFromList(gameManager.PlayerHandCards);
        RemoveCardFromList(gameManager.EnemyHandCards);

        Destroy(gameObject);
    }

    void RemoveCardFromList(List<CardController> list)
    {
        if (list.Exists(x => x == this))
            list.Remove(this);
    }



}
