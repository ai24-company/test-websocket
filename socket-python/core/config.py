from pathlib import Path

from pydantic_settings import BaseSettings
from pydantic import EmailStr
import os


class Settings(BaseSettings):
    DATABASE_PORT: int
    POSTGRES_PASSWORD: str
    POSTGRES_USER: str
    POSTGRES_DB: str
    POSTGRES_HOST: str
    POSTGRES_HOSTNAME: str

    JWT_PUBLIC_KEY: str
    JWT_PRIVATE_KEY: str
    REFRESH_TOKEN_EXPIRES_IN: int
    ACCESS_TOKEN_EXPIRES_IN: int
    JWT_ALGORITHM: str
    CLIENT_ORIGIN: str

    VERIFICATION_SECRET: str

    EMAIL_HOST: str
    EMAIL_PORT: int
    EMAIL_USERNAME: str
    EMAIL_PASSWORD: str
    EMAIL_FROM: EmailStr



    class Config:
        env_file = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), '.env_settings')

print(os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), '.env_settings'))
assert os.path.isfile(os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), '.env_settings'))
BASE_DIR = Path(__file__).parent.parent.absolute()
settings = Settings()
