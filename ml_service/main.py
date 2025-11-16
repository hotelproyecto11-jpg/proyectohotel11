from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Optional, Dict, Any
import pandas as pd
import joblib
import os
from sklearn.ensemble import GradientBoostingRegressor

MODEL_PATH = "model.pkl"

app = FastAPI(title="Pricing ML Service")


class TrainRow(BaseModel):
    roomId: int
    basePrice: float
    hotelOccupancy: float
    dayOfWeek: int
    month: int
    isWeekend: bool
    capacity: Optional[int] = None
    hasSeaView: Optional[bool] = None
    price: float


class TrainRequest(BaseModel):
    rows: List[TrainRow]


class PredictRequest(BaseModel):
    roomId: int
    basePrice: float
    hotelOccupancy: float
    dayOfWeek: int
    month: int
    isWeekend: bool
    capacity: Optional[int] = None
    hasSeaView: Optional[bool] = None


class PredictResponse(BaseModel):
    predictedPrice: float
    modelVersion: Optional[str]


def features_from_row(d: Dict[str, Any]):
    return {
        'basePrice': d.get('basePrice', 0.0),
        'hotelOccupancy': d.get('hotelOccupancy', 0.0),
        'dayOfWeek': d.get('dayOfWeek', 0),
        'month': d.get('month', 0),
        'isWeekend': 1 if d.get('isWeekend') else 0,
        'capacity': d.get('capacity') or 1,
        'hasSeaView': 1 if d.get('hasSeaView') else 0
    }


@app.post('/train')
def train(req: TrainRequest):
    try:
        rows = [r.dict() for r in req.rows]
        df = pd.DataFrame([features_from_row(r) for r in rows])
        y = pd.Series([r['price'] for r in rows])

        model = GradientBoostingRegressor(n_estimators=100, learning_rate=0.1, max_depth=4)
        model.fit(df, y)
        joblib.dump(model, MODEL_PATH)
        return { 'status': 'ok', 'modelVersion': os.path.abspath(MODEL_PATH) }
    except Exception as ex:
        raise HTTPException(status_code=500, detail=str(ex))


@app.post('/predict', response_model=PredictResponse)
def predict(req: PredictRequest):
    if not os.path.exists(MODEL_PATH):
        raise HTTPException(status_code=500, detail='Model not trained')

    model = joblib.load(MODEL_PATH)
    feat = features_from_row(req.dict())
    df = pd.DataFrame([feat])
    pred = model.predict(df)[0]
    return PredictResponse(predictedPrice=float(pred), modelVersion=os.path.abspath(MODEL_PATH))


@app.get('/health')
def health():
    return { 'status': 'ok', 'modelExists': os.path.exists(MODEL_PATH) }
