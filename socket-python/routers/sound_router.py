from fastapi import APIRouter
from fastapi import FastAPI, File, UploadFile, HTTPException, Form
from fastapi.responses import StreamingResponse

import io

router = APIRouter()


def process_file(input_file: io.BytesIO, input_format: str) -> io.BytesIO:
    return input_file


@router.post("/process_sound/")
async def process_sound(file: UploadFile = File(...), sound_id: str = Form(...)):
    try:
        input_format = file.content_type
        processed_file = process_file(file.file, input_format)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

    return StreamingResponse(io.BytesIO(processed_file.read()), media_type=input_format)
