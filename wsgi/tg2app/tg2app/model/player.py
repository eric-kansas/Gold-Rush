# -*- coding: utf-8 -*-
"""Player model."""

from sqlalchemy import *
from sqlalchemy.orm import mapper, relation
from sqlalchemy import Table, ForeignKey, Column
from sqlalchemy.types import Integer, Unicode
from sqlalchemy.orm import relation, backref

from tg2app.model import DeclarativeBase, metadata, DBSession

friends_mapping = Table(
    'friends_mapping', metadata,
    Column('left_id', Integer,
           ForeignKey('player.id'), primary_key=True),
    Column('right_id', Integer,
           ForeignKey('player.id'), primary_key=True))

class Player(DeclarativeBase):
    __tablename__ = 'player'

    id = Column(Integer, primary_key=True)
    name = Column(Unicode(255), nullable=False)

    # A list of games in which I have the next turn
    next_turn_in = relation("Game", backref="whose_turn")

    # A list of hands in which I have its owning player
    hands = relation("Hand", backref="player")

    # A list of entities in which I have its owning player
    entities = relation("Entity", backref="player")

    def to_json(self, no_relations=False):
        if no_relations:
            return {
                'id': self.id,
                'name': self.name,
            }
        else:
            return {
                'id': self.id,
                'name': self.name,
                'friends': [
                    f.to_json(no_relations=True) for f in self.friends
                ],
#		  'hands': [
#                    hand.to_json(no_relations=True) for hand in self.hands
#                ]
            }


Player.__mapper__.add_property('friends', relation(
    Player,
    primaryjoin=Player.id==friends_mapping.c.left_id,
    secondaryjoin=friends_mapping.c.right_id==Player.id,
    secondary=friends_mapping,
    backref=backref('friends_backref')
))

