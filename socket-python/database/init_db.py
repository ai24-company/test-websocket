from sqlalchemy import create_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker
from sqlalchemy.dialects import registry
import os

DATABASE_URL = os.getenv('DATABASE_URL',
                         os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'sql_app.db'))

SQLALCHEMY_DATABASE_URL = f"sqlite:///{DATABASE_URL}"
print(DATABASE_URL)

registry.register('snowflake', 'snowflake.sqlalchemy', 'dialect')

engine = create_engine(
    SQLALCHEMY_DATABASE_URL,
    connect_args={"check_same_thread": False}
)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

Base = declarative_base()


def init_db():
    Base.metadata.create_all(bind=engine)


def get_session():
    with SessionLocal() as session:
        yield session


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
