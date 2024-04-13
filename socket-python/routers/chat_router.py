import uuid
import logging
import asyncio

from fastapi import WebSocket, WebSocketDisconnect, APIRouter
from lorem.text import TextLorem
from loguru import logger
from callback import StreamingLLMCallbackHandler
from schemas.bot_schema import ChatResponse

router = APIRouter()


async def generate_random_sentence():
    lorem = TextLorem(srange=(3, 45))
    sentence = lorem.sentence()
    return sentence.split()


async def agent_astep(callback_my):
    id_uuid = uuid.uuid4()
    await callback_my.on_chat_model_start(run_id=id_uuid)
    for i in await generate_random_sentence():
        await callback_my.on_llm_new_token(i, run_id=id_uuid)
        await asyncio.sleep(0.1)
        await callback_my.on_llm_new_token(' ', run_id=id_uuid)

    await callback_my.on_llm_end(run_id=id_uuid)


@router.websocket('/chat/')
async def websocket_endpoint(websocket: WebSocket, chat_person_id: str = None):
    await websocket.accept()
    logger.success(f"chat_person_id: {chat_person_id}")
    logger.info("websocket success")
    stream_handler = StreamingLLMCallbackHandler(websocket)

    while True:
        try:
            await agent_astep(callback_my=stream_handler)

        except (WebSocketDisconnect, RuntimeError):
            logging.info("websocket disconnect")
            break
