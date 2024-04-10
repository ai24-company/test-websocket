import uvicorn

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from database.init_db import init_db
from routers.chat_router import router as chat_router
from routers.report_router import router as report_router
from routers.sound_router import router as sound_router
from core.config import settings

app = FastAPI()

origins = [settings.CLIENT_ORIGIN]


@app.on_event("startup")
async def on_startup():
    init_db()


app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(report_router, tags=['Report'], prefix='/api')
app.include_router(chat_router, tags=['Chat'], prefix='/api')
app.include_router(sound_router, tags=['Sound'], prefix='/api')


if __name__ == "__main__":
    uvicorn.run(app, host="localhost", port=8000)
