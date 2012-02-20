from sqlalchemy import Table, ForeignKey, Column
from sqlalchemy.types import Unicode, Integer, DateTime, Boolean
from sqlalchemy.orm import relation, synonym

from tg2app.model import DeclarativeBase, metadata, DBSession
from tg2app.model.hand import Hand
from tg2app.model.game import Game

class Card(DeclarativeBase):
    __tablename__ = 'tg_card'

    #{ Columns

    card_id = Column(Integer, autoincrement=True, primary_key=True)

    is_up = Column(Boolean)

    #known_by_players = Column()

    suit = Column(Integer)

    kind = Column(Integer)

    hand_id = Column(Integer, ForeignKey(Hand.id))
    map_id = Column(Integer, ForeignKey(Game.id))

    def to_json(self, no_relations=False):
        if no_relations:
            return {
                'id': self.card_id,
                'is_up': self.is_up,
		  'suit': self.suit,
		  'kind': self.kind,
            }
        else:
            return {
                'id': self.card_id,
                'is_up': self.is_up,
		  'suit': self.suit,
		  'kind': self.kind,
		  'hand_id': self.hand_id,
		  'map_id': self.map_id,
            }
