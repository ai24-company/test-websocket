from database.init_db import Base
from sqlalchemy import Column, String, text, Integer, DateTime, ForeignKey, Text, Table
from sqlalchemy.orm import relationship


class ChatPerson(Base):
    __tablename__ = "chat_persons"

    id = Column(Integer, primary_key=True, index=True)

    chat_person_name = Column(String)
    conversation_purpose = Column(String)
    chat_person_role = Column(String)
    company_name = Column(String)
    company_business = Column(String)
    company_values = Column(String)
    conversation_type = Column(String)

    created_at = Column(DateTime, server_default=text('(CURRENT_TIMESTAMP)'))
    updated_at = Column(DateTime, server_default=text('(CURRENT_TIMESTAMP)'))

    prompts = relationship("Prompt", back_populates="chat_person", cascade="all, delete-orphan")
    stage_talks = relationship("StageTalk", cascade="all, delete-orphan", back_populates="chat_person")


type_prompts_association = Table(
    'prompts_association_types',
    Base.metadata,
    Column('prompts_id', Integer, ForeignKey('prompts.id')),
    Column('type_prompts_id', Integer, ForeignKey('type_prompts.id'))
)


class Prompt(Base):
    __tablename__ = "prompts"

    id = Column(Integer, primary_key=True, index=True)
    prompt_text = Column(Text)
    chat_person_id = Column(Integer, ForeignKey('chat_persons.id'))

    chat_person = relationship("ChatPerson", back_populates="prompts")

    created_at = Column(DateTime, server_default=text('(CURRENT_TIMESTAMP)'))
    updated_at = Column(DateTime, server_default=text('(CURRENT_TIMESTAMP)'))

    type_prompt = relationship('TypePrompt', secondary=type_prompts_association, back_populates='prompts_data')


class TypePrompt(Base):
    __tablename__ = 'type_prompts'
    id = Column(Integer, primary_key=True)
    title = Column(String)
    prompts_data = relationship('Prompt', secondary=type_prompts_association, back_populates='type_prompt')


class StageTalk(Base):
    __tablename__ = 'stage_talks'
    id = Column(Integer, primary_key=True)
    number = Column(Integer)
    stage_tip_text = Column(Text)
    chat_person_id = Column(Integer, ForeignKey('chat_persons.id'))
    chat_person = relationship("ChatPerson", back_populates="stage_talks")
