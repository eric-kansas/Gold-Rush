
from sqlalchemy import Table, ForeignKey, Column
from sqlalchemy.types import Unicode, Integer, DateTime
from sqlalchemy.orm import relation, synonym

from tg2app.model import DeclarativeBase, metadata, DBSession
from tg2app.model.player import Player
from tg2app.model.game import Game

class Hand(DeclarativeBase):
    __tablename__ = 'tg_hand'

    #( Columns

    id = Column(Integer, autoincrement=True, primary_key=True)

    # A list of games in which I have the next turn
    cards = relation("Card", backref="hand")

    game_id = Column(Integer, ForeignKey(Game.id))
    player_id = Column(Integer, ForeignKey(Player.id))

    def to_json(self, no_relations=False):
        if no_relations:
            return {
                'id': self.id,
		  'game_id': self.game_id,
		  'player_id': self.player_id,
            }
        else:
            return {
                'id': self.id,
                'cards': [
                    card.to_json(no_relations=True) for card in self.cards
                ],
            }
