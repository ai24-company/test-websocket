

> python3 -m venv env_test_chat
####
> source env_test_chat\Scripts\activate
#### *проверь может Script
#### после активации 
> pip install -r requirements.txt
####
> uvicorn main:app --host 0.0.0.0 --port 8000 --reload
