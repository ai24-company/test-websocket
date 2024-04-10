"""Callback handlers used in the app."""
from typing import Any, Dict, List

# from langchain.callbacks.base import AsyncCallbackHandler

from schemas.bot_schema import ChatResponse
import uuid
import random


class StreamingLLMCallbackHandler():
    """Callback handler for streaming LLM responses."""

    def __init__(self, websocket):
        self.websocket = websocket


    def random_choice(self):
        return random.choice(["message", "action-form"])


    async def on_llm_new_token(self, token: str, run_id: str = '', **kwargs: Any) -> None:

        resp = ChatResponse(sender="bot", message=token, type="stream", type_message=self.random_choice(),
                            id=str(run_id))
        await self.websocket.send_json(resp.dict())

    async def on_chat_model_start(self, *args, run_id: str = '', **kwargs):
        resp = ChatResponse(sender="bot", message='', type="start", type_message=self.random_choice(), id=str(run_id))
        await self.websocket.send_json(resp.dict())

    async def on_llm_end(self, *args, run_id: str = '', **kwargs):
        end_resp = ChatResponse(sender="bot", message="", type="end", type_message=self.random_choice(), id=str(run_id))
        await self.websocket.send_json(end_resp.dict())


class QuestionGenCallbackHandler():
    """Callback handler for question generation."""

    def __init__(self, websocket):
        self.websocket = websocket

    async def on_llm_start(
            self, serialized: Dict[str, Any], prompts: List[str], **kwargs: Any
    ) -> None:
        """Run when LLM starts running."""
        resp = ChatResponse(
            sender="bot", message="Synthesizing question...", type="info"
        )
        await self.websocket.send_json(resp.dict())