from fastapi import APIRouter
from schemas.report_schema import WorkReport

router = APIRouter()


@router.post("/work-report")
async def create_work_report(report: WorkReport):

    return {"id": report.id, "text_response": report.text_response, "rating": report.rating}
