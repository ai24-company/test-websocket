from typing import Optional, Union
from pydantic.v1 import BaseModel, validator


class ChatResponse(BaseModel):
    """Chat response schema."""

    sender: str
    message: str
    type: str
    id: Optional[Union[str, int, None]]
    type_message: str

    @classmethod
    @validator("sender")
    def validate_sender(cls, sender):
        if sender not in ["bot", "you"]:
            raise ValueError("sender must be bot or you")
        return sender

    @classmethod
    @validator("type")
    def validate_message_type(cls, message_type):
        if message_type not in ["start", "stream", "end", "error", "info"]:
            raise ValueError("type must be start, stream or end")
        return message_type
