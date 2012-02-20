# -*- coding: utf-8 -*-
"""Setup the tg2app application"""

import logging
from tg import config
from tg2app import model

import transaction


def bootstrap(command, conf, vars):
    """Place any commands to setup tg2app here"""

    # <websetup.bootstrap.before.auth
    from sqlalchemy.exc import IntegrityError
    try:
        # Add three lulzy users.
        player1 = model.Player(
            name="Lulzy Guy"
        )
        model.DBSession.add(player1)
        player2 = model.Player(
            name="Lulzy Lady"
        )
        model.DBSession.add(player2)

        player3 = model.Player(
            name="Foo Manchu"
        )
        model.DBSession.add(player3)


        # Make friends!
        player1.friends.append(player2)
        player2.friends.append(player1)

        # Make more friends!
        player1.friends.append(player3)
        player3.friends.append(player1)

        cards = buildDeck()
        for card in cards:
            model.DBSession.add(card)

        game = model.Game()
        game.players.append(player1)
        game.players.append(player2)
        for card in cards:
            game.cards.append(card)

        #make hand
        hand1 = model.Hand()
        hand1.game_id = game
        hand1.player_id = player1
        model.DBSession.add(hand1)

        #make cards in hand
        hand1.cards.append(cards[1])

        game.whose_turn = player1

        entity1 = model.Entity(
            is_avatar = True,
            is_stake = False,
            row = 2,
            col = 3,
        )
        entity1.player = player1
        entity1.game = game

        entity2 = model.Entity(
            is_avatar = True,
            is_stake = False,
            row = 2,
            col = 3,
        )
        entity2.player = player2
        entity2.game = game

        model.DBSession.add(game)
        model.DBSession.add(entity1)
        model.DBSession.add(entity2)


        transaction.commit()

    except IntegrityError:
        print 'Warning, there was a problem adding your auth data, it may have already been added:'
        import traceback
        print traceback.format_exc()
        transaction.abort()
        print 'Continuing with bootstrapping...'
        

    # <websetup.bootstrap.after.auth>

def buildDeck():

    cards = []
    for i in range(4):
        for k in range(13):
            tempCard = model.Card(
                is_up = False,
                suit = i,
                kind = k
            )
            cards.append(tempCard)

    return cards
