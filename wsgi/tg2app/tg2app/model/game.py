
from sqlalchemy import Table, ForeignKey, Column
from sqlalchemy.types import Unicode, Integer, DateTime, Boolean
from sqlalchemy.orm import relation, synonym, backref

from tg2app.model import DeclarativeBase, metadata, DBSession

from tg2app.model.player import Player

players_to_game_mapping = Table(
    'players_to_game_mapping', metadata,
    Column('player_id', Integer,
           ForeignKey('player.id'), primary_key=True),
    Column('game_id', Integer,
           ForeignKey('game.id'), primary_key=True))

class Game(DeclarativeBase):
    __tablename__ = 'game'

    id = Column(Integer, primary_key=True)

    # Don't use this.  Just use 'whose_turn'.  It needs to be here, though.
    whose_turn_id = Column(Integer, ForeignKey(Player.id))

    # A list of games in which I have the next turn
    hands = relation("Hand", backref="game")

    entities = relation("Entity", backref="game")
    cards = relation("Card", backref="game")
    players = relation(
        "Player",
        secondary=players_to_game_mapping,
        backref="games",
    )

    def to_json(self, no_relations=False):
        if no_relations:
            return {
                'id': self.id,
            }
        else:
            return {
                'id': self.id,
                'whose_turn': self.whose_turn.to_json(True),
                'hands': [hand.to_json() for hand in self.hands],
#                'entities': [ent.to_json() for ent in self.entities],
#               'cards': [card.to_json() for card in self.cards], # works
#               'players': [player.to_json() for player in self.players],
            }
