# -*- coding: utf-8 -*-
"""Setup the tg2app application"""

import logging
import transaction
from tg import config

def setup_schema(command, conf, vars):
    """Place any commands to setup tg2app here"""
    # Load the models

    # <websetup.websetup.schema.before.model.import>
    from tg2app import model
    # <websetup.websetup.schema.after.model.import>

    
    # <websetup.websetup.schema.before.metadata.create_all>
    print "Creating tables"
    model.metadata.create_all(bind=config['pylons.app_globals'].sa_engine)
    # <websetup.websetup.schema.after.metadata.create_all>
    transaction.commit()

    from migrate.versioning.shell import main
    from migrate.exceptions import DatabaseAlreadyControlledError
    try:
        main(argv=['version_control'], url=config['sqlalchemy.url'], repository='migration', name='migration')
    except DatabaseAlreadyControlledError:
        print 'Database already under version control'
