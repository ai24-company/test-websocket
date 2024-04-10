from pydantic import BaseModel, UUID4


class WorkReport(BaseModel):
    id: UUID4
    text_response: str
    rating: int
